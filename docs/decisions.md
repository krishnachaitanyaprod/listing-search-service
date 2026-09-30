# Search rules and decisions

The rules the search follows, each with the reason it was chosen. Settings named here live in `appsettings.json`.

## Scoring

Score = `100 × Σ(weight × value) / Σ(weight)`, taken over the factors that apply. It's shown with one decimal; ranking uses the unrounded value.

- Budget fit applies only when `targetBudget` is given.
- Recency always applies.
- Defaults: BudgetWeight 0.7, RecencyWeight 0.3.

**Why:** the budget is what the user typed, so it leads, and recency separates listings that fit the budget about equally well. Dividing by the sum of the weights means they don't have to add up to exactly 1, which is a fragile check on floating-point numbers. It also means a factor added later doesn't force re-balancing the others.

## Budget fit

```
budgetFit = max(0, 1 − distance / (BudgetTolerance × targetBudget))
distance  = targetBudget − price                        when price ≤ targetBudget
          = OverBudgetPenalty × (price − targetBudget)  when price > targetBudget
```

Defaults: BudgetTolerance 0.25, OverBudgetPenalty 2. A listing 25% under budget, or 12.5% over, gets a budget fit of 0. It can still score on recency.

**Why:** without a tolerance, a listing 10% off budget still scores 0.9, and budget stops separating listings. Going over is penalised harder because a buyer can't stretch a budget as easily as they can save. A penalty below 1 would reward going over, so it must be at least 1.

## Recency and "today"

`recency = 0.5 ^ (ageDays / RecencyHalfLifeDays)`. Default half-life: 30 days.

- "Today" is the current UTC date from `TimeProvider`. Tests pin it.
- `ageDays` is the number of whole days from `listedDate` to today.
- A listing dated in the future counts as 0 days old.

**Why:** "halves every 30 days" is easy to explain. There's no cutoff, so older listings still sort.

## Missing targetBudget

Only recency applies, so the scoring formula gives `100 × recency`.

**Why:** search still works without a budget, no target gets made up, and no special case is needed.

## Keyword

- Matching ignores case.
- The description is split into words at any character that isn't a letter or digit.
- A search term matches a word that starts with it.
- With several terms, every term must match.
- An empty keyword means no filter.

**Why:** "pet" finds "Pets" but not "carpet", and extra terms narrow the results like a normal search box.

Known limit: "pet" also matches "no pets".

## City

Exact match after trimming, collapsing inner spaces, and ignoring case.

**Why:** predictable, and it handles the case and spacing differences between feeds.

## Price and bedroom filters

`minPrice`, `maxPrice` and `minBedrooms` are inclusive: `minPrice` 450,000 includes a listing priced 450,000. Price is `decimal`.

**Why:** inclusive is what a user expects, and `decimal` keeps money exact.

## Listing key

A listing is identified by `source:id`, for example `MLS_A:A1`.

**Why:** `id` is only unique within one source.

## Invalid input (400)

Each case below returns a 400 ProblemDetails response with an error on the named field:

- `minPrice` greater than `maxPrice`
- A negative `minPrice`, `maxPrice` or `minBedrooms`
- `targetBudget` of 0 or less
- `pageSize` below 1 or above MaxPageSize
- `page` below 1
- `page` past the last page. The one exception is page 1 when there are no results, which returns an empty result.
- A `city` that no listing has
- A numeric parameter that isn't a number, for example `minPrice=abc`
- A `city` or `keyword` longer than 100 characters

**Why:** the handout asks for a clear error rather than a crash, a silent empty result, or wrong data.

## City with no results

- A city that no listing has is a 400 error (see above).
- A known city where the other filters leave nothing returns 200 with an empty result, and the UI shows its no-results state.

**Why:** a typo like "Springfeld" gets flagged instead of looking like an empty market.

## Paging

- Pages are numbered from 1.
- Defaults: DefaultPageSize 10, MaxPageSize 50.
- The UI goes back to page 1 on every new search.

**Why:** the 12 sample listings make 2 pages, so paging is visible without generated data.

## Tie-break

Equal unrounded scores are ordered by lower price first, then by key in ordinal order.

**Why:** the cheaper listing winning a tie makes sense to a buyer, and the key makes the order fully deterministic.

## Status

All statuses are returned, and each result shows its status.

**Why:** no hidden rule, and the handout doesn't ask for a status filter.

## Cross-feed duplicates

Duplicates are shown as they are. This is a known limitation. The sample has four pairs: A1/B7, A2/B8, A3/B9 and A5/B11.

**Why:** the data doesn't support a reliable matching rule. Addresses are written differently ("Apt 4B" vs "Unit 4B") and zips differ (22150 vs 22151).

## Settings rules

Checked at startup. Invalid values stop the app with a clear message.

| Setting | Default | Rule |
|---|---|---|
| `Scoring:BudgetWeight` | 0.7 | ≥ 0; at least one weight > 0 |
| `Scoring:RecencyWeight` | 0.3 | ≥ 0; at least one weight > 0 |
| `Scoring:BudgetTolerance` | 0.25 | > 0 |
| `Scoring:OverBudgetPenalty` | 2 | ≥ 1 |
| `Scoring:RecencyHalfLifeDays` | 30 | > 0 |
| `Paging:DefaultPageSize` | 10 | > 0 and ≤ MaxPageSize |
| `Paging:MaxPageSize` | 50 | > 0 |

## Worked example

These numbers are as of 2026-09-30, with targetBudget 450,000 and the default settings.

| Rank | Listing | Price | Listed | Age (days) | Budget fit | Recency | Score |
|---|---|---|---|---|---|---|---|
| 1 | A1 | 450,000 | 2026-08-29 | 32 | 1.00 | 0.48 | 84.3 |
| 2 | A5 | 470,000 | 2026-09-04 | 26 | 0.64 | 0.55 | 61.6 |
| 3 | A6 | 415,000 | 2026-08-10 | 51 | 0.69 | 0.31 | 57.5 |
| 4 | A3 | 399,000 | 2026-09-01 | 29 | 0.55 | 0.51 | 53.6 |

- **A6 above A3:** A6 is closer to the budget (fit 0.69 vs 0.55), and that outweighs A3 being three weeks newer.
- **A5:** it is 4.4% over budget. The penalty cuts its fit from 0.82 to 0.64, and its score from 74.0 to 61.6.
