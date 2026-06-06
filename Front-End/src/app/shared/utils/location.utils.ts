import {
  EGYPT_LOCATIONS,
  EgyptLocation,
  getLocationByCity
} from '../constants/egypt-locations';
import {
  AddressDto,
  ApiVendor,
  ServiceAreaDTO
} from '../types/api.interfaces';

/** Free text / alias → canonical location. */
export function resolveLocation(input: string | undefined | null): EgyptLocation | null {
  if (!input?.trim()) return null;
  const byCity = getLocationByCity(input);
  if (byCity) return byCity;

  const needle = input.trim().toLowerCase();
  for (const loc of EGYPT_LOCATIONS) {
    if (loc.governorate.toLowerCase() === needle) return loc;
    if (loc.aliases?.some((a) => a === needle || needle.includes(a) || a.includes(needle))) {
      return loc;
    }
  }
  return null;
}

/** Normalize service area city/region to catalog values when possible. */
export function normalizeServiceArea(area: ServiceAreaDTO): ServiceAreaDTO {
  const resolved =
    resolveLocation(area.city) ??
    resolveLocation(area.region) ??
    null;

  if (!resolved) {
    return {
      ...area,
      city: (area.city ?? '').trim(),
      region: (area.region ?? '').trim()
    };
  }

  return {
    ...area,
    city: resolved.city,
    region: resolved.governorate,
    latitude: area.latitude || resolved.latitude,
    longitude: area.longitude || resolved.longitude
  };
}

export function normalizeServiceAreas(
  areas: ServiceAreaDTO[] | undefined
): ServiceAreaDTO[] {
  if (!areas?.length) return [];
  return areas.map(normalizeServiceArea);
}

/** Match filter chip / picker value against service areas. */
export function matchesLocation(
  areas: ServiceAreaDTO[] | undefined,
  cityOrId: string
): boolean {
  if (!cityOrId?.trim()) return true;
  if (!areas?.length) return false;

  const filterLoc = resolveLocation(cityOrId);
  const needle = cityOrId.trim().toLowerCase();

  return areas.some((area) => {
    const normalized = normalizeServiceArea(area);
    const c = normalized.city.toLowerCase();
    const r = normalized.region.toLowerCase();

    if (filterLoc) {
      return (
        c === filterLoc.city.toLowerCase() ||
        r === filterLoc.governorate.toLowerCase() ||
        filterLoc.aliases?.some((a) => c.includes(a) || r.includes(a))
      );
    }

    return (
      c === needle ||
      r === needle ||
      c.includes(needle) ||
      r.includes(needle)
    );
  });
}

/** AddressDto → ServiceAreaDTO with lat/lng from catalog. */
export function addressToServiceArea(addr: Partial<AddressDto>): ServiceAreaDTO {
  const resolved =
    resolveLocation(addr.city) ??
    resolveLocation(addr.state) ??
    EGYPT_LOCATIONS[0];

  return {
    city: resolved.city,
    region: resolved.governorate,
    latitude: resolved.latitude,
    longitude: resolved.longitude
  };
}

/** Multiple coverage cities → service areas (deduped by city). */
export function citiesToServiceAreas(cityNames: string[]): ServiceAreaDTO[] {
  const seen = new Set<string>();
  const result: ServiceAreaDTO[] = [];

  for (const name of cityNames) {
    const loc = resolveLocation(name);
    if (!loc || seen.has(loc.id)) continue;
    seen.add(loc.id);
    result.push({
      city: loc.city,
      region: loc.governorate,
      latitude: loc.latitude,
      longitude: loc.longitude
    });
  }
  return result;
}

export function serviceAreasToLabel(areas: ServiceAreaDTO[] | undefined): string {
  if (!areas?.length) return '';
  const labels = areas.map((a) => {
    const n = normalizeServiceArea(a);
    return n.region && n.region !== n.city ? `${n.city}, ${n.region}` : n.city;
  });
  return [...new Set(labels)].join(' · ');
}

export function formatAddressLabel(addr: Partial<AddressDto> | undefined): string {
  if (!addr) return '';
  const parts = [addr.street, addr.city, addr.state].filter(
    (p) => p && String(p).trim() && String(p).trim() !== 'Not Specified'
  );
  return parts.join(', ');
}

/** Prefer service areas, else formatted address — never [object Object]. */
export function formatVendorLocation(vendor: Partial<ApiVendor> | null | undefined): string {
  if (!vendor) return '';

  const areasLabel = serviceAreasToLabel(vendor.serviceAreas);
  if (areasLabel) return areasLabel;

  const loc = vendor.location;
  if (typeof loc === 'string' && loc.trim()) return loc.trim();
  if (loc && typeof loc === 'object') {
    const addr = loc as Partial<AddressDto>;
    const formatted = formatAddressLabel(addr);
    if (formatted) return formatted;
  }

  return '';
}

export function formatEventLocation(
  location: Partial<AddressDto> | undefined
): string {
  if (!location) return '';
  const normalized = normalizeAddressFields(location.city ?? '', location.state ?? '');
  const parts = [
    location.street,
    normalized.city,
    normalized.state !== normalized.city ? normalized.state : ''
  ].filter((p) => p && String(p).trim() && String(p).trim() !== 'Unknown');
  return parts.join(', ');
}

/** Coerce picker / free-text to canonical city + governorate before API submit. */
export function normalizeAddressFields(
  city: string,
  state: string
): { city: string; state: string } {
  const fromCity = resolveLocation(city);
  const fromState = resolveLocation(state);

  if (fromCity) {
    return { city: fromCity.city, state: fromCity.governorate };
  }
  if (fromState) {
    return { city: fromState.city, state: fromState.governorate };
  }
  return { city: city.trim(), state: state.trim() };
}

export function normalizeAddressDto(
  addr: Partial<AddressDto> | undefined
): AddressDto | undefined {
  if (!addr) return undefined;
  const { city, state } = normalizeAddressFields(addr.city ?? '', addr.state ?? '');
  return {
    street: (addr.street ?? '').trim(),
    city,
    state,
    postalCode: addr.postalCode?.trim() || undefined
  };
}

/** Append ServiceAreas[i].* fields for ASP.NET FormData binding. */
export function appendServiceAreasToFormData(
  formData: FormData,
  areas: ServiceAreaDTO[]
): void {
  areas.forEach((area, i) => {
    formData.append(`ServiceAreas[${i}].City`, area.city);
    formData.append(`ServiceAreas[${i}].Region`, area.region);
    formData.append(`ServiceAreas[${i}].Latitude`, String(area.latitude));
    formData.append(`ServiceAreas[${i}].Longitude`, String(area.longitude));
  });
}
