import { CommonModule } from '@angular/common';
import { Component, Inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { VerificationService } from '../../../services/verification.service';
@Component({
  selector: 'app-verification-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
  ],
  templateUrl: './verification-dialog.component.html',
  styleUrl: './verification-dialog.component.scss',
})
export class VerificationDialogComponent {
  verificationCode: string = '';
  errorMessage: string = ''; // Змінна для збереження помилок

  type: 'email' | 'phone' = 'email'; // Тип верифікації
  constructor(
    public dialogRef: MatDialogRef<VerificationDialogComponent>,
    private verificationService: VerificationService, // Сервіс для верифікації
    @Inject(MAT_DIALOG_DATA) public data: {type :  'email' | 'phone'}
  ) {
    debugger;
    this.type = data.type;
  }

  onNoClick(): void {
    this.dialogRef.close();
  }

  verifyCode(): void {
    if (this.type === 'email') {
      this.verifyCodeByEmail();
      return;
    }
    if (this.type === 'phone') {
      this.verifyCodeByPhone();
      return;
    }
    throw new Error('Invalid verification type');
  }

  verifyCodeByEmail() {
    this.verificationService.verifyEmailCode(this.verificationCode).subscribe(
      (response) => {
        // Якщо код підтверджено успішно, закриваємо модалку
        this.dialogRef.close(true);
      },
      (error) => {
        // Якщо код невірний, зберігаємо повідомлення про помилку
        this.errorMessage = error.error.message || 'Помилка верифікації';
      }
    );
  }

  verifyCodeByPhone() {
    this.verificationService.verifyPhoneCode(this.verificationCode).subscribe(
      (response) => {
        // Якщо код підтверджено успішно, закриваємо модалку
        this.dialogRef.close(true);
      },
      (error) => {
        // Якщо код невірний, зберігаємо повідомлення про помилку
        this.errorMessage = error.error.message || 'Помилка верифікації';
      }
    );
  }
}
