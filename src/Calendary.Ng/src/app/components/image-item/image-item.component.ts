import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output, OnChanges, SimpleChanges } from '@angular/core';
import { Image } from '../../../models/image';
@Component({
    standalone: true,
    selector: 'app-image-item',
    imports: [CommonModule],
    templateUrl: './image-item.component.html',
    styleUrl: './image-item.component.scss'
})
export class ImageItemComponent implements OnChanges {
  @Input()
  number: number = 0;

  @Input()
  month: string = '';

  @Input()
  image: Image | undefined = undefined;

  @Output()
  upload: EventEmitter<number> = new EventEmitter<number>();

  @Output()
  delete: EventEmitter<number> = new EventEmitter<number>();

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['image'] && !changes['image'].firstChange) {
      this.image = changes['image'].currentValue;
    }
  }

  startUpload() {
    this.upload.emit(this.number);
  }

  onDelete() {
    this.delete.emit(this.number);
  }
}
