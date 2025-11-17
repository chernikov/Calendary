import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { FluxModel } from '../../../../models/flux-model';
import { Job } from '../../../../models/job';
import { CommonModule } from '@angular/common';
import { JobTask } from '../../../../models/job-task';

@Component({
    standalone: true,
    selector: 'app-prompt-results',
    imports: [CommonModule],
    templateUrl: './prompt-results.component.html',
    styleUrl: './prompt-results.component.scss'
})
export class PromptResultsComponent implements OnChanges {
  @Input() tasks: JobTask[] = [];
  selectedImageUrl: string | null = null;
  
  constructor() {}

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['tasks'] && changes['tasks'].currentValue) {
      this.tasks = changes['tasks'].currentValue;
    }
  }

  openImage(imageUrl: string): void {
    this.selectedImageUrl = imageUrl;
  }

  closeImage(): void {
    this.selectedImageUrl = null;
  }
}
