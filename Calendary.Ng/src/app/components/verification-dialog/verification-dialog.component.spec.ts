import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VerificationDialogComponent } from './verification-dialog.component';

describe('VerificationDialogComponent', () => {
  let component: VerificationDialogComponent;
  let fixture: ComponentFixture<VerificationDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VerificationDialogComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VerificationDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
