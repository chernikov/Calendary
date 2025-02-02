import { Routes } from '@angular/router';
import { HomeComponent } from './pages/home/home.component';
import { RegisterComponent } from './pages/register/register.component';
import { Page404Component } from './pages/page404/page404.component';
import { SettingsComponent } from './components/settings/settings.component';
import { LoginComponent } from './pages/login/login.component';
import { CalendarComponent } from './pages/calendar/calendar.component';
import { AdminGuard } from '../guards/admin.guard';
import { MainComponent } from './main.component';
import { AdminComponent } from './admin/admin.component';
import { HolidayComponent } from './admin/pages/holiday/holiday.component';
import { AdminOrderComponent } from './admin/pages/admin-order/admin-order.component';
import { AdminHomeComponent } from './admin/pages/admin-home/admin-home.component';
import { ProfileComponent } from './pages/profile/profile.component';
import { CartComponent } from './pages/cart/cart.component';
import { OrderComponent } from './pages/order/order.component';
import { AdminPromptThemeComponent } from './admin/pages/admin-prompt-theme/admin-prompt-theme.component';
import { AdminPromptComponent } from './admin/pages/admin-prompt/admin-prompt.component';
import { EditPromptComponent } from './admin/pages/admin-prompt/edit-prompt/edit-prompt.component';
import { PromptHistoryComponent } from './admin/pages/prompt-history/prompt-history.component';
import { MasterComponent } from './pages/master/master.component';
import { FluxModelComponent } from './admin/pages/flux-model/flux-model.component';
import { ViewFluxModelComponent } from './admin/pages/flux-model/view-flux-model/view-flux-model.component';
import { AdminCategoryComponent } from './admin/pages/admin-category/admin-category.component';
import { UserGuard } from '../guards/user.guard';
import { ForgotPasswordComponent } from './pages/forgot-password/forgot-password.component';
import { VerifyComponent } from './pages/verify/verify.component';
import { AdminUserComponent } from './admin/pages/admin-user/admin-user.component';
import { AdminUserViewComponent } from './admin/pages/admin-user/admin-user-view/admin-user-view.component';
import { TrainingComponent } from './admin/pages/admin-user/admin-user-view/flux-model/training/training.component';


export const routes: Routes = [
  {
    path: 'admin',
    canActivate: [AdminGuard],
    children : [{
        path: '',
        component: AdminComponent,
        children: [
          { path: '', component: AdminHomeComponent},
          { path: 'holiday', component: HolidayComponent },
          { path: 'orders', component: AdminOrderComponent },
          { path: 'categories', component: AdminCategoryComponent },
          { path: 'prompt-themes', component: AdminPromptThemeComponent },
          { path: 'prompts', component: AdminPromptComponent },
          { path: 'prompts/edit/:id', component: EditPromptComponent },
          { path: 'prompts/create', component: EditPromptComponent },
          { path: 'prompts/:id/history', component: PromptHistoryComponent },
          { path: 'flux-models', component: FluxModelComponent },
          { path: 'flux-models/view/:id', component: ViewFluxModelComponent },
          { path: 'users', component: AdminUserComponent },
          { path: 'users/view/:id', component: AdminUserViewComponent },
          { path: 'training/:id', component: TrainingComponent },
        ],
      }
    ],
  },
  {
    path: '',
    component: MainComponent,
    children: [
      { path: '', component: HomeComponent },
      { path: 'register', component: RegisterComponent },
      { path: 'login', component: LoginComponent },
      { path: 'forgot-password', component: ForgotPasswordComponent },
      { path: 'verify/:token', component: VerifyComponent },
      { path: 'profile', component: ProfileComponent, canActivate: [UserGuard] },
      { path: 'cart', component: CartComponent, canActivate: [UserGuard]  },
      { path: 'order/:orderId', component: OrderComponent },
      { path: 'master', component: MasterComponent, canActivate: [UserGuard]  },

      /* to remove */
      { path: 'settings', component: SettingsComponent, canActivate: [UserGuard]  },
    ],
  },
  { path: '**', component: Page404Component },
];
