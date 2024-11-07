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

  onImagesFull(imagesFull: boolean) {
    this.isValid = imagesFull;
  }

  proceedToOrder() {
    this.calendarService.addToCart(this.calendar!).subscribe({
      next: () => {
        console.log('Added to cart');
      },
      error: (error) => {
        console.error('Error adding to cart', error);
     }
    });
  }

  onCalendarUpdated(calendar: Calendar) {
    this.calendarService.updateCalendar(calendar).subscribe({
      next: () => {},
      error: (error) => {
        console.error('Error saving calendar settings', error);
      },
    });
  }
}
