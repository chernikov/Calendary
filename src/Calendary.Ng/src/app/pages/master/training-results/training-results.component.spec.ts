import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TrainingResultsComponent } from './training-results.component';

describe('TrainingResultsComponent', () => {
  let component: TrainingResultsComponent;
  let fixture: ComponentFixture<TrainingResultsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TrainingResultsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TrainingResultsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
