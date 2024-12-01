import {
  Component,
  EventEmitter,
  Input,
  OnChanges,
  Output,
  SimpleChanges,
} from '@angular/core';
import { CdkDrag, CdkDragDrop, CdkDropList, DragDropModule } from '@angular/cdk/drag-drop';
import { FluxModel } from '../../../../models/flux-model';
import { CommonModule } from '@angular/common';
import { JobTask } from '../../../../models/job-task';

@Component({
  selector: 'app-calendar-months',
  standalone: true,
  imports: [CommonModule, DragDropModule, CdkDropList, CdkDrag],
  templateUrl: './calendar-months.component.html',
  styleUrls: ['./calendar-months.component.scss'],
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

  calendarImages: JobTask[] = new Array(12).fill(null); // Массив для зображень місяців
  jobTasks: JobTask[] = []; // Масив для доступних зображень

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

  onDrop(event: CdkDragDrop<any[]>): void {
    const draggedImage = event.item.data;
    console.log(event);
    // Якщо перетягуємо в місяць
    // if (event.container !== event.previousContainer) {
    //   // Визначаємо індекс місяця
    //   const targetIndex = this.months.findIndex((_, i) => this.calendarImages[i].id === event.container.data);

    //   if (targetIndex !== -1) {
    //     // Додаємо зображення до місяця
    //     this.calendarImages[targetIndex] = draggedImage;

    //     // Видаляємо з галереї
    //     const imageIndex = this.jobTasks.findIndex((img) => img.imageUrl === draggedImage.imageUrl);
    //     if (imageIndex !== -1) {
    //       this.images.splice(imageIndex, 1);
    //     }
    //   }
    // } else {
    //   // Якщо скинули в ту саму галерею
    //   console.log('No container switch detected');
    // }
  }

}
