export function formatHours(hours: number): string {
  return hours.toFixed(2);
}

export function fullName(firstName: string, lastName: string): string {
  return `${firstName} ${lastName}`;
}
