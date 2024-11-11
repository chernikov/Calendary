import { CommonModule } from '@angular/common';
import { MatDialog } from '@angular/material/dialog';
import { Component, EventEmitter, Output, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { UserService } from '../../../services/user.service';
import { VerificationService } from '../../../services/verification.service';
import { UserInfo } from '../../../models/user';
import { VerificationDialogComponent } from '../verification-dialog/verification-dialog.component';

@Component({
  selector: 'app-delivery',
  standalone : true, 
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './delivery.component.html',
  styleUrls: ['./delivery.component.scss']
})
export class DeliveryComponent implements OnInit {
  deliveryForm: FormGroup;
  isEmailValid = false;
  isPhoneValid = false;
  isEmailConfirmed = false;
  isPhoneConfirmed = false;
  errorMessage: string = '';


  @Output() deliveryInfo = new EventEmitter<any>();
  @Output() validStatus = new EventEmitter<{ isEmailValid: boolean; isPhoneValid: boolean }>();

  constructor(private formBuilder: FormBuilder, 
        private userService: UserService, 
        private VerificationService : VerificationService, 
        public dialog: MatDialog) {
    this.deliveryForm = this.formBuilder.group({
      city: ['', Validators.required],
      postOffice: ['', Validators.required],
      name: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', [Validators.required, Validators.pattern(/^(?:\+380|0)\s?(\d\s?){9}$/)]]
    });
  }

  ngOnInit(): void {
    this.initUserInfo();
  }

  initUserInfo() {
    this.userService.getInfo().subscribe(
      (data) => {
        this.deliveryForm.patchValue({
          name: data.userName,
          email: data.email,
          phone: data.phoneNumber
        });

        this.validateEmail();
        this.validatePhone();
        this.isEmailConfirmed = data.isEmailConfirmed;
        if (this.isEmailConfirmed) {
          this.deliveryForm.controls['email'].disable();
        }
        this.isPhoneConfirmed = data.isPhoneNumberConfirmed;
        if (this.isPhoneConfirmed) {
          this.deliveryForm.controls['phone'].disable();
        }
      },
      (error) => {
        console.log(error);
      }
    );
  }

  validateEmail() {
    this.isEmailValid = this.deliveryForm.controls['email'].valid;
    this.emitValidationStatus();
    if (this.isEmailValid) {
      this.updateInfo();
    }
  }

  validatePhone() {
    this.isPhoneValid = this.deliveryForm.controls['phone'].valid;
    this.emitValidationStatus();
    if (this.isPhoneValid) {
      this.updateInfo();
    }
  }

  updateInfo() {
    const userInfo = new UserInfo();
    userInfo.userName = this.deliveryForm.controls['name'].value;
    userInfo.email = this.deliveryForm.controls['email'].value;
    userInfo.phoneNumber = this.deliveryForm.controls['phone'].value;

    this.errorMessage = "";
    this.userService.updateInfo(userInfo).subscribe(
      (data) => {
        this.emitDeliveryInfo();
      },
      (error) => {
        this.errorMessage = error.error.message || "Помилка при оновленні інформації";
      }
    );
  }

  emitValidationStatus() {
    this.validStatus.emit({ isEmailValid: this.isEmailValid, isPhoneValid: this.isPhoneValid });
  }

  emitDeliveryInfo() {
    this.deliveryInfo.emit(this.deliveryForm.value);
  }

  showModalEmailCode() {
    this.VerificationService.sendEmailVerification().subscribe(
      (data) => {
        console.log(data);
        this.modalVerifyEmailCode();
        // Логіка для відкриття модального вікна підтвердження email
      }, 
      (error) => {
        console.log(error);
      });
  }

modalVerifyEmailCode() {
  const dialogRef = this.dialog.open(VerificationDialogComponent, {
    width: '400px',
    panelClass: 'custom-dialog-container',
    data: { email: this.deliveryForm.controls['email'].value }
  });

  dialogRef.afterClosed().subscribe(result => {
    if (result) {
      this.initUserInfo();
    }
  });
}
  
 
  showModalPhoneCode() {
    // Логіка для відкриття модального вікна підтвердження телефону
  }
}