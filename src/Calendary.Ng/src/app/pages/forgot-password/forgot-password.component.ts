import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { UserService } from '../../../services/user.service';
import { ForgotPassword } from '../../../models/forgot-password';

@Component({
  selector: 'app-forgot-password',
  standalone : true,
  imports: [ReactiveFormsModule, CommonModule, MatCardModule, MatFormFieldModule, MatInputModule, MatButtonModule],
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.scss']
})
export class ForgotPasswordComponent {
  forgotPasswordForm: FormGroup;
  submitted = false;
  successMessage = '';
  errorMessage = '';

  constructor(private fb: FormBuilder, private userService: UserService) {
    this.forgotPasswordForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });
  }

  onSubmit() {
    this.submitted = true;
    if (this.forgotPasswordForm.invalid) {
      return;
    }
    const forgotPassword = new ForgotPassword();
    forgotPassword.email = this.forgotPasswordForm.value.email;
    
    this.userService.sendResetLink(forgotPassword).subscribe({
      next: () => {
        this.successMessage = 'Посилання для відновлення паролю надіслано на вашу пошту!';
        this.errorMessage = '';
      },
      error: () => {
        this.errorMessage = 'Сталася помилка. Спробуйте ще раз.';
        this.successMessage = '';
      }
    });
  }
}