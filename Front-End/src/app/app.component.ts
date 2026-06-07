import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { Router, RouterOutlet, NavigationEnd } from '@angular/router';
import { filter, Subscription } from 'rxjs';
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
export class AppComponent implements OnInit, OnDestroy {

  title = 'Eventora';

  private signalRService = inject(SignalRService);
  private authService = inject(AuthService);
  private theme = inject(ThemeService);
  private router = inject(Router);
  private navSub?: Subscription;

  ngOnInit() {
    this.navSub = this.router.events.pipe(
      filter((e): e is NavigationEnd => e instanceof NavigationEnd)
    ).subscribe(() => window.scrollTo({ top: 0, left: 0, behavior: 'instant' as ScrollBehavior }));

    if (this.authService.isLoggedIn()) {
      this.signalRService.sessionBootstrap();
    }

    this.theme.initTheme();
  }

  ngOnDestroy() {
    this.navSub?.unsubscribe();
  }
}