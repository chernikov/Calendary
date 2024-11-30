import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PromptHistoryComponent } from './prompt-history.component';

describe('PromptHistoryComponent', () => {
  let component: PromptHistoryComponent;
  let fixture: ComponentFixture<PromptHistoryComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PromptHistoryComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PromptHistoryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
