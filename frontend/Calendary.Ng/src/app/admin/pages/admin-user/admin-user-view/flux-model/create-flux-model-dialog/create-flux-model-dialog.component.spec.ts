import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CreateFluxModelDialogComponent } from './create-flux-model-dialog.component';

describe('CreateFluxModelDialogComponent', () => {
  let component: CreateFluxModelDialogComponent;
  let fixture: ComponentFixture<CreateFluxModelDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CreateFluxModelDialogComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CreateFluxModelDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
