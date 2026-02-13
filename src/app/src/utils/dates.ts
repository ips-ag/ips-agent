import dayjs from 'dayjs';
import isoWeek from 'dayjs/plugin/isoWeek';

dayjs.extend(isoWeek);

export function getWeekStart(date?: string): string {
  const d = date ? dayjs(date) : dayjs();
  return d.startOf('isoWeek').format('YYYY-MM-DD');
}

export function getWeekDays(weekStart: string): string[] {
  const start = dayjs(weekStart);
  return Array.from({ length: 7 }, (_, i) => start.add(i, 'day').format('YYYY-MM-DD'));
}

export function formatDate(date: string): string {
  return dayjs(date).format('MMM D, YYYY');
}
