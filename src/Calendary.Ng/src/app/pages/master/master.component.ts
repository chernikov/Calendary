import { Component } from '@angular/core';
import { PhotoUploadComponent } from './photo-upload/photo-upload.component';
import { CalendarDatesComponent } from './calendar-dates/calendar-dates.component';
import { CartButtonComponent } from './cart-button/cart-button.component';
import { GenerationResultsComponent } from './generation-results/generation-results.component';
import { GenerationStatusComponent } from './generation-status/generation-status.component';
import { CalendarMonthsComponent } from './calendar-months/calendar-months.component';
import { PromptResultsComponent } from './prompt-results/prompt-results.component';
import { PromptSelectionComponent } from './prompt-selection/prompt-selection.component';
import { FluxModelComponent } from './flux-model/flux-model.component';
import { FluxModel } from '../../../models/flux-model';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-master',
  standalone: true,
  imports: [
    CommonModule,
    FluxModelComponent,
    PhotoUploadComponent,
    CalendarDatesComponent,
    CalendarMonthsComponent,
    CartButtonComponent,
    GenerationResultsComponent,
    GenerationStatusComponent,
    PromptResultsComponent,
    PromptSelectionComponent,
  ],
  templateUrl: './master.component.html',
  styleUrl: './master.component.scss',
})
export class MasterComponent {
  fluxModel: FluxModel | null = null;

  onUpdateFluxModel($event: FluxModel) {
    this.fluxModel = $event;
  }
}
