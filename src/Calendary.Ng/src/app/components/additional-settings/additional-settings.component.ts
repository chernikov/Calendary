import {
  Component,
  EventEmitter,
  Input,
  OnChanges,
  OnInit,
  Output,
  SimpleChanges,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { Language } from '../../../models/language';
import { LanguageService } from '../../../services/language.service';
import { Calendar } from '../../../models/calendar';

@Component({
    standalone: true,
    selector: 'app-additional-settings',
    imports: [CommonModule, FormsModule, ReactiveFormsModule],
    templateUrl: './additional-settings.component.html',
    styleUrl: './additional-settings.component.scss'
})
export class AdditionalSettingsComponent implements OnInit, OnChanges {
  calendarForm!: FormGroup;
  daysOfWeek = ['Неділя', 'Понеділок'];

  languages: Language[] = [];

  @Input()
  calendar: Calendar | null = null;

  @Output()
  calendarUpdated = new EventEmitter<Calendar>();

  constructor(
    private fb: FormBuilder,
    private languageService: LanguageService
  ) {}

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['calendar'] && !changes['calendar'].firstChange) {
      this.calendar = changes['calendar'].currentValue;
      if (this.calendar) {
        this.populateForm();
      }
    }
  }

  populateForm(): void {
    if (this.calendar) {
      this.calendarForm.patchValue({
        firstDayOfWeek: this.calendar.firstDayOfWeek,
        languageId: this.calendar.languageId,
      });
    }
  }

  ngOnInit(): void {
    this.initForm();

    this.languageService.getLanguages().subscribe((data) => {
      this.languages = data;
      this.populateForm();
    });
  }

  initForm() {
    this.calendarForm = this.fb.group({
      firstDayOfWeek: [null, Validators.required],
      languageId: [null, Validators.required],
    });
  }

  onFormChange(): void {
    if (this.calendarForm.valid) {
      const updatedCalendar = this.calendarForm.value;
      updatedCalendar.id = this.calendar?.id;
      this.calendarUpdated.emit(updatedCalendar);
    }
  }
}
