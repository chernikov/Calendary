import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';

export interface FeatureCard {
  title: string;
  description: string;
  icon?: string;
  accent?: 'primary' | 'accent' | 'neutral';
}

@Component({
  selector: 'ui-feature-grid',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './feature-grid.component.html',
  styleUrl: './feature-grid.component.scss',
})
export class FeatureGridComponent {
  @Input() features: FeatureCard[] = [];
}
