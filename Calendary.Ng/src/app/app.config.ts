import { ApplicationConfig, LOCALE_ID, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { provideClientHydration } from '@angular/platform-browser';
import { HTTP_INTERCEPTORS, provideHttpClient, withFetch, withInterceptorsFromDi } from '@angular/common/http';
import { AuthInterceptor } from '../services/auth.interceptor';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import localeUk from '@angular/common/locales/uk';
import { registerLocaleData } from '@angular/common';

registerLocaleData(localeUk);

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }), 
    provideRouter(routes), 
    provideClientHydration(), 
    provideHttpClient(withFetch(), withInterceptorsFromDi()),  
    {
        provide: HTTP_INTERCEPTORS,
        useClass: AuthInterceptor,
        multi: true
    }, 
    { provide: LOCALE_ID, useValue: 'uk' },
    provideAnimationsAsync(),
    
  ]
}
