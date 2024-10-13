import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { UserService } from '../../../services/user.service';
import { User } from '../../../models/user';
import { TokenService } from '../../../services/token.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {
  loginForm: FormGroup;
  submitted = false;
  errorMessage: string | null = null;

  constructor(
    private formBuilder: FormBuilder,
    private userService: UserService,
    private tokenService: TokenService,
    private router: Router
  ) {
    this.loginForm = this.formBuilder.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required]]
    });
  }

  // Getter для зручного доступу до полів форми
  get f() {
    return this.loginForm.controls;
  }

  onSubmit() {
    this.submitted = true;

    // Зупиняємо, якщо форма невалідна
    if (this.loginForm.invalid) {
      return;
    }

    // Викликаємо сервіс для логіну
    const user = new User(
      this.loginForm.value.email,
      this.loginForm.value.password
    );

    this.userService.login(user)
      .subscribe({
        next: (response) => {
          console.log('User logged in successfully!', response);
          this.tokenService.saveToken(response.token);
          this.router.navigate(['/']).then(() => {
            window.location.reload();
          });
        },
        error: (error) => {
          console.error('Error logging in user:', error);
          this.errorMessage = 'Failed to log in. Please check your credentials.';
        }
      });
  }
}