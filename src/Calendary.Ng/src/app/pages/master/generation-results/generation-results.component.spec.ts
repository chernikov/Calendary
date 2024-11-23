import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GenerationResultsComponent } from './generation-results.component';

describe('GenerationResultsComponent', () => {
  let component: GenerationResultsComponent;
  let fixture: ComponentFixture<GenerationResultsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GenerationResultsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(GenerationResultsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
