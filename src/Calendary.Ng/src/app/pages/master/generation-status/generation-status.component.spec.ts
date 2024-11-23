import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GenerationStatusComponent } from './generation-status.component';

describe('GenerationStatusComponent', () => {
  let component: GenerationStatusComponent;
  let fixture: ComponentFixture<GenerationStatusComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GenerationStatusComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(GenerationStatusComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
