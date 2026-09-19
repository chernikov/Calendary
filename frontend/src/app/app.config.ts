import { ApplicationConfig, isDevMode, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideStore } from '@ngrx/store';
import { provideEffects } from '@ngrx/effects';
import { provideStoreDevtools } from '@ngrx/store-devtools';
import { provideNzIcons } from 'ng-zorro-antd/icon';
import { provideNzI18n } from 'ng-zorro-antd/i18n';
import { uk_UA } from 'ng-zorro-antd/i18n';
import {
  ShoppingOutline,
  UserOutline,
  SettingOutline,
  ReloadOutline,
  PictureOutline,
  LeftOutline,
} from '@ant-design/icons-angular/icons';
import { routes } from './app.routes';
import { authInterceptor } from './core/auth.interceptor';
import { ORDER_FEATURE_KEY, OrderEffects, orderReducer } from './core/state/order';
import { ADMIN_FEATURE_KEY, AdminEffects, adminReducer } from './core/state/admin';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor])),
    provideStore({ [ORDER_FEATURE_KEY]: orderReducer, [ADMIN_FEATURE_KEY]: adminReducer }),
    provideEffects([OrderEffects, AdminEffects]),
    provideStoreDevtools({ maxAge: 25, logOnly: !isDevMode() }),
    provideAnimationsAsync(),
    provideNzI18n(uk_UA),
    provideNzIcons([ShoppingOutline, UserOutline, SettingOutline, ReloadOutline, PictureOutline, LeftOutline]),
  ],
};
