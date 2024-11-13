import { Country } from "./country";
import { DayOfWeek } from "./day-of-week.enum";
import { Language } from "./language";

export class Setting 
{
    firstDayOfWeek: DayOfWeek = DayOfWeek.Monday;
    language?: Language;
    country?: Country;
}