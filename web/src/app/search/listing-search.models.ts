// Matches the API contract in src/ListingSearch.Api/Contracts.

export type ListingStatus = 'active' | 'pending' | 'sold';

export interface ListingSearchItem {
  key: string;
  source: string;
  id: string;
  address: string;
  city: string;
  state: string;
  zip: string;
  price: number;
  bedrooms: number;
  bathrooms: number;
  sqft: number;
  listedDate: string;
  status: ListingStatus;
  description: string;
  score: number;
  // Each factor from 0 to 1, or null when it didn't apply (budgetFit without a target budget).
  factors: Record<string, number | null>;
}

export interface ListingSearchResponse {
  items: ListingSearchItem[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

// The filters a search sends. Empty ones are left out of the query string.
export interface ListingSearchCriteria {
  city?: string | null;
  keyword?: string | null;
  minPrice?: number | null;
  maxPrice?: number | null;
  minBedrooms?: number | null;
  targetBudget?: number | null;
  // Not a filter, but sent the same way; null means the API's default.
  pageSize?: number | null;
}

// Messages keyed by query parameter name, from a 400 ValidationProblem.
export type FieldErrors = Record<string, string[]>;
