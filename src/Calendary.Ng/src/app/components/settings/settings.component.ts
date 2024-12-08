import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { UserService } from '../../../services/user.service';
import { UserInfo } from '../../../models/user';
import { VerificationDialogComponent } from '../verification-dialog/verification-dialog.component';
import { VerificationService } from '../../../services/verification.service';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.scss',
})
export class SettingsComponent implements OnInit {
  settingsForm: FormGroup;
  isEmailValid = false;
  isPhoneValid = false;
  isEmailConfirmed = false;
  isPhoneConfirmed = false;
  errorMessage: string = '';

  constructor(
    private formBuilder: FormBuilder,
    private userService: UserService,
    private VerificationService: VerificationService,
    public dialog: MatDialog
  ) {
    this.settingsForm = this.formBuilder.group({
      name: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      phone: [
        '',
        [
          Validators.required,
          Validators.pattern(/^(?:\+380|380|0)\s?(\d\s?){9}$/),
        ],
      ],
    });
  }

  ngOnInit(): void {
    this.initUserInfo();
  }

  initUserInfo() {
    this.userService.getInfo().subscribe(
      (data) => {
        if (data) {
          this.settingsForm.patchValue({
            name: data.userName,
            email: data.email,
            phone: data.phoneNumber,
          });
          this.validateEmail();
          this.validatePhone();
          this.isEmailConfirmed = data.isEmailConfirmed;
          if (this.isEmailConfirmed) {
            this.settingsForm.controls['email'].disable();
          }
          this.isPhoneConfirmed = data.isPhoneNumberConfirmed;
          if (this.isPhoneConfirmed) {
            this.settingsForm.controls['phone'].disable();
          }
        }
      },
      (error) => {
        console.log(error);
      }
    );
  }

  validateEmail() {
    this.updateInfo();
    this.isEmailValid = this.settingsForm.controls['email'].valid;
  }

  validatePhone() {
    this.updateInfo();
    this.isPhoneValid = this.settingsForm.controls['phone'].valid;
  }

  updateInfo() {
    const userInfo = new UserInfo();
    userInfo.userName = this.settingsForm.controls['name'].value;
    userInfo.email = this.settingsForm.controls['email'].value;
    userInfo.phoneNumber = this.settingsForm.controls['phone'].value;

    this.errorMessage = '';
    this.userService.updateInfo(userInfo).subscribe(
      (data) => {},
      (error) => {
        this.errorMessage =
          error.error.message || 'Помилка при оновленні інформації';
      }
    );
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
      }
    );
  }

  modalVerifyEmailCode() {
    const dialogRef = this.dialog.open(VerificationDialogComponent, {
      width: '400px',
      panelClass: 'custom-dialog-container',
      data: { type: 'email' },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.initUserInfo();
      }
    });
  }

  showModalPhoneCode() {
    // Логіка для відкриття модального вікна підтвердження телефону
    this.VerificationService.sendPhoneVerification().subscribe(
      (data) => {
        console.log(data);
        this.modalVerifyPhoneCode();
        // Логіка для відкриття модального вікна підтвердження телефону
      },
      (error) => {
        console.log(error);
      }
    );
  }

  modalVerifyPhoneCode() {
    const dialogRef = this.dialog.open(VerificationDialogComponent, {
      width: '400px',
      panelClass: 'custom-dialog-container',
      data: { type: 'phone' },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.initUserInfo();
      }
    });
  }
}
