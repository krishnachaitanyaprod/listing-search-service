import { CurrencyPipe, DatePipe, DecimalPipe, KeyValuePipe, TitleCasePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { DaysAgoPipe } from './days-ago-pipe';
import { ListingSearchItem } from './listing-search.models';

const factorLabels: Record<string, string> = {
  budgetFit: 'Budget fit',
  recency: 'Recency'
};

@Component({
  selector: 'app-results-list',
  imports: [CurrencyPipe, DatePipe, DaysAgoPipe, DecimalPipe, KeyValuePipe, TitleCasePipe],
  templateUrl: './results-list.html',
  styleUrl: './results-list.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ResultsList {
  readonly items = input.required<ListingSearchItem[]>();

  // A factor added in the API still shows, under its own name.
  protected factorLabel(name: string): string {
    return factorLabels[name] ?? name;
  }
}
