import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { CalendarService } from '../../../services/calendar.service';
import { Calendar } from '../../../models/calendar';
import { Language } from '../../../models/language';
import { LanguageService } from '../../../services/language.service';



@Component({
  selector: 'app-calendar',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './calendar.component.html',
  styleUrls: ['./calendar.component.scss']
})
export class CalendarComponent implements OnInit {
  calendarForm!: FormGroup;
  daysOfWeek = ['Неділя', 'Понеділок'];
  
  languages: Language[] = [];

  calendar: Calendar | null = new Calendar();
  

  constructor(
    private fb: FormBuilder,
    private calendarService: CalendarService,
    private languageService : LanguageService
  ) {}

  ngOnInit() {
    this.initForm();

    this.languageService.getLanguages().subscribe((data) => {
      this.languages = data;
    });
  this.loadCurrentCalendar();
  }

initForm() {
  this.calendarForm = this.fb.group({
    firstDayOfWeek: [null, Validators.required],
    languageId: [null, Validators.required],
  });
}

  loadCurrentCalendar() {
    this.calendarService.getCalendar().subscribe((calendar) => {
      this.calendar = calendar;
      this.calendarForm.patchValue({
        firstDayOfWeek: calendar.firstDayOfWeek,
        languageId: calendar.languageId,
      });
    });
  }


  // Створити новий календар
  onCreateCalendar() {
    this.calendar = new Calendar();
    this.calendarService.createCalendar(this.calendar)
      .subscribe((response: Calendar) => {
        console.log('Calendar created successfully!', response);
      }, error => {
        console.error('Error creating calendar', error);
      });
  }

  onSubmit() {
    if (this.calendarForm.valid) {
      const updatedCalendar = this.calendarForm.value;
      updatedCalendar.id = this.calendar?.id;
      this.calendarService.updateCalendar(updatedCalendar).subscribe({
        next: () => {
          alert('Calendar settings saved successfully!');
        },
        error: (error) => {
          console.error('Error saving calendar settings', error);
        },
      });
    }
  
  }
}