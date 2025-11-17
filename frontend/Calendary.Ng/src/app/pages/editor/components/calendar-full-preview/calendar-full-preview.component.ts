import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSelectModule } from '@angular/material/select';
import { MatSliderModule } from '@angular/material/slider';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { FormsModule } from '@angular/forms';
import { MonthPageComponent, Holiday, EventDate } from '../month-page/month-page.component';
import { MonthAssignment, MONTHS } from '../../models/calendar-assignment.model';

@Component({
  selector: 'app-calendar-full-preview',
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule,
    MatSelectModule,
    MatSliderModule,
    MatButtonToggleModule,
    FormsModule,
    MonthPageComponent
  ],
  templateUrl: './calendar-full-preview.component.html',
  styleUrl: './calendar-full-preview.component.scss'
})
export class CalendarFullPreviewComponent implements OnInit {
  @Input() assignments: MonthAssignment[] = [];
  @Input() year: number = 2026;
  @Input() holidays: Holiday[] = [];
  @Input() eventDates: EventDate[] = [];
  @Input() firstDayOfWeek: number = 1;

  @Output() generatePdf = new EventEmitter<void>();
  @Output() editMonth = new EventEmitter<number>();
  @Output() close = new EventEmitter<void>();

  currentMonth: number = 1;
  zoomLevel: number = 100;
  isFullscreen: boolean = false;
  viewMode: 'single' | 'all' = 'single';

  // Customization options
  layoutMode: 'image-top' | 'image-bottom' = 'image-top';
  colorScheme: 'default' | 'red' | 'blue' | 'green' = 'default';
  fontFamily: 'Arial' | 'Georgia' | 'Courier' = 'Arial';

  months = MONTHS;

  fontOptions = [
    { value: 'Arial', label: 'Arial' },
    { value: 'Georgia', label: 'Georgia' },
    { value: 'Courier', label: 'Courier New' }
  ];

  colorOptions = [
    { value: 'default', label: 'За замовчуванням' },
    { value: 'red', label: 'Червоний' },
    { value: 'blue', label: 'Синій' },
    { value: 'green', label: 'Зелений' }
  ];

  ngOnInit(): void {
    // Start with first month that has assignment, or month 1
    const firstAssignment = this.assignments.find(a => a.month === 1);
    if (firstAssignment) {
      this.currentMonth = firstAssignment.month;
    }
  }

  getMonthName(month: number): string {
    return this.months.find(m => m.value === month)?.label || '';
  }

  getImageUrl(month: number): string | undefined {
    return this.assignments.find(a => a.month === month)?.imageUrl;
  }

  previousMonth(): void {
    if (this.currentMonth > 1) {
      this.currentMonth--;
    } else {
      this.currentMonth = 12;
    }
  }

  nextMonth(): void {
    if (this.currentMonth < 12) {
      this.currentMonth++;
    } else {
      this.currentMonth = 1;
    }
  }

  goToMonth(month: number): void {
    this.currentMonth = month;
  }

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
    if (!this.isFullscreen) {
      this.enterFullscreen();
    } else {
      this.exitFullscreen();
    }
  }

  private enterFullscreen(): void {
    const elem = document.documentElement;
    if (elem.requestFullscreen) {
      elem.requestFullscreen();
      this.isFullscreen = true;
    }
  }

  private exitFullscreen(): void {
    if (document.exitFullscreen) {
      document.exitFullscreen();
      this.isFullscreen = false;
    }
  }

  toggleViewMode(): void {
    this.viewMode = this.viewMode === 'single' ? 'all' : 'single';
  }

  onEditMonth(month: number): void {
    this.editMonth.emit(month);
  }

  onGeneratePdf(): void {
    this.generatePdf.emit();
  }

  onClose(): void {
    this.close.emit();
  }

  get canGeneratePdf(): boolean {
    return this.assignments.length === 12;
  }

  getZoomStyle(): any {
    return {
      transform: `scale(${this.zoomLevel / 100})`,
      transformOrigin: 'top center'
    };
  }

  getMonthsToDisplay(): number[] {
    if (this.viewMode === 'single') {
      return [this.currentMonth];
    }
    return [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12];
  }
}
