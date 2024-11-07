import { Component, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FaIconLibrary, FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { TokenService } from '../../../services/token.service'; // Сервіс для роботи з токеном
import { faSignIn, faSignOut, faUserAlt, faCartShopping } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, FontAwesomeModule, RouterModule],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss'
})

export class HeaderComponent implements OnInit {
  isLoggedIn = false;
  email: string | null = null;

  userIcon = faUserAlt;
  signInIcon = faSignIn;
  signOutIcon = faSignOut;
  cartIcon = faCartShopping;

  constructor(
    private tokenService: TokenService,
    public router: Router
  ) {
  }

  ngOnInit(): void {
    // Перевіряємо наявність токену при ініціалізації компонента
    const token = this.tokenService.getToken();

    if (token) {
      this.isLoggedIn = true;
      this.email = this.parseTokenEmail(token); // Парсимо email з токена
    }
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
