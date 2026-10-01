import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { FieldErrors, ListingSearchCriteria, ListingSearchResponse } from './listing-search.models';

const searchUrl = '/api/v1/listings/search';

@Injectable({ providedIn: 'root' })
export class ListingSearchService {
  private readonly http = inject(HttpClient);

  search(criteria: ListingSearchCriteria, page: number): Observable<ListingSearchResponse> {
    return this.http.get<ListingSearchResponse>(searchUrl, { params: toQueryParams(criteria, page) });
  }
}

// The field errors of a 400 ValidationProblem, or null for any other failure.
export function fieldErrorsOf(error: unknown): FieldErrors | null {
  if (!(error instanceof HttpErrorResponse) || error.status !== 400) return null;
  const errors: unknown = error.error?.errors;
  return typeof errors === 'object' && errors !== null ? (errors as FieldErrors) : null;
}

// Sends only the filters that are filled in; the API treats a missing one as "no filter".
function toQueryParams(criteria: ListingSearchCriteria, page: number): HttpParams {
  let params = new HttpParams().set('page', page);
  for (const [name, value] of Object.entries(criteria)) {
    const text = value === null || value === undefined ? '' : String(value).trim();
    if (text !== '') params = params.set(name, text);
  }
  return params;
}
