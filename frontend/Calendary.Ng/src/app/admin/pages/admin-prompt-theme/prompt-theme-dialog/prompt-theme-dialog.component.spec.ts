import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PromptThemeDialogComponent } from './prompt-theme-dialog.component';

describe('PromptThemeDialogComponent', () => {
  let component: PromptThemeDialogComponent;
  let fixture: ComponentFixture<PromptThemeDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PromptThemeDialogComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PromptThemeDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
