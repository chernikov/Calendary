import { CommonModule } from '@angular/common';
import { Component, Input, OnInit } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';

export interface Holiday {
  day: number;
  month: number;
  description: string;
}

export interface EventDate {
  day: number;
  month: number;
  description: string;
}

@Component({
  selector: 'app-month-page',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  templateUrl: './month-page.component.html',
  styleUrl: './month-page.component.scss'
})
export class MonthPageComponent implements OnInit {
  @Input() month!: number; // 1-12
  @Input() year: number = 2026;
  @Input() imageUrl?: string;
  @Input() monthName!: string;
  @Input() holidays: Holiday[] = [];
  @Input() eventDates: EventDate[] = [];
  @Input() firstDayOfWeek: number = 1; // 0 = Sunday, 1 = Monday
  @Input() layoutMode: 'image-top' | 'image-bottom' = 'image-top';
  @Input() colorScheme: 'default' | 'red' | 'blue' | 'green' = 'default';
  @Input() fontFamily: 'Arial' | 'Georgia' | 'Courier' = 'Arial';

  daysOfWeek: string[] = [];
  calendarDays: (number | null)[] = [];

  ngOnInit(): void {
    this.generateDaysOfWeek();
    this.generateCalendarDays();
  }

  private generateDaysOfWeek(): void {
    const allDays = ['Нд', 'Пн', 'Вт', 'Ср', 'Чт', 'Пт', 'Сб'];
    this.daysOfWeek = [];
    for (let i = 0; i < 7; i++) {
      this.daysOfWeek.push(allDays[(this.firstDayOfWeek + i) % 7]);
    }
  }

  private generateCalendarDays(): void {
    this.calendarDays = [];
    const firstDay = new Date(this.year, this.month - 1, 1);
    const lastDay = new Date(this.year, this.month, 0);
    const daysInMonth = lastDay.getDate();

    // Calculate starting position
    let startDay = firstDay.getDay() - this.firstDayOfWeek;
    if (startDay < 0) startDay += 7;

    // Add empty cells before first day
    for (let i = 0; i < startDay; i++) {
      this.calendarDays.push(null);
    }

    // Add days of month
    for (let day = 1; day <= daysInMonth; day++) {
      this.calendarDays.push(day);
    }

    // Fill remaining cells to complete the grid
    const remainingCells = (7 - (this.calendarDays.length % 7)) % 7;
    for (let i = 0; i < remainingCells; i++) {
      this.calendarDays.push(null);
    }
  }

  isDayHighlighted(day: number | null): boolean {
    if (!day) return false;

    const date = new Date(this.year, this.month - 1, day);
    const dayOfWeek = date.getDay();

    // Check if weekend
    if (dayOfWeek === 0 || dayOfWeek === 6) return true;

    // Check if holiday
    if (this.holidays.some(h => h.day === day && h.month === this.month)) return true;

    // Check if event date
    if (this.eventDates.some(e => e.day === day && e.month === this.month)) return true;

    return false;
  }

  getDayDescription(day: number | null): string {
    if (!day) return '';

    const holiday = this.holidays.find(h => h.day === day && h.month === this.month);
    if (holiday) return holiday.description;

    const eventDate = this.eventDates.find(e => e.day === day && e.month === this.month);
    if (eventDate) return eventDate.description;

    return '';
  }

  getColorClass(): string {
    return `color-${this.colorScheme}`;
  }

  getFontClass(): string {
    return `font-${this.fontFamily.toLowerCase()}`;
  }
}
