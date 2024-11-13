import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { DayOfWeek } from '../../../models/day-of-week.enum';
import { Setting } from '../../../models/setting';
import { Language } from '../../../models/language';
import { Country } from '../../../models/country';
import { SettingService } from '../../../services/setting.service';
import { CountryService } from '../../../services/country.service';
import { LanguageService } from '../../../services/language.service';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.scss',
})
export class SettingsComponent implements OnInit {
  settingsForm!: FormGroup;

  settings!: Setting;

  daysOfWeek = ['Неділя', 'Понеділок'];

  languages: Language[] = [];
  countries: Country[] = [];

  constructor(
    private fb: FormBuilder,
    private settingService: SettingService,
    private countryService: CountryService,
    private languageService: LanguageService
  ) {}

  ngOnInit(): void {
    this.initForm();

    this.countryService.getCountries().subscribe((data) => {
      this.countries = data;
    });

    this.languageService.getLanguages().subscribe((data) => {
      this.languages = data;
    });

    // Отримуємо дані налаштувань для заповнення форми
    this.settingService.getSettings().subscribe((data: Setting) => {
      this.settings = data;
      this.settingsForm.patchValue(this.settings);
    });
  }

  initForm() {
    this.settingsForm = this.fb.group({
      firstDayOfWeek: [null, Validators.required],
      language: this.fb.group({
        id: [null, Validators.required],
        name: [''],
      }),
      country: this.fb.group({
        id: [null, Validators.required],
        name: [''],
        code: ['']
      }),
    });
  }

  // Метод для збереження змін
  onSubmit() {
    if (this.settingsForm.valid) {
      const updatedSettings = this.settingsForm.value as Setting;
      this.settingService.saveSettings(updatedSettings).subscribe({
        next: (response) => {
          alert('Settings saved successfully!');
        },
        error: (error) => {
          console.error('Error saving settings', error);
        },
      });
    }
  }
}
