import { ApiProduct } from '../types/api.interfaces';

/** Primary cover image for a product/service card. */
export function getProductCoverImage(product?: ApiProduct | null): string | null {
  if (!product) return null;
  if (product.imageUrl) return product.imageUrl;
  if (product.imageUrls?.length) return product.imageUrls[0];
  return null;
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
