import { Component, OnInit, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ToastComponent } from './shared/components/toast/toast.component';
import { ModalComponent } from './shared/components/modal/modal.component';
import { SignalRService } from './core/services/signalr.service';
import { AuthService } from './core/services/auth.service';
import { ThemeService } from './services/theme.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, ToastComponent, ModalComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit {

  title = 'Eventora';

  private signalRService = inject(SignalRService);
  private authService = inject(AuthService);
  private theme = inject(ThemeService);

  ngOnInit() {

    if (this.authService.isLoggedIn()) {
      this.signalRService.startConnections();
    }

    this.theme.initTheme();
  }
}