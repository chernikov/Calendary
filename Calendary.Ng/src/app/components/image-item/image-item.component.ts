import { CommonModule } from '@angular/common';
import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { Image } from '../../../models/image';
@Component({
  selector: 'app-image-item',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './image-item.component.html',
  styleUrl: './image-item.component.scss'
})
export class ImageItemComponent implements OnChanges{

  @Input()
  number : number = 0;
  @Input()
  month : string = '';
  @Input()
  image : Image | undefined = undefined;


  ngOnChanges(changes: SimpleChanges): void {
    if (changes['image'] && !changes['image'].firstChange) {
      this.image = changes['image'].currentValue;
    }
  }
}
