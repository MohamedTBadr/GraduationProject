import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ThemeService } from '../../../services/theme.service';

@Component({
  selector: 'app-vendor-topbar',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './vendor-topbar.component.html',
  styleUrl: './vendor-topbar.component.scss'
})
export class VendorTopbarComponent {
  constructor(public theme: ThemeService) {}
}
