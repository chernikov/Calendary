import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TestPromptDialogComponent } from './test-prompt-dialog.component';

describe('TestPromptDialogComponent', () => {
  let component: TestPromptDialogComponent;
  let fixture: ComponentFixture<TestPromptDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TestPromptDialogComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TestPromptDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
