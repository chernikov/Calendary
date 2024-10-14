import { DayOfWeek } from "./day-of-week.enum";

export class Calendar {
    id: number = 0;
    year: number = 0;
    firstDayOfWeek: DayOfWeek = DayOfWeek.Monday;
    languageId: number = 0;
  }