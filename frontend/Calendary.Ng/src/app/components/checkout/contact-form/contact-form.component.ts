import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-contact-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './contact-form.component.html',
  styleUrl: './contact-form.component.scss',
})
export class ContactFormComponent {
  @Input({ required: true }) form!: FormGroup;
  @Input() isEmailConfirmed = false;
  @Input() isPhoneConfirmed = false;
  @Input() isSubmitting = false;

  @Output() emailVerification = new EventEmitter<void>();
  @Output() phoneVerification = new EventEmitter<void>();
}
