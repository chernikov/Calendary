import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ImageGenerationComponent } from './image-generation.component';

describe('ImageGenerationComponent', () => {
  let component: ImageGenerationComponent;
  let fixture: ComponentFixture<ImageGenerationComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ImageGenerationComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ImageGenerationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
