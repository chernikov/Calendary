import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MonthAssignment, MONTHS } from '../../models/calendar-assignment.model';

@Component({
  selector: 'app-calendar-preview',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatButtonModule, MatTooltipModule],
    templateUrl: './calendar-preview.component.html',
    styleUrl: './calendar-preview.component.scss'
})
export class CalendarPreviewComponent {
  @Input() assignments: MonthAssignment[] = [];
  @Input() duplicateImageIds: string[] = [];
  @Output() monthSelected = new EventEmitter<number>();
  @Output() clearMonth = new EventEmitter<number>();
  @Output() clearAll = new EventEmitter<void>();
  @Output() swapRequested = new EventEmitter<{ from: number; to: number }>();

  months = MONTHS;

  get assignedCount(): number {
    return this.assignments.length;
  }

  get isComplete(): boolean {
    return this.assignments.length === 12;
  }

  validationMessage(): string {
    if (this.duplicateImageIds?.length) {
      const duplicates = this.duplicateImageIds.map((id) => `#${id}`).join(', ');
      return `Попередження: зображення використовуються декілька разів (${duplicates}).`;
    }

    if (!this.isComplete) {
      return `Заповніть ще ${12 - this.assignedCount} місяців, щоб завершити календар.`;
    }

    return 'Усі місяці заповнені — можна створювати календар!';
  }

  onMonthClick(month: number): void {
    this.monthSelected.emit(month);
  }

  onClearMonth(month: number, event: Event): void {
    event.stopPropagation();
    this.clearMonth.emit(month);
  }

  onClearAllClick(): void {
    this.clearAll.emit();
  }

  onDragStart(month: number, event: DragEvent): void {
    event.dataTransfer?.setData('text/plain', month.toString());
    event.dataTransfer?.setDragImage(new Image(), 0, 0);
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
  }

  onDrop(targetMonth: number, event: DragEvent): void {
    event.preventDefault();
    const fromText = event.dataTransfer?.getData('text/plain');
    const from = fromText ? parseInt(fromText, 10) : NaN;
    if (!from || Number.isNaN(from) || from === targetMonth) {
      return;
    }
    this.swapRequested.emit({ from, to: targetMonth });
  }

  assignmentForMonth(month: number): MonthAssignment | undefined {
    return this.assignments.find((assignment) => assignment.month === month);
  }

  isDuplicate(assignment?: MonthAssignment): boolean {
    if (!assignment) {
      return false;
    }
    return this.duplicateImageIds?.includes(assignment.imageId) ?? false;
  }

  progressLabel(): string {
    return this.isComplete
      ? 'Готово: 12/12'
      : `Заповнено: ${this.assignedCount}/12`;
  }
}
