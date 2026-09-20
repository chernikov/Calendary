import { registerLocaleData } from '@angular/common';
import localeUk from '@angular/common/locales/uk';
import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { AppComponent } from './app/app.component';

// ng-zorro's date-picker (provideNzI18n(uk_UA) in app.config.ts) calls into Angular's own
// formatDate/DatePipe for the 'uk' locale — Angular 19 throws NG0701 (MISSING_LOCALE_DATA) if
// that locale was never registered, whereas 18 apparently tolerated it silently.
registerLocaleData(localeUk);

bootstrapApplication(AppComponent, appConfig).catch((err) => console.error(err));
