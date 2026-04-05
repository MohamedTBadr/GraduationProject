import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { MOCK_VENDORS } from '../../../shared/data/mock-vendors.data';
import { Vendor } from '../../../shared/types/vendor.interface';
import { ToastService } from '../../../shared/components/toast/toast.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss']
})
export class ProfileComponent implements OnInit {
  vendor!: Vendor;
  activeTab = 'info';

  constructor(
    private toastService: ToastService,
    private router: Router
  ) {}

  ngOnInit() {
    const v = MOCK_VENDORS.find(v => v.id === 1);
    if (v) {
      // Create a local copy to edit so changes aren't applied until "Save" is clicked
      this.vendor = JSON.parse(JSON.stringify(v));
    }
  }

  saveChanges() {
    const idx = MOCK_VENDORS.findIndex(v => v.id === 1);
    if (idx !== -1) {
      MOCK_VENDORS[idx] = JSON.parse(JSON.stringify(this.vendor));
      this.toastService.show('Profile updated successfully!', 'success');
      this.router.navigate(['/vendor/1']);
    }
  }
}
