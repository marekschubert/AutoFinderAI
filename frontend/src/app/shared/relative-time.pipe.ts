import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'relativeTime', standalone: true, pure: true })
export class RelativeTimePipe implements PipeTransform {
  transform(value: string | Date | null | undefined): string {
    if (!value) {
      return '';
    }

    const date = typeof value === 'string' ? new Date(value) : value;
    const diffMs = Date.now() - date.getTime();
    const diffMinutes = Math.round(diffMs / 60000);

    if (diffMinutes < 1) {
      return 'przed chwilą';
    }
    if (diffMinutes < 60) {
      return `${diffMinutes} min temu`;
    }
    const diffHours = Math.round(diffMinutes / 60);
    if (diffHours < 24) {
      return `${diffHours} godz. temu`;
    }
    const diffDays = Math.round(diffHours / 24);
    if (diffDays < 30) {
      return `${diffDays} dni temu`;
    }
    return date.toLocaleDateString('pl-PL');
  }
}
