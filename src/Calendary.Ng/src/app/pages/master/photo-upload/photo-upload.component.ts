import {
  Component,
  Input,
  OnChanges,
  SimpleChanges,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FileUploader, FileUploadModule } from 'ng2-file-upload';
import { FluxModel } from '../../../../models/flux-model';
import { FluxModelService } from '../../../../services/flux-model.service';
import { UploadPhotoService } from '../../../../services/upload-photo.service';

@Component({
  selector: 'app-photo-upload',
  standalone: true,
  imports: [CommonModule, FormsModule, FileUploadModule],
  templateUrl: './photo-upload.component.html',
  styleUrl: './photo-upload.component.scss',
})
export class PhotoUploadComponent implements OnChanges {
  @Input() fluxModel: FluxModel | null = null;

  uploader: FileUploader;
  uploading: boolean = false;
  progress: number = 0;
  uploadSuccess: boolean = false;

  constructor(private fluxModelService: FluxModelService,
    private uploadPhotoService: UploadPhotoService
  ) {
    // Ініціалізація FileUploader
    this.uploader = new FileUploader({
      url: '/api/upload-photo',
      autoUpload: false,
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['fluxModel'] && changes['fluxModel'].currentValue) {
      this.fluxModel = changes['fluxModel'].currentValue;
    }
  }

  async uploadPhotos(): Promise<void> {
    if (this.uploader.queue.length < 12) {
      alert('Будь ласка, виберіть не менше 12 фотографій.');
      return;
    }

    this.uploading = true;
    this.uploadSuccess = false;
    this.progress = 0;
    // Додаємо FluxModelId до кожного запиту
    this.uploader.onBuildItemForm = (fileItem, form) => {
      form.append('fluxModelId', this.fluxModel!.id);
    };

    // Завантаження файлів
    this.uploader.onProgressAll = (progress: any) => {
      this.progress = progress;
    };

    this.uploader.onCompleteAll = async () => {

      this.uploadPhotoService.complete(this.fluxModel!.id).subscribe({
        next: () => {
          console.log('Фотографії завантажено!');
          this.uploading = false;
          this.uploadSuccess = true;
          this.uploader.clearQueue(); // Очищення черги
        },
        error: (err) => {
          console.error('Помилка завантаження фотографій:', err);
        }
      });
    };

    console.log('Uploader queue:', this.uploader.queue);
    this.uploader.uploadAll();
  }
}
