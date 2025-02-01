import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UserCalendarListComponent } from './user-calendar-list.component';

describe('UserCalendarListComponent', () => {
  let component: UserCalendarListComponent;
  let fixture: ComponentFixture<UserCalendarListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UserCalendarListComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(UserCalendarListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
