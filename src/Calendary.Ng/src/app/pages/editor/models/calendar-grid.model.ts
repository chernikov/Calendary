import { Holiday } from '../../../models/holiday';

export interface CalendarDay {
  day: number;
  date: Date;
  isWeekend: boolean;
  isHoliday: boolean;
  holidayName?: string;
  isCurrentMonth: boolean;
}

export interface CalendarMonth {
  month: number; // 1-12
  year: number;
  monthName: string;
  days: CalendarDay[];
  imageUrl?: string;
}

export interface CalendarCustomization {
  fontFamily: string;
  fontSize: number;
  primaryColor: string;
  secondaryColor: string;
  holidayColor: string;
  weekendColor: string;
  layoutPosition: 'top' | 'bottom'; // image position
}

export const DEFAULT_CUSTOMIZATION: CalendarCustomization = {
  fontFamily: 'Arial, sans-serif',
  fontSize: 14,
  primaryColor: '#111827',
  secondaryColor: '#6b7280',
  holidayColor: '#dc2626',
  weekendColor: '#3b82f6',
  layoutPosition: 'top',
};

export function generateCalendarMonth(
  month: number,
  year: number,
  holidays: Holiday[] = []
): CalendarMonth {
  const days: CalendarDay[] = [];
  const firstDay = new Date(year, month - 1, 1);
  const lastDay = new Date(year, month, 0);
  const daysInMonth = lastDay.getDate();

  // Get day of week (0 = Sunday, 1 = Monday, etc.)
  let startDayOfWeek = firstDay.getDay();
  // Convert to Monday-based (0 = Monday, 6 = Sunday)
  startDayOfWeek = startDayOfWeek === 0 ? 6 : startDayOfWeek - 1;

  // Add days from previous month
  const prevMonth = month === 1 ? 12 : month - 1;
  const prevYear = month === 1 ? year - 1 : year;
  const daysInPrevMonth = new Date(prevYear, prevMonth, 0).getDate();

  for (let i = startDayOfWeek - 1; i >= 0; i--) {
    const day = daysInPrevMonth - i;
    const date = new Date(prevYear, prevMonth - 1, day);
    days.push({
      day,
      date,
      isWeekend: date.getDay() === 0 || date.getDay() === 6,
      isHoliday: false,
      isCurrentMonth: false,
    });
  }

  // Add days of current month
  for (let day = 1; day <= daysInMonth; day++) {
    const date = new Date(year, month - 1, day);
    const holiday = holidays.find(
      (h) =>
        new Date(h.date).getMonth() === month - 1 &&
        new Date(h.date).getDate() === day
    );

    days.push({
      day,
      date,
      isWeekend: date.getDay() === 0 || date.getDay() === 6,
      isHoliday: !!holiday,
      holidayName: holiday?.name,
      isCurrentMonth: true,
    });
  }

  // Add days from next month to complete the grid
  const remainingDays = 42 - days.length; // 6 weeks * 7 days
  const nextMonth = month === 12 ? 1 : month + 1;
  const nextYear = month === 12 ? year + 1 : year;

  for (let day = 1; day <= remainingDays; day++) {
    const date = new Date(nextYear, nextMonth - 1, day);
    days.push({
      day,
      date,
      isWeekend: date.getDay() === 0 || date.getDay() === 6,
      isHoliday: false,
      isCurrentMonth: false,
    });
  }

  const monthNames = [
    'Січень',
    'Лютий',
    'Березень',
    'Квітень',
    'Травень',
    'Червень',
    'Липень',
    'Серпень',
    'Вересень',
    'Жовтень',
    'Листопад',
    'Грудень',
  ];

  return {
    month,
    year,
    monthName: monthNames[month - 1],
    days,
  };
}
