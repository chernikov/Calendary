import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { CalendarService } from '../../../../../../../services/admin/calendar.service';
import { Calendar } from '../../../../../../../models/calendar';


@Component({
  standalone: true,
  imports: [CommonModule, MatTableModule],
  selector: 'app-user-calendar-list',
  templateUrl: './user-calendar-list.component.html',
  styleUrls: ['./user-calendar-list.component.scss']
})
export class UserCalendarListComponent implements OnInit {
  @Input() userId!: number; // Очікуємо, що userId буде передано з батьківського компонента
  calendars: Calendar[] = [];
  displayedColumns: string[] = ['id', 'preview', 'actions'];

  constructor(private calendarService: CalendarService) {}

  ngOnInit(): void {
    this.loadCalendars();
  }

  loadCalendars(): void {
    this.calendarService.getByUserId(this.userId).subscribe({
      next: (calendars) => this.calendars = calendars,
      error: (err) => console.error('Помилка завантаження календарів', err)
    });
  }
}
