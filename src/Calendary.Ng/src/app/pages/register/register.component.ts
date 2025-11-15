import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';

import { UserService } from '../../../services/user.service';
import {  UserRegister } from '../../../models/user';
import { TokenService } from '../../../services/token.service';
import { VerificationDialogComponent } from '../../components/verification-dialog/verification-dialog.component';
@Component({
    standalone: true,
    selector: 'app-register',
    imports: [ReactiveFormsModule, CommonModule, MatDialogModule, RouterModule],
    templateUrl: './register.component.html',
    styleUrl: './register.component.scss'
})
export class RegisterComponent {
  registerForm: FormGroup;
  submitted = false;
  errorMessage: string | null = null;
  isRegistered = false;

  constructor(
    private formBuilder: FormBuilder,
    private userService: UserService, 
    private tokenService : TokenService,
    public dialog: MatDialog,
    private router : Router
  ) {
    this.registerForm = this.formBuilder.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required]],
      confirmPassword: ['', [Validators.required]]
    }, {
      validator: this.mustMatch('password', 'confirmPassword')
    });
  }

  // Custom validator to check if passwords match
  mustMatch(password: string, confirmPassword: string) {
    return (formGroup: FormGroup) => {
      const control = formGroup.controls[password];
      const matchingControl = formGroup.controls[confirmPassword];

      if (matchingControl.errors && !matchingControl.errors['mustMatch']) {
        return;
      }

      if (control.value !== matchingControl.value) {
        matchingControl.setErrors({ mustMatch: true });
      } else {
        matchingControl.setErrors(null);
      }
    };
  }

  // Getter for easier access to form fields
  get f() {
    return this.registerForm.controls;
  }

  onSubmit() {
    this.submitted = true;

    // Stop if form is invalid
    if (this.registerForm.invalid) {
      return;
    }

    // Make the API call to register the user
    const user = new UserRegister();
    user.email = this.registerForm.value.email;
    user.password = this.registerForm.value.password;
    user.confirmPassword = this.registerForm.value.confirmPassword;

    // Викликаємо метод сервісу для реєстрації
    this.userService.register(user)
      .subscribe({
        next: (response) => {
          console.log('User registered successfully!', response);
          this.tokenService.saveToken(response.token);
          this.isRegistered = true;
          this.modalVerifyEmailCode();
        },
        error: (error) => {
          const message = error.error.message;
          this.errorMessage = message;
          console.error('Error registering user:', error);
        }
      });
  }

  
  modalVerifyEmailCode() {
    const dialogRef = this.dialog.open(VerificationDialogComponent, {
      width: '400px',
      panelClass: 'custom-dialog-container',
      data: { type: 'email' },
    });

    dialogRef.afterClosed().subscribe((result) => {

      if (result) {
        this.router.navigate(['/master']).then(() => {
          window.location.reload();
        });
      }
    });
  }

}
