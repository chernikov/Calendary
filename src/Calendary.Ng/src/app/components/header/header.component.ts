import { Component, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { TokenService } from '../../../services/token.service'; // Сервіс для роботи з токеном
import { CartService } from '../../../services/cart.service';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterModule, MatButtonModule, MatIconModule],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss'
})

export class HeaderComponent implements OnInit {
  isInited = false;
  isLoggedIn = false;
  email: string | null = null;

  cartCount = 0;
 
  constructor(
    private tokenService: TokenService,
    private cartService: CartService,
    public router: Router
  ) {
  }

  ngOnInit(): void {
    // Перевіряємо наявність токену при ініціалізації компонента
    const token = this.tokenService.getToken();

    if (token) {
      this.isLoggedIn = true;
      this.email = this.parseTokenEmail(token); // Парсимо email з токена
      this.isInited = true;
    }

    this.cartService.itemsInCart().subscribe((count) => {
      this.cartCount = count;
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
