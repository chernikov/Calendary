import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { EmailConfirmBannerComponent } from './email-confirm-banner.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, EmailConfirmBannerComponent],
  template: `
    <app-email-confirm-banner />
    <router-outlet />
  `,
})
export class AppComponent {}
