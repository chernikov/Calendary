import { CommonModule, NgClass } from '@angular/common';
import { Component, Input } from '@angular/core';

@Component({
  selector: 'ui-section',
  standalone: true,
  imports: [CommonModule, NgClass],
  templateUrl: './section.component.html',
  styleUrl: './section.component.scss',
})
export class SectionComponent {
  @Input() title = '';
  @Input() subtitle = '';
  @Input() eyebrow = '';
  @Input() alignment: 'left' | 'center' = 'center';
  @Input() variant: 'default' | 'surface' | 'muted' | 'highlight' = 'default';
  @Input() fullWidth = false;

  get sectionClasses() {
    return {
      'ui-section--surface': this.variant === 'surface',
      'ui-section--muted': this.variant === 'muted',
      'ui-section--highlight': this.variant === 'highlight',
      'ui-section--full': this.fullWidth,
    };
  }

  get headerAlignment() {
    return {
      'ui-section__header--center': this.alignment === 'center',
      'ui-section__header--left': this.alignment === 'left',
    };
  }
}
