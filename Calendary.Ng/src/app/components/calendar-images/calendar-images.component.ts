import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { FileUploader, FileUploadModule } from 'ng2-file-upload';
import { ImageService } from '../../../services/image.service';
import { Image } from '../../../models/image';
import { MatIconModule } from '@angular/material/icon';
@Component({
  selector: 'app-calendar-images',
  standalone: true,
  imports: [CommonModule, FileUploadModule, MatIconModule],
  templateUrl: './calendar-images.component.html',
  styleUrl: './calendar-images.component.scss'
})
export class CalendarImagesComponent implements OnChanges, OnInit {
  @Input() calendarId!: number;

  images: Image[] = [];

  public uploader!: FileUploader;
  
  @Output() uploadCompleted = new EventEmitter<string>();


  constructor(private imageService : ImageService) 
  {
   
  }

  ngOnInit(): void {
    this.initializeUploader();
    this.loadImages();
  }


  ngOnChanges(changes: SimpleChanges): void {
    if (changes['calendarId'] && !changes['calendarId'].firstChange) {
      this.initializeUploader();
    }
  }

  private initializeUploader(): void {
    if (this.calendarId) {
      this.uploader = new FileUploader({
        url: `/api/image/upload/${this.calendarId}`,
        disableMultipart: false,
        autoUpload: true, // Автоматичне завантаження
        allowedFileType: ['image'], // Дозволяємо лише зображення
        maxFileSize: 5 * 1024 * 1024 // Максимальний розмір 5MB
      });

      this.uploader.onCompleteItem = (item, response, status, headers) => {
        this.loadImages();
      };

      this.uploader.onErrorItem = (item, response, status, headers) => {
        console.error('Помилка завантаження:', response);
      };
    }
  }

  loadImages(): void {
    // Assuming imageService is injected and has a method to get images by calendarId
    this.imageService.getImages(this.calendarId).subscribe(
      (images: Image[]) => {
        this.images = images;
      },
      (error) => {
        console.error('Error loading images:', error);
      }
    );
  }

  deleteImage(index: number): void {
    const image = this.images[index];
    this.imageService.deleteImage(image.id).subscribe(() => {
      this.loadImages();
    });
  }
}