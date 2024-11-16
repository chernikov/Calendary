import { Component, Inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';

@Component({
  selector: 'app-order-status-dialog',
  standalone: true,
  imports: [MatDialogModule, MatFormFieldModule, MatSelectModule, MatButtonModule],
  templateUrl: './order-status-dialog.component.html',
  styleUrl: './order-status-dialog.component.scss'
})
export class OrderStatusDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<OrderStatusDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { status: string }
  ) {}

  save(): void {
    this.dialogRef.close(this.data.status);
  }
}
