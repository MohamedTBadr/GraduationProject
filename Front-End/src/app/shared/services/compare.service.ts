import { Injectable, signal, computed } from '@angular/core';
import { Vendor } from '../types/vendor.interface';

@Injectable({
  providedIn: 'root'
})
export class CompareService {
  private compareList = signal<Vendor[]>([]);
  compareListItems = computed(() => this.compareList());
  compareCount = computed(() => this.compareList().length);

  toggleCompare(vendor: Vendor): { success: boolean, added?: boolean, message?: string } {
    const list = this.compareList();
    const index = list.findIndex(v => v.id === vendor.id);

    if (index > -1) {
      this.compareList.set(list.filter(v => v.id !== vendor.id));
      return { success: true, added: false };
    } else {
      if (list.length >= 3) {
        return { success: false, message: 'Maximum 3 vendors for comparison' };
      }
      this.compareList.set([...list, vendor]);
      return { success: true, added: true };
    }
  }

  isInCompare(id: number): boolean {
    return this.compareList().some(v => v.id === id);
  }

  clearCompare() {
    this.compareList.set([]);
  }
}
