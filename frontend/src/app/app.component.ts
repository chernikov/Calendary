import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { EmailConfirmBannerComponent } from './email-confirm-banner.component';
import { TopNavComponent } from './top-nav.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, EmailConfirmBannerComponent, TopNavComponent],
  template: `
    <app-email-confirm-banner />
    <app-top-nav />
    <router-outlet />
  `,
})
export class AppComponent {}
