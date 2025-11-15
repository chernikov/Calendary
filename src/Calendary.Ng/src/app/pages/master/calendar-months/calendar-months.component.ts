import {
  Component,
  EventEmitter,
  Input,
  OnChanges,
  Output,
  SimpleChanges,
} from '@angular/core';
import {
  CdkDrag,
  CdkDragDrop,
  CdkDropList,
  DragDropModule,
} from '@angular/cdk/drag-drop';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { FluxModel } from '../../../../models/flux-model';
import { JobTask } from '../../../../models/job-task';
import { CalendarService } from '../../../../services/calendar.service';
import { FillCalendar } from '../../../../models/requests/fill-calendar';

import { Image } from '../../../../models/image';
@Component({
    selector: 'app-calendar-months',
    imports: [CommonModule, DragDropModule, CdkDropList, CdkDrag, MatButtonModule],
    templateUrl: './calendar-months.component.html',
    styleUrls: ['./calendar-months.component.scss']
})
export class CalendarMonthsComponent implements OnChanges {
  @Input()
  fluxModel: FluxModel | null = null;

  @Output()
  onUpdate = new EventEmitter<FluxModel>();

  months = [
    'Січень',
    'Лютий',
    'Березень',
    'Квітень',
    'Травень',
    'Червень',
    'Липень',
    'Серпень',
    'Вересень',
    'Жовтень',
    'Листопад',
    'Грудень',
  ];

  calendarImages: (JobTask | null)[] = new Array(12).fill(null); // Массив для зображень місяців
  jobTasks: JobTask[] = []; // Масив для доступних зображень
  selectedImage: string | null = null; // Для прев'ю

  constructor(private calendarService: CalendarService) 
  {

  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['fluxModel'] && changes['fluxModel'].currentValue) {
      this.fluxModel = changes['fluxModel'].currentValue;
      this.loadImageUrls();
    }
  }

  private loadImageUrls(): void {
    this.jobTasks = [];

    if (this.fluxModel?.jobs && this.fluxModel.jobs.length > 0) {
      const lastJob = this.fluxModel.jobs[this.fluxModel.jobs.length - 1];
      if (lastJob.tasks && lastJob.tasks.length > 0) {
        this.jobTasks = lastJob.tasks.filter((task) => !!task.imageUrl);
      }
    }
  }

  onDrop(event: CdkDragDrop<any[]>, month_index: number): void {
    const draggedJobTask = this.jobTasks[event.previousIndex];

    if (event.container !== event.previousContainer) 
      {
        if (!this.calendarImages[month_index]) {
          this.calendarImages[month_index] = draggedJobTask;
        }
      
      const jobTaskIndex = this.jobTasks.findIndex(
        (img) => img.id === draggedJobTask.id
      );
      if (jobTaskIndex !== -1) {
        this.jobTasks.splice(jobTaskIndex, 1);
      }
    }
  }

  removeImage(month_index: number) {
    const removedImage = this.calendarImages[month_index];
    if (removedImage) {
      this.jobTasks.push(removedImage);
      this.calendarImages[month_index] = null;
    }
  }

  autoPlaceImages(): void {
    // Перебираємо перші 12 слотів
    for (let i = 0; i < 12; i++) {
      // Пропускаємо, якщо слот уже заповнений
      if (this.calendarImages[i]) {
        continue;
      }
  
      // Якщо зображень у галереї немає, завершуємо
      if (this.jobTasks.length === 0) {
        break;
      }
  
      // Витягуємо перше доступне зображення та призначаємо його до місяця
      const task = this.jobTasks.shift();
      this.calendarImages[i] = task || null;
    }
  }

   // Відкрити зображення у модальному вікні
   openPreview(imageUrl: string): void {
    this.selectedImage = imageUrl;
  }

  // Закрити модальне вікно
  closePreview(): void {
    this.selectedImage = null;
  }
  
  isCalendarFilled(): boolean {
    return this.calendarImages.every((image) => !!image);
  }

  saveCalendar(): void {
    if (!this.isCalendarFilled() || !this.fluxModel) {
      return;
    }

    const fillCalendar = new FillCalendar();
    fillCalendar.fluxModelId = this.fluxModel.id;

    this.calendarImages.forEach((image, index) => {
      if (image) {
        const saveImage = new Image();
        saveImage.imageUrl = image.imageUrl;
        saveImage.monthNumber = index;
        fillCalendar.images.push(saveImage);
      }
    });

    this.calendarService.fill(fillCalendar).subscribe({
      next: () => {
        alert('Календар збережено успішно!');
        this.onUpdate.emit();
      },
      error: () => {
        alert('Сталася помилка при збереженні календаря.');
      }
    });
  }
}
