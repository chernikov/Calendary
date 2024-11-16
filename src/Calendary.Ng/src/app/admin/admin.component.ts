import { Component } from '@angular/core';
import { MatSidenavModule } from '@angular/material/sidenav';
import { Router, RouterModule } from '@angular/router';
import { MenuComponent } from './components/menu/menu.component';
import { TokenService } from '../../services/token.service';
@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [MenuComponent, RouterModule, MatSidenavModule],
  templateUrl: "./admin.component.html",
  styleUrl: "./admin.component.scss",
})
export class AdminComponent {

  constructor(
    private tokenService : TokenService, 
    private router: Router) 
    { }
  logout(): void {
    this.tokenService.removeToken(); // Видаляємо токен або інші дані сесії
    this.router.navigate(['/login']); // Перенаправляємо на сторінку входу
  }
}