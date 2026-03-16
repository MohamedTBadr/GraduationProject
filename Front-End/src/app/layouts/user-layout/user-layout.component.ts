import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { UserSidebarComponent } from './user-sidebar/user-sidebar.component';
import { UserTopbarComponent } from './user-topbar/user-topbar.component';

@Component({
  selector: 'app-user-layout',
  standalone: true,
  imports: [RouterOutlet, UserSidebarComponent, UserTopbarComponent],
  templateUrl: './user-layout.component.html',
  styleUrl: './user-layout.component.scss'
})
export class UserLayoutComponent {

}
