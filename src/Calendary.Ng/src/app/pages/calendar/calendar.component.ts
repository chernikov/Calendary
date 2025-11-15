import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CalendarService } from '../../../services/calendar.service';
import { Calendar } from '../../../models/calendar';
import { CalendarImagesComponent } from '../../components/calendar-images/calendar-images.component';
import { EventDatesComponent } from '../../components/event-date/event-dates.component';
import { Router, RouterModule } from '@angular/router';

@Component({
    standalone: true,
    selector: 'app-calendar',
    imports: [
        CommonModule,
        CalendarImagesComponent,
        EventDatesComponent,
        RouterModule
    ],
    templateUrl: './calendar.component.html',
    styleUrls: ['./calendar.component.scss']
})
export class CalendarComponent implements OnInit {
  calendar: Calendar | null = null;
  isValid: boolean = false;

  constructor(private calendarService: CalendarService, 
      private router: Router
  ) {}

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
        this.router.navigate(['/cart']);
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
