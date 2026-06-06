import { Injectable, signal, computed } from '@angular/core';
import { ApiVendor, ApiProduct } from '../types/api.interfaces';

const VENDOR_STORAGE_KEY = 'eventora_compare_vendors';
const SERVICE_STORAGE_KEY = 'eventora_compare_services';

@Injectable({
  providedIn: 'root'
})
export class CompareService {
  private vendorList = signal<ApiVendor[]>(this.loadVendorsFromStorage());
  private serviceList = signal<ApiProduct[]>(this.loadServicesFromStorage());

  compareListItems = computed(() => this.vendorList());
  compareCount = computed(() => this.vendorList().length);

  serviceCompareItems = computed(() => this.serviceList());
  serviceCompareCount = computed(() => this.serviceList().length);

  toggleCompare(vendor: ApiVendor): { success: boolean; added?: boolean; message?: string } {
    const list = this.vendorList();
    const index = list.findIndex(v => v.id === vendor.id);

    if (index > -1) {
      this.persistVendors(list.filter(v => v.id !== vendor.id));
      return { success: true, added: false };
    }

    if (list.length >= 3) {
      return { success: false, message: 'Maximum 3 vendors for comparison' };
    }

    this.persistVendors([...list, vendor]);
    return { success: true, added: true };
  }

  toggleServiceCompare(service: ApiProduct): { success: boolean; added?: boolean; message?: string } {
    const list = this.serviceList();
    const index = list.findIndex(s => s.id === service.id);

    if (index > -1) {
      this.persistServices(list.filter(s => s.id !== service.id));
      return { success: true, added: false };
    }

    if (list.length >= 3) {
      return { success: false, message: 'Maximum 3 services for comparison' };
    }

    this.persistServices([...list, service]);
    return { success: true, added: true };
  }

  isInCompare(id: string): boolean {
    return this.vendorList().some(v => v.id === id);
  }

  isServiceInCompare(id: string): boolean {
    return this.serviceList().some(s => s.id === id);
  }

  clearCompare() {
    this.persistVendors([]);
  }

  clearServiceCompare() {
    this.persistServices([]);
  }

  private persistVendors(list: ApiVendor[]) {
    this.vendorList.set(list);
    this.saveToStorage(VENDOR_STORAGE_KEY, list);
  }

  private persistServices(list: ApiProduct[]) {
    this.serviceList.set(list);
    this.saveToStorage(SERVICE_STORAGE_KEY, list);
  }

  private saveToStorage(key: string, list: unknown[]) {
    try {
      if (list.length) {
        localStorage.setItem(key, JSON.stringify(list));
      } else {
        localStorage.removeItem(key);
      }
    } catch {
      // ignore quota / private mode
    }
  }

  private loadVendorsFromStorage(): ApiVendor[] {
    return this.loadFromStorage<ApiVendor>(VENDOR_STORAGE_KEY);
  }

  private loadServicesFromStorage(): ApiProduct[] {
    return this.loadFromStorage<ApiProduct>(SERVICE_STORAGE_KEY);
  }

  private loadFromStorage<T extends { id?: string }>(key: string): T[] {
    try {
      const raw = localStorage.getItem(key);
      if (!raw) return [];
      const parsed = JSON.parse(raw);
      return Array.isArray(parsed) ? parsed.filter(item => item?.id) : [];
    } catch {
      return [];
    }
  }
}
