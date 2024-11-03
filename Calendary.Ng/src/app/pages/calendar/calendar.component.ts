import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CalendarService } from '../../../services/calendar.service';
import { Calendar } from '../../../models/calendar';
import { CalendarImagesComponent } from '../../components/calendar-images/calendar-images.component';
import { AdditionalSettingsComponent } from '../../components/additional-settings/additional-settings.component';
import { EventDatesComponent } from '../../components/event-date/event-dates.component';

@Component({
  selector: 'app-calendar',
  standalone: true,
  imports: [
    CommonModule,
    CalendarImagesComponent,
    EventDatesComponent,
    AdditionalSettingsComponent,
  ],
  templateUrl: './calendar.component.html',
  styleUrls: ['./calendar.component.scss'],
})
export class CalendarComponent implements OnInit {
  calendar: Calendar | null = null;
  isValid: boolean = false;

  constructor(private calendarService: CalendarService) {}

  ngOnInit() {
    this.loadCurrentCalendar();
  }

  loadCurrentCalendar() {
    this.calendarService.getCalendar().subscribe((calendar) => {
      console.log('Calendar loaded');
      this.calendar = calendar;
    });
  }

  // Створити новий календар
  onCreateCalendar() {
    this.calendar = new Calendar();
    this.calendarService.createCalendar(this.calendar).subscribe(
      (response: Calendar) => {
        console.log('Calendar created successfully!', response);
      },
      (error) => {
        console.error('Error creating calendar', error);
      }
    );
  }

  onImagesFull(imagesFull: boolean) {
    this.isValid = imagesFull;
  }

  proceedToOrder() {
    alert('Перехід до оформлення замовлення');
  }

  onCalendarUpdated(calendar: Calendar) {
    this.calendarService.updateCalendar(calendar).subscribe({
      next: () => {},
      error: (error) => {
        console.error('Error saving calendar settings', error);
      },
    });
  }

  onImageUpload(imageUrl: string) {}

  onGeneratedCompleted($event: any) {
    alert('PDF згенеровано успішно!');
    this.loadCurrentCalendar();
  }
}
