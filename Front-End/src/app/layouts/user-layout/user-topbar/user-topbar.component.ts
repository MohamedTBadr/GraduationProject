import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';
@Component({
  selector: 'app-user-topbar',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './user-topbar.component.html',
  styleUrl: './user-topbar.component.scss'
})
export class UserTopbarComponent {
  constructor(public authService: AuthService) {}
  today: Date = new Date(); 
  
}
