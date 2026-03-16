import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { VendorSidebarComponent } from './vendor-sidebar/vendor-sidebar.component';
import { VendorTopbarComponent } from './vendor-topbar/vendor-topbar.component';

@Component({
  selector: 'app-vendor-layout',
  standalone: true,
  imports: [RouterOutlet, VendorSidebarComponent, VendorTopbarComponent],
  templateUrl: './vendor-layout.component.html',
  styleUrl: './vendor-layout.component.scss'
})
export class VendorLayoutComponent {

}
