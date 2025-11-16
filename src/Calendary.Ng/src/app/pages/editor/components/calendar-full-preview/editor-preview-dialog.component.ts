import { CommonModule } from '@angular/common';
import { Component, Inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatDialogModule, MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Calendar } from '../../../../../models/calendar';
import { MonthAssignment } from '../../models/calendar-assignment.model';
import { CalendarFullPreviewComponent } from './calendar-full-preview.component';
import { CalendarService } from '../../../../../services/calendar.service';
import { finalize } from 'rxjs/operators';

export interface EditorPreviewDialogData {
  calendar: Calendar | null;
  assignments: MonthAssignment[];
}

@Component({
  selector: 'app-editor-preview-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatButtonModule,
    MatSelectModule,
    MatInputModule,
    MatSnackBarModule,
    CalendarFullPreviewComponent,
  ],
  templateUrl: './editor-preview-dialog.component.html',
  styleUrl: './editor-preview-dialog.component.scss',
})
export class EditorPreviewDialogComponent {
  format: 'A4' | 'A3' = 'A4';
  paper: 'Matte' | 'Glossy' = 'Matte';
  quantity = 1;
  isSubmitting = false;

  constructor(
    private dialogRef: MatDialogRef<EditorPreviewDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: EditorPreviewDialogData,
    private calendarService: CalendarService,
    private snackBar: MatSnackBar
  ) {}

  get price(): number {
    const base = this.format === 'A3' ? 399 : 299;
    const paperMultiplier = this.paper === 'Glossy' ? 1.2 : 1;
    return Math.round(base * paperMultiplier * this.quantity);
  }

  get canAddToCart(): boolean {
    return !!this.data.calendar?.id && this.data.assignments.length === 12;
  }

  addToCart(): void {
    if (!this.canAddToCart || !this.data.calendar) {
      return;
    }

    this.isSubmitting = true;
    this.calendarService
      .addToCart(this.data.calendar)
      .pipe(finalize(() => (this.isSubmitting = false)))
      .subscribe({
        next: () => this.dialogRef.close('added-to-cart'),
        error: () =>
          this.snackBar.open('Не вдалося додати календар до кошика. Спробуйте пізніше', 'OK', {
            duration: 4000,
          }),
      });
  }
}
