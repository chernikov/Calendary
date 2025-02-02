import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UserPhotoGalleryComponent } from './user-photo-gallery.component';

describe('UserPhotoGalleryComponent', () => {
  let component: UserPhotoGalleryComponent;
  let fixture: ComponentFixture<UserPhotoGalleryComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UserPhotoGalleryComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(UserPhotoGalleryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
