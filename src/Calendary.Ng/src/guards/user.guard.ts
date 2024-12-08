import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { TokenService } from '../services/token.service';

@Injectable({
  providedIn: 'root',
})
export class UserGuard implements CanActivate {
  constructor(private tokenService: TokenService, 
    private router: Router) {}

  canActivate(): boolean {
    const token = this.tokenService.getToken();
    if (!token) {
      this.redirectRegister();
      return false;
    }
    const role = this.tokenService.getRole(token);
    if (role === 'User') {
      return true;
    } else {
      this.redirectRegister();
      return false;
    }
  }

  private redirectRegister() {
    this.router.navigate(['/register']);
  }
}
