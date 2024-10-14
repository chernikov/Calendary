import { Routes } from '@angular/router';
import { HomeComponent } from './pages/home/home.component';
import { RegisterComponent } from './pages/register/register.component';
import { Page404Component } from './pages/page404/page404.component';
import { SettingsComponent } from './pages/settings/settings.component';
import { LoginComponent } from './pages/login/login.component';
import { EventDatesComponent } from './pages/event-date/event-dates.component';
import { CalendarComponent } from './pages/calendar/calendar.component';

export const routes: Routes = [
    {path: '', component: HomeComponent},
    { path: 'calendar', component: CalendarComponent },
    {path: 'register', component: RegisterComponent},
    {path: 'login', component: LoginComponent},
    {path: 'settings', component: SettingsComponent},
    {path: 'event-dates', component: EventDatesComponent},
    {path: '**', component: Page404Component },
];
