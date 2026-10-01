import { ChangeDetectionStrategy, Component, inject, input, output } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule } from '@angular/forms';
import { FieldErrors, ListingSearchCriteria } from './listing-search.models';

interface Field {
  name: keyof ListingSearchCriteria;
  label: string;
  placeholder: string;
  step?: number;
}

// The names the form shows errors for; a 400 on any other field is shown by the page.
export const formFields: readonly string[] = ['city', 'keyword', 'minPrice', 'maxPrice', 'minBedrooms', 'targetBudget'];

@Component({
  selector: 'app-search-form',
  imports: [ReactiveFormsModule],
  templateUrl: './search-form.html',
  styleUrl: './search-form.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SearchForm {
  readonly fieldErrors = input<FieldErrors>({});
  readonly search = output<ListingSearchCriteria>();

  protected readonly form = inject(NonNullableFormBuilder).group({
    city: '',
    keyword: '',
    minPrice: null as number | null,
    maxPrice: null as number | null,
    minBedrooms: null as number | null,
    targetBudget: null as number | null
  });

  protected readonly textFields: Field[] = [
    { name: 'city', label: 'City', placeholder: 'Any city' },
    { name: 'keyword', label: 'Keyword', placeholder: 'e.g. garage' }
  ];

  protected readonly numberFields: Field[] = [
    { name: 'minPrice', label: 'Min price', placeholder: 'Any', step: 1000 },
    { name: 'maxPrice', label: 'Max price', placeholder: 'Any', step: 1000 },
    { name: 'minBedrooms', label: 'Min bedrooms', placeholder: 'Any', step: 1 },
    { name: 'targetBudget', label: 'Target budget', placeholder: 'None', step: 1000 }
  ];

  protected errorsFor(field: string): string[] {
    return this.fieldErrors()[field] ?? [];
  }

  protected submit(): void {
    this.search.emit(this.form.getRawValue());
  }

  protected clear(): void {
    this.form.reset();
    this.submit();
  }
}
