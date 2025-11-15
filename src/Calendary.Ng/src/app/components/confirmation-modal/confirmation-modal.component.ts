import { Component } from '@angular/core';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';

@Component({
    selector: 'app-confirmation-modal',
    imports: [MatDialogModule],
    templateUrl: './confirmation-modal.component.html',
    styleUrl: './confirmation-modal.component.scss'
})
export class ConfirmationModalComponent {
  constructor(
    public dialogRef: MatDialogRef<ConfirmationModalComponent>
  ) {}

  onClose(): void {
    this.dialogRef.close();
  }
}
