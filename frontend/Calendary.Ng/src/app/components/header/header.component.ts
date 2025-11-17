import { Component, OnDestroy, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { TokenService } from '../../../services/token.service'; // Сервіс для роботи з токеном
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Subject, takeUntil } from 'rxjs';
import { CartStore } from '../../store/cart.store';

@Component({
    selector: 'app-header',
    standalone: true,
    imports: [CommonModule, RouterModule, MatButtonModule, MatIconModule],
    templateUrl: './header.component.html',
    styleUrl: './header.component.scss'
})

export class HeaderComponent implements OnInit, OnDestroy {
  isInited = false;
  isLoggedIn = false;
  email: string | null = null;

  cartCount = 0;
  role: string = "";
  private destroy$ = new Subject<void>();

  constructor(
    private tokenService: TokenService,
    private readonly cartStore: CartStore,
    public router: Router
  ) {
  }

  ngOnInit(): void {
    // Перевіряємо наявність токену при ініціалізації компонента
    const token = this.tokenService.getToken();

    if (token) {
      this.isLoggedIn = true;
      this.role = this.tokenService.getRole(token); // Парсимо роль з токена
      this.email = this.parseTokenEmail(token); // Парсимо email з токена
      this.checkCart();
    } else {
      this.isInited = true;
    }

    this.cartStore.totalItems$
      .pipe(takeUntil(this.destroy$))
      .subscribe((count) => (this.cartCount = count));
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  checkCart() : void
  {
    this.cartStore.syncCartCount().subscribe({
      next: () => (this.isInited = true),
      error: (error) => {
        console.error('Error getting cart count:', error);
        this.isInited = true;
      },
    });
  }

  // Парсимо токен, щоб отримати email (можна використовувати бібліотеку jwt-decode)
  parseTokenEmail(token: string): string | null {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload.sub || null;
    } catch (error) {
      console.error('Error parsing token:', error);
      return null;
    }
  }

  logout(): void {
    this.tokenService.removeToken(); // Видаляємо токен
    this.isLoggedIn = false;
    window.location.href = "/"; // Перезавантажуємо сторінку
  }
}
