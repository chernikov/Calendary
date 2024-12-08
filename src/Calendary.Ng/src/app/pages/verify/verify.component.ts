import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { UserService } from '../../../services/user.service';
import { TokenService } from '../../../services/token.service';
import { NewPassword } from '../../../models/new-password';


@Component({
  selector: 'app-verify',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, MatCardModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatProgressSpinnerModule],
  templateUrl: './verify.component.html',
  styleUrls: ['./verify.component.scss']
})
export class VerifyComponent implements OnInit {
  isLoading = true;
  isInChangePassword = false;
  changePasswordForm: FormGroup;
  submitted = false;
  successMessage = '';
  errorMessage = '';

  constructor(
    private route: ActivatedRoute,
    private userService: UserService,
    private tokenService: TokenService,
    private fb: FormBuilder
  ) {
    this.changePasswordForm = this.fb.group(
      {
        newPassword: ['', [Validators.required]],
        confirmPassword: ['', [Validators.required]]
      },
      { validators: this.mustMatch('newPassword', 'confirmPassword') }
    );
  }

  
  ngOnInit(): void {
    const token = this.route.snapshot.paramMap.get('token');
    if (token) {
      this.userService.verify(token).subscribe({
        next: (user) => {
          this.tokenService.saveToken(user.token);
          this.isLoading = false;
          this.isInChangePassword = true;
        },
        error: (err) => {
          this.isLoading = false;
          this.errorMessage = 'Токен не вірний.';
        }
      });
    } else {
      this.isLoading = false;
      this.errorMessage = 'Токен відсутній.';
    }
  }

  
  mustMatch(password: string, confirmPassword: string) {
    return (formGroup: FormGroup) => {
      const passControl = formGroup.controls[password];
      const confirmControl = formGroup.controls[confirmPassword];
      if (confirmControl.errors && !confirmControl.errors['mustMatch']) {
        return;
      }
      if (passControl.value !== confirmControl.value) {
        confirmControl.setErrors({ mustMatch: true });
      } else {
        confirmControl.setErrors(null);
      }
    };
  }

  onSubmit() {
    this.submitted = true;

    if (this.changePasswordForm.invalid) {
      return;
    }
    const changePassword = new NewPassword();
    changePassword.newPassword = this.changePasswordForm.value.newPassword;
    this.errorMessage = "";
    this.userService.newPassword(changePassword).subscribe({
      next: (result) => {
        this.successMessage = result.message;
        this.changePasswordForm.reset();
        this.submitted = false;
      },
      error: (error) => {
        this.errorMessage = error.error.message;
      }
    });
   
  }
}