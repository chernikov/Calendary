import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'ui-color-picker',
  standalone: true,
  imports: [CommonModule, FormsModule, MatButtonModule],
  templateUrl: './color-picker.component.html',
  styleUrl: './color-picker.component.scss',
})
export class ColorPickerComponent {
  @Input() label = '';
  @Input() value = '#000000';
  @Output() valueChange = new EventEmitter<string>();

  readonly presets = ['#111827', '#6b7280', '#d97706', '#059669', '#2563eb', '#ef4444'];

  onSelect(color: string): void {
    this.value = color;
    this.valueChange.emit(color);
  }

  onInputChange(value: string): void {
    this.value = value;
    this.valueChange.emit(value);
  }
}
