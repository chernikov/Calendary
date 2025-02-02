import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SynthesisDialogComponent } from './synthesis-dialog.component';

describe('SynthesisDialogComponent', () => {
  let component: SynthesisDialogComponent;
  let fixture: ComponentFixture<SynthesisDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SynthesisDialogComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SynthesisDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
