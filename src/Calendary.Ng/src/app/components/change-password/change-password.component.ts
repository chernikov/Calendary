import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { UserService } from '../../../services/user.service';
import { ChangePassword } from '../../../models/change-password';
import { MatInputModule } from '@angular/material/input';


@Component({
  selector: 'app-change-password',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, MatCardModule, MatFormFieldModule, MatInputModule, MatButtonModule],
  templateUrl: './change-password.component.html',
  styleUrls: ['./change-password.component.scss']
})
export class ChangePasswordComponent {
  changePasswordForm: FormGroup;
  submitted = false;
  successMessage = '';
  errorMessage = '';

  constructor(private fb: FormBuilder, 
    private userService: UserService) {
    this.changePasswordForm = this.fb.group(
      {
        currentPassword: ['', [Validators.required]],
        newPassword: ['', [Validators.required]],
        confirmPassword: ['', [Validators.required]]
      },
      { validators: this.mustMatch('newPassword', 'confirmPassword') }
    );
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
    const changePassword = new ChangePassword();
    changePassword.currentPassword = this.changePasswordForm.value.currentPassword;
    changePassword.newPassword = this.changePasswordForm.value.newPassword;
    this.errorMessage = "";
    this.userService.changePassword(changePassword).subscribe({
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