import { Component, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { TokenService } from '../../../services/token.service'; // Сервіс для роботи з токеном
import { faSignIn, faSignOut, faUserAlt, faCartShopping } from '@fortawesome/free-solid-svg-icons';
import { CartService } from '../../../services/cart.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, FontAwesomeModule, RouterModule],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss'
})

export class HeaderComponent implements OnInit {
  isInited = false;
  isLoggedIn = false;
  email: string | null = null;

  cartCount = 0;
  userIcon = faUserAlt;
  signInIcon = faSignIn;
  signOutIcon = faSignOut;
  cartIcon = faCartShopping;

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
