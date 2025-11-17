export interface HolidayPresetTranslation {
    id: number;
    holidayPresetId: number;
    languageId: number;
    name: string;
}

export interface HolidayTranslation {
    id: number;
    holidayId: number;
    languageId: number;
    name: string;
}

export interface Holiday {
    id: number;
    month?: number;
    day?: number;
    isMovable: boolean;
    calculationType?: string;
    offsetDays: number;
    type: string;
    isWorkingDay: boolean;
    holidayPresetId?: number;
    translations?: HolidayTranslation[];
}

export interface HolidayPreset {
    id: number;
    code: string;
    type: string;
    translations?: HolidayPresetTranslation[];
    holidays?: Holiday[];
}

export interface HolidayPresetSummary {
    id: number;
    code: string;
    type: string;
    name: string;
    holidayCount: number;
}

export interface ApplyPresetRequest {
    calendarId: number;
    presetCode: string;
}
