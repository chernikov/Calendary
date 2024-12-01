import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CalendarReadyComponent } from './calendar-ready.component';

describe('CalendarReadyComponent', () => {
  let component: CalendarReadyComponent;
  let fixture: ComponentFixture<CalendarReadyComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CalendarReadyComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CalendarReadyComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
