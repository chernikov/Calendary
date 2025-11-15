import { Component } from '@angular/core';
import { PhotoUploadComponent } from './photo-upload/photo-upload.component';
import { CalendarDatesComponent } from './calendar-dates/calendar-dates.component';
import { GenerationComponent } from './generation-status/generation.component';
import { CalendarMonthsComponent } from './calendar-months/calendar-months.component';
import { PromptSelectionComponent } from './prompt-selection/prompt-selection.component';
import { FluxModelComponent } from './flux-model/flux-model.component';
import { FluxModel } from '../../../models/flux-model';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { PaymentComponent } from './payment/payment.component';
import { ImageGenerationComponent } from './image-generation/image-generation.component';
import { CalendarReadyComponent } from './calendar-ready/calendar-ready.component';
import { MatIconModule } from '@angular/material/icon';

@Component({
    standalone: true,
    selector: 'app-master',
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
        MatIconModule,
    ],
    templateUrl: './master.component.html',
    styleUrl: './master.component.scss'
})
export class MasterComponent {
  fluxModel: any = null; // Поточна FluxModel

  fluxModelResult : boolean = false; 
  uploadPhotoResult : boolean = false;
  payedModelResult : boolean = false;
  generationModelResult : boolean = false;
  promptSelectionResult : boolean = false;
  imageGenerationResult : boolean = false;

  updateFluxModel($event: FluxModel) {
    this.fluxModel = $event;
  }

  toggleCard(toggle: boolean) : boolean {
    return !toggle;
  }

}
