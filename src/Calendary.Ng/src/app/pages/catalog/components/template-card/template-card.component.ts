import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { RouterModule } from '@angular/router';
import { TemplateSummary } from '../../../../../models/template';

@Component({
  selector: 'app-template-card',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatButtonModule, MatIconModule, RouterModule],
  templateUrl: './template-card.component.html',
  styleUrl: './template-card.component.scss',
})
export class TemplateCardComponent {
  @Input({ required: true }) template!: TemplateSummary;
  @Input() highlight = false;
  @Output() preview = new EventEmitter<TemplateSummary>();

  onPreview(event: MouseEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.preview.emit(this.template);
  }
}
