import { Component } from '@angular/core';
import { PhotoUploadComponent } from './photo-upload/photo-upload.component';
import { CalendarDatesComponent } from './calendar-dates/calendar-dates.component';
import { CartButtonComponent } from './cart-button/cart-button.component';
import { GenerationComponent } from './generation-status/generation.component';
import { CalendarMonthsComponent } from './calendar-months/calendar-months.component';
import { PromptResultsComponent } from './prompt-results/prompt-results.component';
import { PromptSelectionComponent } from './prompt-selection/prompt-selection.component';
import { FluxModelComponent } from './flux-model/flux-model.component';
import { FluxModel } from '../../../models/flux-model';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { PaymentComponent } from './payment/payment.component';
import { TrainingResultsComponent } from './training-results/training-results.component';
import { ImageGenerationComponent } from './image-generation/image-generation.component';
import { PhotoSelectionComponent } from './photo-selection/photo-selection.component';
import { CalendarReadyComponent } from './calendar-ready/calendar-ready.component';
import { CalendarComponent } from "../calendar/calendar.component";

@Component({
  selector: 'app-master',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    FluxModelComponent,
    PhotoUploadComponent,
    GenerationComponent,
    PromptSelectionComponent,
    PaymentComponent,
    ImageGenerationComponent,
    CalendarMonthsComponent,
    CalendarDatesComponent,
    CalendarReadyComponent,
    CalendarComponent
],
  templateUrl: './master.component.html',
  styleUrl: './master.component.scss',
})
export class MasterComponent {
  fluxModel: any = null; // Поточна FluxModel

  updateFluxModel($event: FluxModel) {
    this.fluxModel = $event;
  }
}
