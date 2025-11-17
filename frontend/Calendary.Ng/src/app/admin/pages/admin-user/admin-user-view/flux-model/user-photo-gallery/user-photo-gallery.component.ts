import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  MAT_DIALOG_DATA,
  MatDialog,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { UploadPhotoComponent } from '../upload-photo/upload-photo.component';
import { Photo } from '../../../../../../../models/photo';
import { UserFluxModelService } from '../../../../../../../services/admin/user-flux-model.service';
import { AdminFluxModel } from '../../../../../../../models/flux-model';
import { UploadPhotoService } from '../../../../../../../services/admin/upload-photo.service';
import { FullScreenPhotoComponent } from '../../../../../components/full-screen-photo.component';

export interface UserPhotoGalleryData {
  userId: number;
  fluxModel: AdminFluxModel;
}

@Component({
    standalone: true,
    selector: 'app-user-photo-gallery',
    imports: [
        CommonModule,
        MatDialogModule,
        MatButtonModule,
        UploadPhotoComponent,
    ],
    templateUrl: './user-photo-gallery.component.html',
    styleUrl: './user-photo-gallery.component.scss'
})
export class UserPhotoGalleryComponent implements OnInit {
  fluxModel: AdminFluxModel | null = null;
  photos: Photo[] = [];
  uploading: boolean = false;

  constructor(
    private dialog: MatDialog,
    private dialogRef: MatDialogRef<UserPhotoGalleryComponent>,
    @Inject(MAT_DIALOG_DATA) public data: UserPhotoGalleryData,
    private fluxModelService: UserFluxModelService,
    private uploadPhotoService: UploadPhotoService
  ) {
    this.fluxModel = data.fluxModel;
  }

  ngOnInit(): void {
    this.loadPhotos();
  }

  loadPhotos(): void {
    this.fluxModelService
      .getPhotos(this.data.userId, this.data.fluxModel.id)
      .subscribe({
        next: (photos) => (this.photos = photos),
        error: (err) => console.error('Помилка завантаження фото', err),
      });
  }

  loadFluxModel(): void {
    this.fluxModelService
      .getById(this.data.fluxModel.id)
      .subscribe({
        next: (fluxModel) => (this.fluxModel = fluxModel),
        error: (err) => console.error('Помилка завантаження', err),
      });
  }

  openFullScreen(photo: any): void {
    this.dialog.open(FullScreenPhotoComponent, {
      panelClass: 'full-screen-dialog',
      // disableClose: false за замовчуванням дозволяє закривати по кліку на бекграунд,
      data: { imageUrl: photo.imageUrl },
    });
  }

  onUploadComplete(): void {
    this.loadPhotos();
  }

  complete(): void {
    if (this.photos.length && this.photos.length < 12) {
      alert('Завантажте 12 фото');
    }
    this.uploadPhotoService.complete(this.fluxModel!.id).subscribe({
      next: (response) => {
        console.log('Завантаження завершено:', response);
        this.loadPhotos();
        this.loadFluxModel();
      },
      error: (err) => {
        console.error('Помилка завершення завантаження:', err);
      },
    });
  }

  photoDisabled() {
    return !!this.photos && this.photos?.length < 12;
  }

  close(): void {
    this.dialogRef.close();
  }
}
