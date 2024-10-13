import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EventDate } from '../../../models/event-date';
import { EventDateService } from '../../../services/event-date.service';


@Component({
  selector: 'app-event-dates',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './event-dates.component.html',
  styleUrls: ['./event-dates.component.scss']
})
export class EventDatesComponent implements OnInit {
  eventDates: EventDate[] = [];
  newEventDate : EventDate = new EventDate();

  constructor(private eventDateService: EventDateService) { }


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
}
