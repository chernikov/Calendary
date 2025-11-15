import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Output } from '@angular/core';

@Component({
    standalone: true,
    selector: 'app-photo-instructions',
    imports: [CommonModule],
    templateUrl: './photo-instructions.component.html',
    styleUrl: './photo-instructions.component.scss'
})
export class PhotoInstructionsComponent {
  @Output() close = new EventEmitter<void>();

  photos = [
    { filename: 'ins_1.jpg', description: 'Лице фас' },
    { filename: 'ins_2.jpg', description: 'Лице фас усміхнене' },
    { filename: 'ins_3.jpg', description: 'Лице фас широко усміхнене' },
    { filename: 'ins_4.jpg', description: 'Лице профіль справа' },
    { filename: 'ins_5.jpg', description: 'Лице профіль зліва' },
    { filename: 'ins_6.jpg', description: 'Лице на 3/4' },
    { filename: 'ins_7.jpg', description: 'Лице на 3/4 усміхнене' },
    { filename: 'ins_8.jpg', description: 'Погруддя прямо' },
    { filename: 'ins_9.jpg', description: 'Повний ріст прямо' },
    { filename: 'ins_10.jpg', description: 'Повний ріст збоку' },
    { filename: 'ins_11.jpg', description: 'Сидячи на стільці' },
    { filename: 'ins_12.jpg', description: 'Сидячи на стільці збоку' }
  ];

  fullImage: string | null = null;

  openFullImage(filename: string) {
    this.fullImage = filename;
  }

  closeFullImage() {
    this.fullImage = null;
  }
  
  closeModal() {
    this.close.emit();
  }
}
