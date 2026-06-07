import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

/** Notifies vendor layout widgets to reload pending booking counts. */
@Injectable({ providedIn: 'root' })
export class VendorBookingsRefreshService {
  private readonly refreshSource = new Subject<void>();
  readonly refresh$ = this.refreshSource.asObservable();

  notifyRefresh(): void {
    this.refreshSource.next();
  }
}
