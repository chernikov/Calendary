import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'ui-cta-button',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './cta-button.component.html',
  styleUrl: './cta-button.component.scss',
})
export class CtaButtonComponent {
  @Input() variant: 'primary' | 'secondary' = 'primary';
  @Input() label = '';
  @Output() action = new EventEmitter<Event>();

  emit(event: Event) {
    event.preventDefault();
    this.action.emit(event);
  }
}
