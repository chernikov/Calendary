import { Component, Input, OnChanges, OnInit, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatCardModule } from '@angular/material/card';
import { EventDate } from '../../../../models/event-date';
import { EventDateService } from '../../../../services/event-date.service';
import { FluxModel } from '../../../../models/flux-model';
import { CalendarService } from '../../../../services/calendar.service';
import { Calendar } from '../../../../models/calendar';
import { HolidayPreset } from '../../../../models/holiday-preset';
import { HolidayPresetService } from '../../../../services/holiday-preset.service';

@Component({
    standalone: true,
    selector: 'app-calendar-dates',
    imports: [CommonModule, FormsModule, MatButtonModule, MatCheckboxModule, MatCardModule],
    templateUrl: './calendar-dates.component.html',
    styleUrl: './calendar-dates.component.scss'
})
export class CalendarDatesComponent implements OnInit, OnChanges{

  @Input()
  fluxModel: FluxModel | null = null;
  calendar: Calendar | null = null;
  isGenerating = false;

  eventDates: EventDate[] = [];
  newEventDate : EventDate = new EventDate();

  // Holiday presets
  holidayPresets: HolidayPreset[] = [];
  selectedPresets: { [key: string]: boolean } = {};
  isApplyingPresets = false;

  constructor(
    private eventDateService: EventDateService,
    private calendarService : CalendarService,
    private holidayPresetService: HolidayPresetService)
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
    this.loadHolidayPresets();
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

  // Holiday preset methods
  loadHolidayPresets(): void {
    this.holidayPresetService.getAllPresets().subscribe(
      (presets: HolidayPreset[]) => {
        this.holidayPresets = presets;
      },
      (error) => {
        console.error('Error loading holiday presets', error);
      }
    );
  }

  getPresetName(preset: HolidayPreset): string {
    const ukrainianTranslation = preset.translations?.find(t => t.languageId === 1);
    return ukrainianTranslation?.name || preset.code;
  }

  getPresetTypeLabel(type: string): string {
    const typeMap: { [key: string]: string } = {
      'State': 'Державні',
      'Religious': 'Релігійні',
      'International': 'Міжнародні'
    };
    return typeMap[type] || type;
  }

  applySelectedPresets(): void {
    if (!this.calendar) {
      console.error('Calendar not loaded');
      return;
    }

    const selectedCodes = Object.keys(this.selectedPresets).filter(code => this.selectedPresets[code]);

    if (selectedCodes.length === 0) {
      alert('Будь ласка, оберіть принаймні один пресет свят');
      return;
    }

    this.isApplyingPresets = true;
    let completedRequests = 0;

    selectedCodes.forEach(code => {
      this.holidayPresetService.applyPresetToCalendar({
        calendarId: this.calendar!.id,
        presetCode: code
      }).subscribe(
        (success) => {
          completedRequests++;
          if (completedRequests === selectedCodes.length) {
            this.isApplyingPresets = false;
            alert('Свята успішно додано до календаря!');
          }
        },
        (error) => {
          completedRequests++;
          this.isApplyingPresets = false;
          console.error('Error applying preset', error);
          alert('Помилка при застосуванні пресету свят');
        }
      );
    });
  }

  generateCalendar() {
    // Блокування кнопки
    this.isGenerating = true;

    this.calendarService.generatePdf(this.calendar!.id, this.fluxModel!.id).subscribe(
      () => {
        this.isGenerating = false; // Розблокувати кнопку після завершення
        window.location.reload(); // Перезавантаження сторінки
      },
      (error) => {
        this.isGenerating = false; // Розблокувати кнопку навіть у разі помилки
        console.error('Error generating calendar', error);
      }
    );
  }
}
