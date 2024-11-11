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
  imports: [CommonModule, FormsModule, MatFormFieldModule, MatInputModule, MatButtonModule],
  templateUrl: './verification-dialog.component.html',
  styleUrl: './verification-dialog.component.scss'
})
export class VerificationDialogComponent {
  verificationCode: string = '';
  errorMessage: string = ''; // Змінна для збереження помилок

  constructor(
    public dialogRef: MatDialogRef<VerificationDialogComponent>,
    private verificationService: VerificationService, // Сервіс для верифікації
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {}

  onNoClick(): void {
    this.dialogRef.close();
  }

  verifyCode(): void {
    this.verificationService.verifyEmailCode(this.verificationCode).subscribe(
      (response) => {
        // Якщо код підтверджено успішно, закриваємо модалку
        this.dialogRef.close(true);
      },
      (error) => {
        // Якщо код невірний, зберігаємо повідомлення про помилку
        this.errorMessage = error.error.message || "Помилка верифікації";
      }
    );
  }
}