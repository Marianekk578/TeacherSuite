/**
 * Formats a date string to a localized date format
 * @param dateString - ISO date string
 * @returns Formatted date string or 'N/A' if invalid
 */
export function formatDate(dateString: string | null | undefined): string {
  if (!dateString) return 'N/A';
  
  const date = new Date(dateString);
  
  if (isNaN(date.getTime())) {
    return 'Invalid Date';
  }
  
  return date.toLocaleDateString('en-US', { 
    year: 'numeric', 
    month: 'long', 
    day: 'numeric',
    timeZone: 'UTC'
  });
}

/**
 * Gets the current date in YYYY-MM-DD format
 * @returns Current date string
 */
export function getCurrentDate(): string {
  const today = new Date();
  const month = String(today.getMonth() + 1).padStart(2, '0');
  const day = String(today.getDate()).padStart(2, '0');
  return `${today.getFullYear()}-${month}-${day}`;
}

/**
 * Converts a date string to UTC ISO string format
 * @param dateString - Date string in YYYY-MM-DD format
 * @returns UTC ISO string
 */
export function convertToUtcIsoString(dateString: string): string {
  const date = new Date(dateString + 'T00:00:00.000Z');
  return date.toISOString();
}
