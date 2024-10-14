import { Component, OnInit } from '@angular/core';
import { CalendarService } from '../../../services/calendar.service';
import { Calendar } from '../../../models/calendar';
import { Language } from '../../../models/language';
import { LanguageService } from '../../../services/language.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';


@Component({
  selector: 'app-calendar',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './calendar.component.html',
  styleUrls: ['./calendar.component.scss']
})
export class CalendarComponent implements OnInit {
  calendar: Calendar = new Calendar();
  languages: Language[] = [];

  calendarId: number | null = null;
  

  constructor(private calendarService: CalendarService,
    private languageService : LanguageService
  ) {}

  ngOnInit() {
    this.languageService.getLanguages().subscribe((data) => {
        this.languages = data;
    });
  }


  // Створити новий календар
  onCreateCalendar() {
    this.calendarService.createCalendar(this.calendar)
      .subscribe((response: Calendar) => {
        this.calendarId = response.id;
        console.log('Calendar created successfully!', response);
      }, error => {
        console.error('Error creating calendar', error);
      });
  }
}