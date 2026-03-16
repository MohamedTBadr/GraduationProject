import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-user-topbar',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './user-topbar.component.html',
  styleUrl: './user-topbar.component.scss'
})
export class UserTopbarComponent {

}
