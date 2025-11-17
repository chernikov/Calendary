import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PromptSelectionComponent } from './prompt-selection.component';

describe('PromptSelectionComponent', () => {
  let component: PromptSelectionComponent;
  let fixture: ComponentFixture<PromptSelectionComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PromptSelectionComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PromptSelectionComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
