import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { CalendarService } from '../../../../../../../services/admin/calendar.service';
import { UserCalendarListComponent } from './user-calendar-list.component';

describe('UserCalendarListComponent', () => {
  let component: UserCalendarListComponent;
  let fixture: ComponentFixture<UserCalendarListComponent>;
  let calendarServiceSpy: jasmine.SpyObj<CalendarService>;

  beforeEach(async () => {
    calendarServiceSpy = jasmine.createSpyObj('CalendarService', ['getByUserId']);
    calendarServiceSpy.getByUserId.and.returnValue(of([]));

    await TestBed.configureTestingModule({
      imports: [UserCalendarListComponent],
      providers: [{ provide: CalendarService, useValue: calendarServiceSpy }],
    }).compileComponents();

    fixture = TestBed.createComponent(UserCalendarListComponent);
    component = fixture.componentInstance;
    component.userId = 1;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
