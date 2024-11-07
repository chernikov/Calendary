import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Output } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-delivery',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './delivery.component.html',
  styleUrl: './delivery.component.scss'
})
export class DeliveryComponent {
  delivery = { city: '', postOffice: '', name: '', email: '', phone: '' };
  isEmailValid = false;
  isPhoneValid = false;

  @Output() deliveryInfo = new EventEmitter<any>();
  @Output() validStatus = new EventEmitter<{ isEmailValid: boolean; isPhoneValid: boolean }>();

  validateEmail(emailInput: any) {
    this.isEmailValid = emailInput.valid;
    this.emitValidationStatus();
  }

  validatePhone(phoneInput: any) {
    this.isPhoneValid = phoneInput.valid;
    this.emitValidationStatus();
  }

  emitValidationStatus() {
    this.validStatus.emit({ isEmailValid: this.isEmailValid, isPhoneValid: this.isPhoneValid });
  }

  emitDeliveryInfo() {
    this.deliveryInfo.emit(this.delivery);
  }
}