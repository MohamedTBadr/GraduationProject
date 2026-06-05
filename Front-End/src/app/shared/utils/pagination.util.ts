/** Build page number array with -1 as ellipsis markers. */
export function buildPageNumbers(currentPage: number, totalPages: number): number[] {
  const t = Math.max(1, totalPages);
  if (t <= 7) return Array.from({ length: t }, (_, i) => i + 1);
  const pages: number[] = [1];
  if (currentPage > 3) pages.push(-1);
  for (let i = Math.max(2, currentPage - 1); i <= Math.min(t - 1, currentPage + 1); i++) {
    pages.push(i);
  }
  if (currentPage < t - 2) pages.push(-1);
  pages.push(t);
  return pages;
}
