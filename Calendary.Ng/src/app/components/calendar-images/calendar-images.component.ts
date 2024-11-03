import { CommonModule } from '@angular/common';
import { Component, ElementRef, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges, ViewChild } from '@angular/core';
import { FileUploader, FileUploadModule } from 'ng2-file-upload';
import { ImageService } from '../../../services/image.service';
import { Image } from '../../../models/image';
import { MatIconModule } from '@angular/material/icon';
import { CalendarService } from '../../../services/calendar.service';
import { ImageItemComponent } from '../image-item/image-item.component';
@Component({
  selector: 'app-calendar-images',
  standalone: true,
  imports: [CommonModule, FileUploadModule, MatIconModule, ImageItemComponent],
  templateUrl: './calendar-images.component.html',
  styleUrl: './calendar-images.component.scss'
})
export class CalendarImagesComponent implements OnChanges, OnInit {


  @Input() calendarId!: number;
  @Output() uploadCompleted = new EventEmitter<string>();
  @Output() generatedCompleted = new EventEmitter<boolean>();
  
  @ViewChild('fileInput') fileInput!: ElementRef;

  items = Array.from({length: 12}, (_, i) => i + 1);
  monthes = ['Січень', 'Лютий', 'Березень', 'Квітень', 'Травень', 'Червень', 'Липень', 'Серпень', 'Вересень', 'Жовтень', 'Листопад', 'Грудень'];
  images: Image[] = [];

  public uploader!: FileUploader;
  selectedMonth: number = 0;
  
  constructor(private imageService : ImageService, 
    private calendarService: CalendarService
  ) 
  {
   
  }

  ngOnInit(): void {
    this.initializeUploader();
    this.loadImages();
  }


  ngOnChanges(changes: SimpleChanges): void {
    if (changes['calendarId'] && !changes['calendarId'].firstChange) 
    {
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

      this.uploader.onBeforeUploadItem = (item) => {
        item.url = `/api/image/upload/${this.calendarId}?month=${this.selectedMonth}`;
      };

      this.uploader.onCompleteItem = (item, response, status, headers) => {
        this.loadImages();
      };

      this.uploader.onErrorItem = (item, response, status, headers) => {
        console.error('Помилка завантаження:', response);
      };
    }
  }

  getImage(monthNumber : number): Image | undefined {
    return this.images.find(i => i.monthNumber === monthNumber);
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

  onStartUpload(monthNumber: number): void 
  {
    this.selectedMonth = monthNumber;
    this.fileInput.nativeElement.click();
  }

  deleteImage(monthNumber: number): void {
    const image = this.images.find(i => i.monthNumber === monthNumber);
    if (!image) {
      return;
    }
    this.imageService.deleteImage(image.id).subscribe(() => {
      this.loadImages();
    });
  }

  canGeneratePdf(): boolean {
    return this.images.length === 12;
  }

  generatePdf(): void {
    // Виклик API для генерації PDF
    this.calendarService.generatePdf(this.calendarId).subscribe(response => {
      this.generatedCompleted.emit(true);
    });
  }
}