import { Routes, PreloadAllModules } from '@angular/router';
import { HomeComponent } from './pages/home/home.component';
import { RegisterComponent } from './pages/register/register.component';
import { Page404Component } from './pages/page404/page404.component';
import { SettingsComponent } from './pages/settings/settings.component';
import { LoginComponent } from './pages/login/login.component';
import { EventDatesComponent } from './pages/event-date/event-dates.component';
import { CalendarComponent } from './pages/calendar/calendar.component';
import { AuthGuard } from '../guards/admin.guard';
import { MainComponent } from './main.component';
import { AdminComponent } from './admin/admin.component';
import { HolidayComponent } from './admin/pages/holiday/holiday.component';
import { AdminHomeComponent } from './admin/pages/admin-home/admin-home.component';
import { ProfileComponent } from './pages/profile/profile.component';
import { CartComponent } from './pages/cart/cart.component';

export const routes: Routes = [
  {
    path: 'admin',
    canActivate: [AuthGuard],
    children : [{
        path: '',
        component: AdminComponent,
        children: [
          { path: '', component: AdminHomeComponent},
          { path: 'holiday', component: HolidayComponent },
        ],
      }
    ],
  },
  {
    path: '',
    component: MainComponent,
    children: [
      { path: '', component: HomeComponent },
      { path: 'calendar', component: CalendarComponent },
      { path: 'profile', component: ProfileComponent },
      { path: 'cart', component: CartComponent },

      /* to refine **/
      { path: 'register', component: RegisterComponent },
      { path: 'login', component: LoginComponent },
      
      /* to remove */
      { path: 'settings', component: SettingsComponent },
      { path: 'event-dates', component: EventDatesComponent },
    ],
  },
  { path: '**', component: Page404Component },
];
