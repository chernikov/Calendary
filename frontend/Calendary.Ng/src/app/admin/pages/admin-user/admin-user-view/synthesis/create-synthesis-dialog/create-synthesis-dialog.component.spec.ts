import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CreateSynthesisDialogComponent } from './create-synthesis-dialog.component';

describe('CreateSynthesisDialogComponent', () => {
  let component: CreateSynthesisDialogComponent;
  let fixture: ComponentFixture<CreateSynthesisDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CreateSynthesisDialogComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CreateSynthesisDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
