import { Injectable, signal } from '@angular/core';

export type ModalType = 'login' | 'signup' | 'enter-event-details' | 'choose-event' | 'service-detail' | 'package-detail' | 'add-event' | 'quick-action' | 'review-vendor' | 'confirm' | 'add-package' | 'edit-package' | 'add-vendor' | 'schedule-report' | 'add-banner' | 'manage-taxonomy' | null;

@Injectable({
  providedIn: 'root'
})
export class ModalService {
  private activeModalSignal = signal<ModalType>(null);
  private modalDataSignal = signal<any>(null);

  activeModal = this.activeModalSignal.asReadonly();
  modalData = this.modalDataSignal.asReadonly();

  open(type: ModalType, data: any = null) {
    this.activeModalSignal.set(type);
    this.modalDataSignal.set(data);
    document.body.style.overflow = 'hidden';
  }

  close() {
    this.activeModalSignal.set(null);
    this.modalDataSignal.set(null);
    document.body.style.overflow = '';
  }
}
