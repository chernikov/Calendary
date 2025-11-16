import { CommonModule } from '@angular/common';
import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import {
  CalendarMonth,
  CalendarCustomization,
  DEFAULT_CUSTOMIZATION,
  generateCalendarMonth,
} from '../../models/calendar-grid.model';
import { Holiday } from '@models/holiday';

@Component({
  selector: 'app-month-page',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  templateUrl: './month-page.component.html',
  styleUrl: './month-page.component.scss',
})
export class MonthPageComponent implements OnChanges {
  @Input() month: number = 1;
  @Input() year: number = 2026;
  @Input() imageUrl?: string;
  @Input() holidays: Holiday[] = [];
  @Input() customization: CalendarCustomization = DEFAULT_CUSTOMIZATION;

  calendarMonth?: CalendarMonth;
  weekDays = ['Пн', 'Вт', 'Ср', 'Чт', 'Пт', 'Сб', 'Нд'];

  ngOnChanges(changes: SimpleChanges): void {
    if (
      changes['month'] ||
      changes['year'] ||
      changes['holidays'] ||
      changes['imageUrl']
    ) {
      this.generateMonth();
    }
  }

  private generateMonth(): void {
    this.calendarMonth = generateCalendarMonth(
      this.month,
      this.year,
      this.holidays
    );
    if (this.imageUrl) {
      this.calendarMonth.imageUrl = this.imageUrl;
    }
  }

  getCustomStyles() {
    return {
      '--font-family': this.customization.fontFamily,
      '--font-size': `${this.customization.fontSize}px`,
      '--primary-color': this.customization.primaryColor,
      '--secondary-color': this.customization.secondaryColor,
      '--holiday-color': this.customization.holidayColor,
      '--weekend-color': this.customization.weekendColor,
    };
  }

  getMonthHolidays(): Holiday[] {
    return this.holidays.filter((h) => {
      const date = new Date(h.date);
      return date.getMonth() === this.month - 1;
    });
  }
}
