import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { UserService } from '../../services/user.service';
import { User } from '../../models/user';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
export class RegisterComponent {
  registerForm: FormGroup;
  submitted = false;
  errorMessage: string | null = null;

  constructor(
    private formBuilder: FormBuilder,
    private userService: UserService, 
    private router: Router
  ) {
    this.registerForm = this.formBuilder.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required]]
    });
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
    const user = new User(
      this.registerForm.value.email,
      this.registerForm.value.password
    );

    // Викликаємо метод сервісу для реєстрації
    this.userService.register(user)
      .subscribe({
        next: (response) => {
          console.log('User registered successfully!', response);
          this.router.navigate(['/']);
        },
        error: (error) => {
          console.error('Error registering user:', error);
          this.errorMessage = 'Failed to register user. Please try again.';
        }
      });
  }
}