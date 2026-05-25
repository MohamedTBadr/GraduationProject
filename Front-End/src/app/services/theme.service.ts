import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {

  darkMode = signal(false);

  initTheme() {

    const saved = localStorage.getItem('theme');

    this.darkMode.set(saved === 'dark');

    document.documentElement.classList.toggle(
      'dark',
      this.darkMode()
    );
  }

  toggleTheme() {

    this.darkMode.update(v => !v);

    document.documentElement.classList.toggle(
      'dark',
      this.darkMode()
    );

    localStorage.setItem(
      'theme',
      this.darkMode() ? 'dark' : 'light'
    );
  }

  isDark() {
    return this.darkMode();
  }
}