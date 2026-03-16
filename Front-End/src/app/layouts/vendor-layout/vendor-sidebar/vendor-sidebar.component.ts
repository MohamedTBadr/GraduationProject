import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'app-vendor-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './vendor-sidebar.component.html',
  styleUrl: './vendor-sidebar.component.scss'
})
export class VendorSidebarComponent {

}
