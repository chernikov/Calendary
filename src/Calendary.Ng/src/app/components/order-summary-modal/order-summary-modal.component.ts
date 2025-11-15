import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { SummaryOrder } from '../../../models/summary-order';

@Component({
    selector: 'app-order-summary-modal',
    imports: [MatDialogModule],
    templateUrl: './order-summary-modal.component.html',
    styleUrl: './order-summary-modal.component.scss'
})
export class OrderSummaryModalComponent {
  constructor(
    public dialogRef: MatDialogRef<OrderSummaryModalComponent>,
    @Inject(MAT_DIALOG_DATA) public order: SummaryOrder
  ) {}

  onConfirm() {
    this.dialogRef.close('confirm');
  }

  onCancel() {
    this.dialogRef.close();
  }
}
