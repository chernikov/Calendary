import { Component, inject } from '@angular/core';
import { DOCUMENT } from '@angular/common';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { NzLayoutModule } from 'ng-zorro-antd/layout';
import { NzMenuModule } from 'ng-zorro-antd/menu';
import { NzIconModule } from 'ng-zorro-antd/icon';

const STYLESHEET_ID = 'ng-zorro-antd-styles';

@Component({
  selector: 'app-admin-shell',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, RouterOutlet, NzLayoutModule, NzMenuModule, NzIconModule],
  template: `
    <nz-layout style="min-height: 100vh;">
      <nz-sider nzWidth="220px" nzTheme="dark">
        <div style="color: #fff; font-weight: 600; font-size: 18px; padding: 16px;">Calendary Admin</div>
        <ul nz-menu nzTheme="dark" nzMode="inline">
          <li nz-menu-item routerLink="/admin/orders" routerLinkActive="ant-menu-item-selected">
            <span nz-icon nzType="shopping"></span>
            <span>Замовлення</span>
          </li>
          <li nz-menu-item routerLink="/admin/users" routerLinkActive="ant-menu-item-selected">
            <span nz-icon nzType="user"></span>
            <span>Користувачі</span>
          </li>
          <li nz-menu-item routerLink="/admin/settings" routerLinkActive="ant-menu-item-selected">
            <span nz-icon nzType="setting"></span>
            <span>Налаштування</span>
          </li>
        </ul>
      </nz-sider>
      <nz-layout>
        <nz-content style="padding: 24px;">
          <router-outlet></router-outlet>
        </nz-content>
      </nz-layout>
    </nz-layout>
  `,
})
export class AdminShellComponent {
  constructor() {
    // ng-zorro-antd's CSS is only needed inside /admin — loading it lazily here (rather than in
    // angular.json's global styles) keeps the customer-facing bundle unaffected by this
    // admin-only dependency.
    const document = inject(DOCUMENT);
    if (!document.getElementById(STYLESHEET_ID)) {
      const link = document.createElement('link');
      link.id = STYLESHEET_ID;
      link.rel = 'stylesheet';
      link.href = '/ng-zorro-antd.min.css';
      document.head.appendChild(link);
    }
  }
}
