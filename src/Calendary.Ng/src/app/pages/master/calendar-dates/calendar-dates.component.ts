import { Component, Input, OnChanges, OnInit, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { EventDate } from '../../../../models/event-date';
import { EventDateService } from '../../../../services/event-date.service';
import { FluxModel } from '../../../../models/flux-model';
import { CalendarService } from '../../../../services/calendar.service';
import { Calendar } from '../../../../models/calendar';

@Component({
  selector: 'app-calendar-dates',
  standalone: true,
  imports: [CommonModule, FormsModule, MatButtonModule],
  templateUrl: './calendar-dates.component.html',
  styleUrl: './calendar-dates.component.scss'
})
export class CalendarDatesComponent implements OnInit, OnChanges{

  @Input()
  fluxModel: FluxModel | null = null;
  calendar: Calendar | null = null;
  
  eventDates: EventDate[] = [];
  newEventDate : EventDate = new EventDate();

  constructor(
    private eventDateService: EventDateService, 
    private calendarService : CalendarService) 
    {
      this.getCurrentCalendar();
    }


  getCurrentCalendar() {
    this.calendarService.getCalendar().subscribe(
      (calendar) => {
        this.calendar = calendar;
      },
      (error) => {
        console.error('Error getting current calendar', error);
      }
    );
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['fluxModel'] && changes['fluxModel'].currentValue) {
      this.fluxModel = changes['fluxModel'].currentValue;
    }
  }

  ngOnInit(): void {
    this.loadEventDates();
  }

  // Метод для завантаження всіх EventDates
  loadEventDates(): void {
    const userId = 1; // Отримати userId з авторизації або іншого джерела
    this.eventDateService.getAll().subscribe(
      (data: EventDate[]) => {
        this.eventDates = data;
      },
      (error) => {
        console.error('Error loading event dates', error);
      }
    );
  }

  // Метод для створення нового EventDate
  addEventDate(newEventDate: EventDate): void {
    this.eventDateService.createEventDate(newEventDate).subscribe(
      (eventDate: EventDate) => {
        this.eventDates.push(eventDate);
      },
      (error) => {
        console.error('Error creating event date', error);
      }
    );
  }

  // Метод для оновлення EventDate
  updateEventDate(id: number, updatedEventDate: EventDate): void {
    this.eventDateService.updateEventDate(updatedEventDate).subscribe(
      (eventDate: EventDate) => {
        const index = this.eventDates.findIndex(e => e.id === id);
        if (index !== -1) {
          this.eventDates[index] = eventDate;
        }
      },
      (error) => {
        console.error('Error updating event date', error);
      }
    );
  }

  // Метод для видалення EventDate
  deleteEventDate(id: number): void {
    this.eventDateService.deleteEventDate(id).subscribe(
      () => {
        this.eventDates = this.eventDates.filter(e => e.id !== id);
      },
      (error) => {
        console.error('Error deleting event date', error);
      }
    );
  }

  generateCalendar() {
    this.calendarService.generatePdf(this.calendar!.id, this.fluxModel!.id).subscribe(
      () => {
        this.getCurrentCalendar();
      }, 
      () => {
        console.error('Error generating calendar');
      }
    );   
  }
}
