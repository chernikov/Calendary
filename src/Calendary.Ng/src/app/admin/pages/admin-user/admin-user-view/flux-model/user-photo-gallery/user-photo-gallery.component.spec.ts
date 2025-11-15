import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { of } from 'rxjs';
import { AdminFluxModel } from '../../../../../../../models/flux-model';
import { UploadPhotoService } from '../../../../../../../services/admin/upload-photo.service';
import { UserFluxModelService } from '../../../../../../../services/admin/user-flux-model.service';
import { UserPhotoGalleryComponent } from './user-photo-gallery.component';

describe('UserPhotoGalleryComponent', () => {
  let component: UserPhotoGalleryComponent;
  let fixture: ComponentFixture<UserPhotoGalleryComponent>;

  const fluxModel = new AdminFluxModel();
  fluxModel.id = 1;

  const fluxModelServiceSpy = jasmine.createSpyObj('UserFluxModelService', ['getPhotos', 'getById']);
  const uploadPhotoServiceSpy = jasmine.createSpyObj('UploadPhotoService', ['complete']);

  beforeEach(async () => {
    fluxModelServiceSpy.getPhotos.and.returnValue(of([]));
    fluxModelServiceSpy.getById.and.returnValue(of(fluxModel));
    uploadPhotoServiceSpy.complete.and.returnValue(of({}));

    await TestBed.configureTestingModule({
      imports: [UserPhotoGalleryComponent],
      providers: [
        {
          provide: MAT_DIALOG_DATA,
          useValue: { userId: 1, fluxModel },
        },
        { provide: UserFluxModelService, useValue: fluxModelServiceSpy },
        { provide: UploadPhotoService, useValue: uploadPhotoServiceSpy },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(UserPhotoGalleryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
