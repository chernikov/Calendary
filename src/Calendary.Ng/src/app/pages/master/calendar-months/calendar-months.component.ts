import {
  Component,
  EventEmitter,
  Input,
  OnChanges,
  Output,
  SimpleChanges,
} from '@angular/core';
import { CdkDragDrop, DragDropModule } from '@angular/cdk/drag-drop';
import { FluxModel } from '../../../../models/flux-model';
import { CommonModule } from '@angular/common';
import { JobTask } from '../../../../models/job-task';

@Component({
  selector: 'app-calendar-months',
  standalone: true,
  imports: [CommonModule, DragDropModule],
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

  onDrop(event: any): void {
    debugger;
    console.log("onDrop", event);
    // if (i === null) {
    //   const droppedImage = event.item.data as JobTask;
    //   this.images.push(droppedImage);
    //   console.log("Вернути назад", droppedImage);
    //   return;
    // }
    // const monthIndex = i;
    // const droppedImage = event.item.data as JobTask;

    // this.calendarImages[monthIndex] = droppedImage;
    // const index = this.images.indexOf(droppedImage);

    // console.log("Поставити на " + index, droppedImage);
    // if (index !== -1) {
    //   this.images.splice(index, 1);
    // }
  }

}
