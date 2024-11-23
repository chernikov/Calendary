import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PromptResultsComponent } from './prompt-results.component';

describe('PromptResultsComponent', () => {
  let component: PromptResultsComponent;
  let fixture: ComponentFixture<PromptResultsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PromptResultsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PromptResultsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
