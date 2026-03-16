import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { FontAwesomeModule, FaIconLibrary } from '@fortawesome/angular-fontawesome';
import { faChartBar, faEdit, faBuilding, faUsers, faCalendarAlt, faBox, faMoneyBillWave, faCreditCard, faFileAlt, faTag, faTicketAlt, faCrown, faChartPie, faLifeRing } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-admin-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, FontAwesomeModule],
  templateUrl: './admin-sidebar.component.html',
  styleUrl: './admin-sidebar.component.scss'
})
export class AdminSidebarComponent {
  constructor(private library: FaIconLibrary) {
    library.addIcons(
      faChartBar,
      faEdit,
      faBuilding,
      faUsers,
      faCalendarAlt,
      faBox,
      faMoneyBillWave,
      faCreditCard,
      faFileAlt,
      faTag,
      faTicketAlt,
      faCrown,
      faChartPie,
      faLifeRing
    );
  }
}
