import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { Photo } from '../../../../../../../models/photo';
import { UserFluxModelService } from '../../../../../../../services/admin/user-flux-model.service';
import { CommonModule } from '@angular/common';

export interface UserPhotoGalleryData {
  userId: number;
  fluxModelId: number;
}

@Component({
  selector: 'app-user-photo-gallery',
  standalone: true,
  imports: [CommonModule, MatDialogModule],
  templateUrl: './user-photo-gallery.component.html',
  styleUrl: './user-photo-gallery.component.scss'
})
export class UserPhotoGalleryComponent  implements OnInit {
  photos: Photo[] = [];

  constructor(
    private dialogRef: MatDialogRef<UserPhotoGalleryComponent>,
    @Inject(MAT_DIALOG_DATA) public data: UserPhotoGalleryData,
    private fluxModelService: UserFluxModelService
  ) {}

  ngOnInit(): void {
    this.loadPhotos();
  }

  loadPhotos(): void {
    this.fluxModelService.getPhotos(this.data.userId, this.data.fluxModelId).subscribe({
      next: (photos) => this.photos = photos,
      error: (err) => console.error('Помилка завантаження фото', err)
    });
  }

  close(): void {
    this.dialogRef.close();
  }
}
