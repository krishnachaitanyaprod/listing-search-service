import { Pipe, PipeTransform } from '@angular/core';

const dayMs = 24 * 60 * 60 * 1000;

// "today", "1 day ago", "29 days ago". Counted against today's UTC date, like the API's recency factor.
@Pipe({ name: 'daysAgo' })
export class DaysAgoPipe implements PipeTransform {
  transform(isoDate: string): string {
    const now = new Date();
    const today = Date.UTC(now.getUTCFullYear(), now.getUTCMonth(), now.getUTCDate());
    const days = Math.max(0, Math.round((today - Date.parse(isoDate)) / dayMs));
    if (days === 0) return 'today';
    return days === 1 ? '1 day ago' : `${days} days ago`;
  }
}
