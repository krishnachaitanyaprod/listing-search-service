# Listing Search Service

A full-stack app for searching property listings. An ASP.NET Core Web API (.NET 10) filters, scores and ranks the listings in `data/sample_listings.json`, and an Angular page searches them.

## How to run

**Prerequisites:** .NET 10 SDK and Node 24 (with npm).

Start the API from the repo root:

```bash
dotnet run --project src/ListingSearch.Api --launch-profile http
```

It listens on http://localhost:5194. Try http://localhost:5194/api/v1/listings/search?city=Springfield&targetBudget=450000.

- **Parameters** (all optional): `city`, `keyword`, `minPrice`, `maxPrice`, `minBedrooms`, `targetBudget`, `page`, `pageSize`.
- **Health:** `/health/live` and `/health/ready`.
- **OpenAPI:** `/openapi/v1.json` (Development only).

Start the web app in a second terminal:

```bash
cd web
npm ci
npm start
```

Then open **http://localhost:4200**. The dev server forwards `/api` to the API, so no CORS setup is needed.

Run the tests from the repo root:

```bash
dotnet test
```

That runs 175 tests:
- Unit tests for the rules.
- Integration tests that start the real API in memory, with "today" pinned to 2026-09-30.

**Turning on generated data:** pass a count after `--`, or set `GeneratedListings:Count` in `src/ListingSearch.Api/appsettings.json`:

```bash
dotnet run --project src/ListingSearch.Api --launch-profile http -- --GeneratedListings:Count=100000
```

Generated listings have the source `GEN` and use the sample cities. The same `Seed` and `AnchorDate` always give the same listings.

## Layout

- `src/ListingSearch.Core`: the rules (filters, scoring, ranking, paging and validation), with no ASP.NET and no file access.
- `src/ListingSearch.Infrastructure`: where listings come from (the JSON file and the generator), loaded once into an in-memory catalog at startup.
- `src/ListingSearch.Api`: HTTP only. The search endpoint, ProblemDetails errors, health checks, and settings checked at startup.
- `web/`: the Angular search page, with a form, a results list and a pager.
- `tests/`: unit tests for Core and Infrastructure, and integration tests that call the real API over HTTP.

## How I built it

The rules came first, written down with their reasons in [docs/decisions.md](docs/decisions.md). Then I built it in layers, in this order: the domain model, the filters, scoring, loading the data, the search service, the API, the web page, production basics, and finally scale.

Each backend step was test-first: I wrote the tests, saw them fail, then wrote the code. Each step is its own commit, so `git log --oneline` shows the build in order.

## Scoring

```
score = 100 × Σ(weight × factor) / Σ(weight)      over the factors that apply
```

- **Budget fit** applies only when `targetBudget` is given:
  - It's `max(0, 1 − distance / (0.25 × targetBudget))`.
  - Distance is how far under budget the price is, or **twice** how far over, because going over is penalised harder.
- **Recency** is `0.5 ^ (days since listed / 30)`, so it halves every 30 days.
- **Weights:** budget 0.7, recency 0.3. Without a target budget only recency applies, so the score is 100 × recency.
- **`targetBudget` only ranks.** It never removes a listing; use `maxPrice` for a hard limit.
- **Changing them:** the weights, the tolerance (0.25), the over-budget penalty (2) and the half-life (30 days) are in the `Scoring` section of `src/ListingSearch.Api/appsettings.json`. They're checked at startup, and a bad value stops the app with a message naming the rule.
- **Tie-break:** equal scores go to the lower price, then to the listing key (`source:id`) in ordinal order, so the order never changes between runs.

**Worked example:** A1, target budget 450,000, as of 2026-09-30.

- The price of 450,000 is exactly on budget, so budget fit is **1.00**.
- It was listed on 2026-08-29, 32 days earlier, so recency is 0.5^(32/30) ≈ **0.477** (shown as 0.48).
- Score = 100 × (0.7 × 1.00 + 0.3 × 0.477) / (0.7 + 0.3) = **84.3**, the top result.
- By contrast, A5 costs 470,000, which is 20,000 over budget. Its distance is 2 × 20,000 = 40,000, so its budget fit is 1 − 40,000 / 112,500 = **0.64**.

The full rules, each with its reason, are in [docs/decisions.md](docs/decisions.md).

## Invalid input

These return a 400 ProblemDetails response, with a message under each field and a `traceId`. All field errors come back together:

- `minPrice` greater than `maxPrice`
- A negative `minPrice`, `maxPrice` or `minBedrooms`
- A `targetBudget` of 0 or less
- `page` below 1 or past the last page
- `pageSize` outside 1 to 50
- A value that isn't a number, such as `minPrice=abc`
- A `city` or `keyword` longer than 100 characters
- A `city` that no listing has

An unknown city is an error because it's almost always a typo. If `Springfeld` returned an empty list, it would look like there are no homes in that city.

A known city where the other filters leave nothing, such as `city=Reston&maxPrice=400000`, is a real answer. It returns 200 with an empty list, and the page shows "No listings match your search."

Anything unexpected returns a 500 ProblemDetails with a `traceId` and no stack trace. The details are logged on the server.

## Trade-offs and limits

- **Keywords match words, not meaning.** "pet" matches any word that starts with "pet", so it also finds "No pets." Handling negation properly needs phrase matching or a search engine.
- **Cross-feed duplicates are shown.** The same home can appear in both feeds; for example, A1 and B7 are "123 Main St, Apt 4B" and "123 Main Street, Unit 4B". Addresses and zips differ between feeds, so there's no reliable rule for merging them.
- **The catalog is in memory.** Listings are loaded once at startup. A changed file needs a restart, and the data must fit in memory. That's fine at this size, but not for a live feed.
- **"Today" is the UTC date.** It's read once per search. On a US evening it's already tomorrow in UTC, so "days ago" and the scores can run a day ahead of local time.
- **There are no Angular tests.** The time went into the 175 API and rules tests. The web page was checked by hand and in a headless browser.

## Scale

**Measurement:** one search with `targetBudget=450000` over 100,000 generated listings (100,012 in total). Every listing matches, so all of them are ranked. Release build, median of 60 runs after 60 warm-ups, timed around the search itself.

- **Full sort:** 32 ms.
- **Top-k ranking:** 17 ms. A heap keeps only the best page × pageSize matches, and `totalCount` still counts all of them. Tests check that the results are exactly the same as a full sort.

What I'd do next:

- **Score less.** Every match is still scored, and each score builds a small factor breakdown. Building the breakdown only for the listings on the page is the next easy win.
- **Filter less.** Index listings by city and price, so most are skipped before scoring.
- **At real scale, move filtering and scoring closer to the data.** Put the data in a database or a search engine that filters, scores and pages there, use cursor-based paging for deep pages, and cache common searches.
