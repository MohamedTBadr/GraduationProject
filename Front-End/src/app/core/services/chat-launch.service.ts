import { Injectable } from '@angular/core';

export interface PendingVendorChat {
  vendorId: string;
  vendorName?: string;
  initialMessage: string;
}

/** Holds a one-shot vendor chat payload across router navigation. */
@Injectable({ providedIn: 'root' })
export class ChatLaunchService {
  private pending: PendingVendorChat | null = null;

  setPending(vendorId: string, vendorName: string | undefined, initialMessage: string): void {
    this.pending = { vendorId, vendorName, initialMessage };
  }

  consumePending(): PendingVendorChat | null {
    const payload = this.pending;
    this.pending = null;
    return payload;
  }
}
