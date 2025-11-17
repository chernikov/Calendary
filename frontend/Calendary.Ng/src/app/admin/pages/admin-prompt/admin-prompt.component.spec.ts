import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminPromptComponent } from './admin-prompt.component';

describe('AdminPromptComponent', () => {
  let component: AdminPromptComponent;
  let fixture: ComponentFixture<AdminPromptComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdminPromptComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AdminPromptComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
