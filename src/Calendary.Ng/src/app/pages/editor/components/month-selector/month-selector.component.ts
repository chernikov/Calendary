import { CommonModule } from '@angular/common';
import { Component, Inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MonthAssignment, MONTHS } from '../../models/calendar-assignment.model';

export interface MonthSelectorImage {
  id: string;
  url: string;
}

export interface MonthSelectorData {
  month?: number;
  assignments?: MonthAssignment[];
  allowImageSelection?: boolean;
  images?: MonthSelectorImage[];
  selectedImageId?: string;
}

export interface MonthSelectorResult {
  month: number;
  imageId?: string;
}

@Component({
    selector: 'app-month-selector',
    standalone: true,
    imports: [CommonModule, MatDialogModule, MatFormFieldModule, MatSelectModule, MatButtonModule, MatIconModule],
    templateUrl: './month-selector.component.html',
    styleUrl: './month-selector.component.scss'
})
export class MonthSelectorComponent {
  months = MONTHS;
  selectedMonth: number | null = null;
  selectedImageId: string | null = null;

  constructor(
    private dialogRef: MatDialogRef<MonthSelectorComponent>,
    @Inject(MAT_DIALOG_DATA) public data: MonthSelectorData
  ) {
    this.selectedMonth = data?.month ?? null;
    this.selectedImageId = data?.selectedImageId ?? null;
  }

  get assignments(): MonthAssignment[] {
    return this.data.assignments || [];
  }

  get images(): MonthSelectorImage[] {
    return this.data.images || [];
  }

  onConfirm(): void {
    if (!this.selectedMonth) {
      return;
    }

    if (this.data.allowImageSelection && !this.selectedImageId) {
      return;
    }

    const result: MonthSelectorResult = {
      month: this.selectedMonth,
      imageId: this.data.allowImageSelection ? this.selectedImageId || undefined : this.data.selectedImageId,
    };

    this.dialogRef.close(result);
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onImagePicked(imageId: string): void {
    this.selectedImageId = imageId;
  }

  assignmentLabel(month: number): string | null {
    const assignment = this.assignments.find((item) => item.month === month);
    return assignment ? `— #${assignment.imageId}` : null;
  }
}
