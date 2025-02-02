import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { FileUploader, FileUploadModule } from 'ng2-file-upload';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UploadPhotoService } from '../../../../../../../services/admin/upload-photo.service';


@Component({
  selector: 'app-upload-photo',
  standalone: true,
  imports: [CommonModule, FormsModule, FileUploadModule],
  templateUrl: './upload-photo.component.html',
  styleUrl: './upload-photo.component.scss',
})
export class UploadPhotoComponent implements OnChanges {
  @Input() fluxModelId: number | null = null;
  @Output() uploadComplete = new EventEmitter<void>();
  
  isModalOpen = false;

  uploader: FileUploader;
  uploading: boolean = false;
  progress: number = 0;
  uploadSuccess: boolean = false;

  constructor(
    private uploadPhotoService: UploadPhotoService
  ) {
    // Ініціалізація FileUploader
    this.uploader = new FileUploader({
      url: '/api/admin/user-upload-photo',
      autoUpload: false,
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['fluxModelId'] && changes['fluxModelId'].currentValue) {
      this.fluxModelId = changes['fluxModelId'].currentValue;
    }
  }

  async uploadPhotos(): Promise<void> {

    this.uploading = true;
    this.uploadSuccess = false;
    this.progress = 0;
    // Додаємо FluxModelId до кожного запиту
    this.uploader.onBuildItemForm = (fileItem, form) => {
      form.append('fluxModelId', this.fluxModelId);
    };

    // Завантаження файлів
    this.uploader.onProgressAll = (progress: any) => {
      this.progress = progress;
    };

    this.uploader.onCompleteAll = async () => {
      this.uploading = false;
      this.uploadSuccess = true;
      this.uploader.clearQueue(); // Очищення черги
      // Емітуємо подію, щоб батьківський компонент знав, що фото завантажено
      this.uploadComplete.emit();
    };

    console.log('Uploader queue:', this.uploader.queue);
    this.uploader.uploadAll();
  }

  complete() {
    this.uploadPhotoService.complete(this.fluxModelId!).subscribe({
      next: () => {
        console.log('Фотографії завантажено!');
        
        window.location.reload();
      },
      error: (err) => {
        console.error('Помилка завантаження фотографій:', err);
      }
    });
  }
}
