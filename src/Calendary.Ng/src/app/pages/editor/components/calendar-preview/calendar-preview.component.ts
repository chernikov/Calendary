import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output, OnInit } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSliderModule } from '@angular/material/slider';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { FormsModule } from '@angular/forms';
import { MonthAssignment, MONTHS } from '../../models/calendar-assignment.model';
import { MonthPageComponent } from '../month-page/month-page.component';
import { CalendarCustomization, DEFAULT_CUSTOMIZATION } from '../../models/calendar-grid.model';
import { Holiday } from '../../../../models/holiday';

@Component({
  selector: 'app-calendar-preview',
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule,
    MatSliderModule,
    MatSelectModule,
    MatFormFieldModule,
    FormsModule,
    MonthPageComponent,
  ],
  templateUrl: './calendar-preview.component.html',
  styleUrl: './calendar-preview.component.scss',
})
export class CalendarPreviewComponent implements OnInit {
  @Input() assignments: MonthAssignment[] = [];
  @Input() duplicateImageIds: string[] = [];
  @Input() holidays: Holiday[] = [];
  @Output() monthSelected = new EventEmitter<number>();
  @Output() clearMonth = new EventEmitter<number>();
  @Output() clearAll = new EventEmitter<void>();
  @Output() swapRequested = new EventEmitter<{ from: number; to: number }>();
  @Output() generatePdf = new EventEmitter<void>();
  @Output() editMonth = new EventEmitter<number>();

  months = MONTHS;

  // Preview mode
  viewMode: 'grid' | 'preview' = 'grid';
  currentPreviewMonth = 1;
  year = 2026;

  // Controls
  zoomLevel = 100;
  isFullscreen = false;

  // Customization
  customization: CalendarCustomization = { ...DEFAULT_CUSTOMIZATION };
  showCustomization = false;

  fontOptions = [
    'Arial, sans-serif',
    'Georgia, serif',
    'Times New Roman, serif',
    'Courier New, monospace',
    'Verdana, sans-serif',
    'Helvetica, sans-serif',
  ];

  ngOnInit(): void {
    this.loadCustomization();
  }

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

  // View mode methods
  switchToPreview(): void {
    if (!this.isComplete) {
      return;
    }
    this.viewMode = 'preview';
    this.currentPreviewMonth = 1;
  }

  switchToGrid(): void {
    this.viewMode = 'grid';
  }

  // Navigation methods
  nextMonth(): void {
    if (this.currentPreviewMonth < 12) {
      this.currentPreviewMonth++;
    }
  }

  prevMonth(): void {
    if (this.currentPreviewMonth > 1) {
      this.currentPreviewMonth--;
    }
  }

  goToMonth(month: number): void {
    this.currentPreviewMonth = month;
  }

  getCurrentMonthAssignment(): MonthAssignment | undefined {
    return this.assignmentForMonth(this.currentPreviewMonth);
  }

  // Control methods
  zoomIn(): void {
    if (this.zoomLevel < 200) {
      this.zoomLevel += 10;
    }
  }

  zoomOut(): void {
    if (this.zoomLevel > 50) {
      this.zoomLevel -= 10;
    }
  }

  resetZoom(): void {
    this.zoomLevel = 100;
  }

  toggleFullscreen(): void {
    if (!document.fullscreenElement) {
      const elem = document.documentElement;
      elem.requestFullscreen().then(() => {
        this.isFullscreen = true;
      });
    } else {
      document.exitFullscreen().then(() => {
        this.isFullscreen = false;
      });
    }
  }

  onEditCurrentMonth(): void {
    this.editMonth.emit(this.currentPreviewMonth);
  }

  onGeneratePdf(): void {
    if (this.isComplete) {
      this.generatePdf.emit();
    }
  }

  // Customization methods
  toggleCustomization(): void {
    this.showCustomization = !this.showCustomization;
  }

  onCustomizationChange(): void {
    this.saveCustomization();
  }

  resetCustomization(): void {
    this.customization = { ...DEFAULT_CUSTOMIZATION };
    this.saveCustomization();
  }

  private loadCustomization(): void {
    if (typeof localStorage === 'undefined') {
      return;
    }

    try {
      const saved = localStorage.getItem('calendar-customization');
      if (saved) {
        this.customization = JSON.parse(saved);
      }
    } catch {
      // Use defaults if loading fails
    }
  }

  private saveCustomization(): void {
    if (typeof localStorage === 'undefined') {
      return;
    }

    try {
      localStorage.setItem(
        'calendar-customization',
        JSON.stringify(this.customization)
      );
    } catch {
      // Silently fail
    }
  }

  getPreviewStyles() {
    return {
      transform: `scale(${this.zoomLevel / 100})`,
      'transform-origin': 'top center',
    };
  }
}
