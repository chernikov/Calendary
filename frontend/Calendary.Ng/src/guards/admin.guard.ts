import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { TokenService } from '../services/token.service';
import { jwtDecode } from 'jwt-decode';


@Injectable({
  providedIn: 'root',
})
export class AdminGuard implements CanActivate {
  constructor(private tokenService: TokenService, 
    private router: Router) {}

  canActivate(): boolean {
    const token = this.tokenService.getToken();
    if (!token) {
      this.redirectToHome();
      return false;
    }

    const role = this.tokenService.getRole(token);
    if (role === 'Admin') {
      return true;
    } else {
      this.redirectToHome();
      return false;
    }
  }

  private redirectToHome() {
    this.router.navigate(['/']);
  }
}
