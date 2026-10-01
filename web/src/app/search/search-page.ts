import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Subject, catchError, map, of, switchMap } from 'rxjs';
import { FieldErrors, ListingSearchCriteria, ListingSearchResponse } from './listing-search.models';
import { ListingSearchService, fieldErrorsOf } from './listing-search.service';
import { Pager } from './pager';
import { ResultsList } from './results-list';
import { SearchForm, formFields } from './search-form';

type Outcome =
  | { kind: 'loaded'; response: ListingSearchResponse }
  | { kind: 'invalid'; errors: FieldErrors }
  | { kind: 'failed' };

interface SearchRequest {
  criteria: ListingSearchCriteria;
  page: number;
}

@Component({
  selector: 'app-search-page',
  imports: [SearchForm, ResultsList, Pager],
  templateUrl: './search-page.html',
  styleUrl: './search-page.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SearchPage {
  private readonly searchService = inject(ListingSearchService);
  private readonly requests = new Subject<SearchRequest>();

  // The last submitted filters. The pager pages through these, not through edits that weren't searched yet.
  private criteria: ListingSearchCriteria = {};

  protected readonly loading = signal(false);
  private readonly outcome = signal<Outcome | null>(null);

  protected readonly response = computed(() => {
    const outcome = this.outcome();
    return outcome?.kind === 'loaded' ? outcome.response : null;
  });
  protected readonly fieldErrors = computed(() => {
    const outcome = this.outcome();
    return outcome?.kind === 'invalid' ? outcome.errors : {};
  });
  protected readonly invalid = computed(() => this.outcome()?.kind === 'invalid');
  protected readonly failed = computed(() => this.outcome()?.kind === 'failed');

  // Errors on a field the form doesn't have (page, pageSize) are shown here instead.
  protected readonly otherErrors = computed(() =>
    Object.entries(this.fieldErrors())
      .filter(([field]) => !formFields.includes(field))
      .flatMap(([, messages]) => messages)
  );

  protected readonly statusText = computed(() => {
    const count = this.response()?.totalCount ?? 0;
    if (this.loading()) return 'Searching…';
    return count > 0 ? `${count} ${count === 1 ? 'listing matches' : 'listings match'}` : '';
  });

  constructor() {
    // switchMap drops the answer to an older request, so a slow response can't replace a newer one.
    this.requests
      .pipe(
        switchMap(({ criteria, page }) => {
          this.loading.set(true);
          return this.searchService.search(criteria, page).pipe(
            map((response): Outcome => ({ kind: 'loaded', response })),
            catchError((error: unknown) => {
              const errors = fieldErrorsOf(error);
              return of<Outcome>(errors ? { kind: 'invalid', errors } : { kind: 'failed' });
            })
          );
        }),
        takeUntilDestroyed()
      )
      .subscribe(outcome => {
        this.outcome.set(outcome);
        this.loading.set(false);
      });

    this.requests.next({ criteria: this.criteria, page: 1 });
  }

  // New filters always start again at page 1.
  protected search(criteria: ListingSearchCriteria): void {
    this.criteria = criteria;
    this.requests.next({ criteria, page: 1 });
  }

  protected goToPage(page: number): void {
    this.requests.next({ criteria: this.criteria, page });
  }
}
