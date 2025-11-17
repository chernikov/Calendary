import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminPromptThemeComponent } from './admin-prompt-theme.component';

describe('AdminPromptThemeComponent', () => {
  let component: AdminPromptThemeComponent;
  let fixture: ComponentFixture<AdminPromptThemeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdminPromptThemeComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AdminPromptThemeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
