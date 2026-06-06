import { ApiProduct } from '../types/api.interfaces';

/** Primary cover image for a product/service card. */
export function getProductCoverImage(product?: ApiProduct | null): string | null {
  if (!product) return null;
  if (product.imageUrl) return product.imageUrl;
  if (product.imageUrls?.length) return product.imageUrls[0];
  return null;
}

/** Extract image URL strings from a raw API service object. */
export function getServiceImagesFromRaw(service: any): string[] {
  const images = service?.serviceImages ?? service?.ServiceImages ?? [];
  if (!Array.isArray(images)) return [];
  return images
    .map((item: unknown) => {
      if (typeof item === 'string' && item) return item;
      if (item && typeof item === 'object') {
        const obj = item as Record<string, unknown>;
        const path = obj['imagePath'] ?? obj['ImagePath'] ?? obj['url'] ?? obj['Url'];
        return typeof path === 'string' ? path : '';
      }
      return '';
    })
    .filter((u): u is string => !!u);
}

/** All displayable image URLs for a product. */
export function getProductImageUrls(product?: ApiProduct | null): string[] {
  if (!product) return [];
  if (product.imageUrls?.length) return product.imageUrls.filter(u => !!u);
  if (product.imageUrl) {
    return product.imageUrl.split(',').map(s => s.trim()).filter(s => !!s);
  }
  return [];
}

/** Safe value for CSS background-image (handles spaces, parentheses in S3 filenames). */
export function cssBackgroundImage(url: string | null | undefined): string {
  if (!url) return 'none';
  const escaped = url.replace(/\\/g, '\\\\').replace(/"/g, '\\"');
  return `url("${escaped}")`;
}

/** Fetches a remote image URL and wraps it as a File (for re-submitting on service update). */
export async function urlToFile(url: string): Promise<File | null> {
  try {
    const response = await fetch(url, { mode: 'cors' });
    if (!response.ok) return null;
    const blob = await response.blob();
    const pathname = new URL(url).pathname;
    const name = decodeURIComponent(pathname.split('/').pop() || 'image.jpg');
    return new File([blob], name, { type: blob.type || 'image/jpeg' });
  } catch {
    return null;
  }
}
