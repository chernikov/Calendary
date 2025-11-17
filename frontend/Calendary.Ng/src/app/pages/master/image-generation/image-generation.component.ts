import { Component, EventEmitter, Input, OnChanges, OnDestroy, Output, SimpleChanges } from '@angular/core';
import { FluxModel } from '../../../../models/flux-model';
import { FluxModelService } from '../../../../services/flux-model.service';
import { interval, Subscription, switchMap, takeWhile } from 'rxjs';
import { CommonModule } from '@angular/common';

@Component({
    standalone: true,
    selector: 'app-image-generation',
    imports: [CommonModule],
    templateUrl: './image-generation.component.html',
    styleUrl: './image-generation.component.scss'
})
export class ImageGenerationComponent implements OnChanges, OnDestroy {
  @Input()
  fluxModel: FluxModel | null = null;
  @Output() 
  onUpdate = new EventEmitter<FluxModel>();

  imageUrls: string[] = [];
  selectedImage: string | null = null; // Обране зображення для прев'ю
  
  private pollingSubscription: Subscription | null = null;
  
  constructor(private fluxModelService: FluxModelService) {

  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['fluxModel'] && changes['fluxModel'].currentValue) {
      this.fluxModel = changes['fluxModel'].currentValue;
    }
    this.loadImageUrls();
     // Якщо статус "image_generating", почати опитування
     if (this.fluxModel?.status === 'image_generating') {
      this.startPolling();
    } else {
      this.stopPolling();

    }
  }

  ngOnDestroy(): void {
    // Зупинити опитування при знищенні компонента
    this.stopPolling();
  }

  private startPolling(): void {
    if (this.pollingSubscription) {
      this.pollingSubscription.unsubscribe();
    }

    this.pollingSubscription = interval(2000) // Інтервал у 10 секунд
      .pipe(
        switchMap(() => this.fluxModelService.current()) // Виклик API
      )
      .subscribe({
        next: (updatedModel: FluxModel) => {
          this.fluxModel = updatedModel;
          this.loadImageUrls();
          this.onUpdate.emit(this.fluxModel);
          if (this.fluxModel.status !== 'image_generating') {
            this.stopPolling(); // Зупинити опитування, якщо статус зміниться
          }
        },
        error: (err) => {
          console.error('Помилка при отриманні оновлення FluxModel:', err);
          this.stopPolling();
        }
      });
  }

  private stopPolling(): void {
    if (this.pollingSubscription) {
      this.pollingSubscription.unsubscribe();
      this.pollingSubscription = null;
    }
  }

  private loadImageUrls(): void {
    this.imageUrls = [];

    if (this.fluxModel?.jobs && this.fluxModel.jobs.length > 0) {
      const lastJob = this.fluxModel.jobs[this.fluxModel.jobs.length - 1];
      if (lastJob.tasks && lastJob.tasks.length > 0) {
        this.imageUrls = lastJob.tasks.map(task => task.imageUrl).filter(url => !!url);
      }
    }
  }

   // Відкрити прев'ю
   openPreview(imageUrl: string): void {
    this.selectedImage = imageUrl;
  }

  // Закрити прев'ю
  closePreview(): void {
    this.selectedImage = null;
  }
}
