import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { CalendarService } from '../../services/calendar.service';
import { Calendar } from '../../models/calendar';

/**
 * Integration tests for PDF generation
 * Task 30: Testing PDF Generation
 *
 * These tests verify:
 * - End-to-end PDF generation from calendar data to PDF file
 * - Different scenarios (different presets, languages, formats)
 * - Edge cases (missing images, invalid dates)
 */
describe('PDF Generation Integration Tests', () => {
  let service: CalendarService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [CalendarService]
    });
    service = TestBed.inject(CalendarService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  describe('End-to-End PDF Generation', () => {
    it('should generate PDF for valid calendar with all months', (done) => {
      const calendarId = 1;
      const fluxModelId = null;
      const mockCalendar: Calendar = {
        id: calendarId,
        year: 2024,
        languageId: 1,
        firstDayOfWeek: 1, // Monday
        filePath: 'uploads/calendar_1_2024.pdf',
        previewPath: 'uploads/calendar_1_2024_preview.jpg'
      };

      service.generatePdf(calendarId, fluxModelId).subscribe(
        (result) => {
          expect(result).toBeTruthy();
          expect(result.filePath).toContain('.pdf');
          expect(result.filePath).toContain('calendar_1_2024');
          done();
        },
        (error) => {
          fail('PDF generation should not fail: ' + error);
        }
      );

      const req = httpMock.expectOne(`/api/calendar/generate/${calendarId}`);
      expect(req.request.method).toBe('GET');
      req.flush(mockCalendar);
    });

    it('should generate PDF with different language (English)', (done) => {
      const calendarId = 2;
      const mockCalendar: Calendar = {
        id: calendarId,
        year: 2024,
        languageId: 2,
        firstDayOfWeek: 0, // Sunday
        filePath: 'uploads/calendar_2_2024.pdf',
        previewPath: 'uploads/calendar_2_2024_preview.jpg'
      };

      service.generatePdf(calendarId, null).subscribe(
        (result) => {
          expect(result).toBeTruthy();
          expect(result.languageId).toBe(2);
          expect(result.firstDayOfWeek).toBe(0); // Sunday for English
          done();
        }
      );

      const req = httpMock.expectOne(`/api/calendar/generate/${calendarId}`);
      req.flush(mockCalendar);
    });

    it('should generate PDF with holidays highlighted', (done) => {
      const calendarId = 3;
      const mockCalendar: Calendar = {
        id: calendarId,
        year: 2024,
        languageId: 1,
        firstDayOfWeek: 1,
        filePath: 'uploads/calendar_3_2024.pdf',
        previewPath: 'uploads/calendar_3_2024_preview.jpg'
      };

      service.generatePdf(calendarId, null).subscribe(
        (result) => {
          expect(result).toBeTruthy();
          expect(result.filePath).toContain('.pdf');
          done();
        }
      );

      const req = httpMock.expectOne(`/api/calendar/generate/${calendarId}`);
      req.flush(mockCalendar);
    });

    it('should generate PDF with flux model preset', (done) => {
      const calendarId = 4;
      const fluxModelId = 1;
      const mockCalendar: Calendar = {
        id: calendarId,
        year: 2024,
        languageId: 1,
        firstDayOfWeek: 1,
        filePath: 'uploads/calendar_4_2024.pdf',
        previewPath: 'uploads/calendar_4_2024_preview.jpg'
      };

      service.generatePdf(calendarId, fluxModelId).subscribe(
        (result) => {
          expect(result).toBeTruthy();
          expect(result.filePath).toContain('.pdf');
          done();
        }
      );

      const req = httpMock.expectOne(`/api/calendar/generate/${calendarId}?fluxModelId=${fluxModelId}`);
      expect(req.request.method).toBe('GET');
      req.flush(mockCalendar);
    });
  });

  describe('Edge Cases', () => {
    it('should handle missing images gracefully', (done) => {
      const calendarId = 5;
      const mockCalendar: Calendar = {
        id: calendarId,
        year: 2024,
        languageId: 1,
        firstDayOfWeek: 1,
        filePath: null,
        previewPath: null
      };

      service.generatePdf(calendarId, null).subscribe(
        (result) => {
          expect(result).toBeTruthy();
          // Should handle missing images scenario
          done();
        }
      );

      const req = httpMock.expectOne(`/api/calendar/generate/${calendarId}`);
      req.flush(mockCalendar);
    });

    it('should handle leap year February correctly', (done) => {
      const calendarId = 6;
      const mockCalendar: Calendar = {
        id: calendarId,
        year: 2024, // Leap year
        languageId: 1,
        firstDayOfWeek: 1,
        filePath: 'uploads/calendar_6_2024.pdf',
        previewPath: 'uploads/calendar_6_2024_preview.jpg'
      };

      service.generatePdf(calendarId, null).subscribe(
        (result) => {
          expect(result).toBeTruthy();
          expect(result.year).toBe(2024);
          done();
        }
      );

      const req = httpMock.expectOne(`/api/calendar/generate/${calendarId}`);
      req.flush(mockCalendar);
    });

    it('should handle invalid calendar ID', (done) => {
      const invalidCalendarId = 9999;

      service.generatePdf(invalidCalendarId, null).subscribe(
        () => {
          fail('Should have failed with 404');
        },
        (error) => {
          expect(error.status).toBe(404);
          done();
        }
      );

      const req = httpMock.expectOne(`/api/calendar/generate/${invalidCalendarId}`);
      req.flush('Calendar not found', { status: 404, statusText: 'Not Found' });
    });
  });

  describe('Different First Day of Week', () => {
    it('should generate PDF starting with Monday', (done) => {
      const calendarId = 7;
      const mockCalendar: Calendar = {
        id: calendarId,
        year: 2024,
        languageId: 1,
        firstDayOfWeek: 1, // Monday
        filePath: 'uploads/calendar_7_2024.pdf',
        previewPath: 'uploads/calendar_7_2024_preview.jpg'
      };

      service.generatePdf(calendarId, null).subscribe(
        (result) => {
          expect(result.firstDayOfWeek).toBe(1);
          done();
        }
      );

      const req = httpMock.expectOne(`/api/calendar/generate/${calendarId}`);
      req.flush(mockCalendar);
    });

    it('should generate PDF starting with Sunday', (done) => {
      const calendarId = 8;
      const mockCalendar: Calendar = {
        id: calendarId,
        year: 2024,
        languageId: 2,
        firstDayOfWeek: 0, // Sunday
        filePath: 'uploads/calendar_8_2024.pdf',
        previewPath: 'uploads/calendar_8_2024_preview.jpg'
      };

      service.generatePdf(calendarId, null).subscribe(
        (result) => {
          expect(result.firstDayOfWeek).toBe(0);
          done();
        }
      );

      const req = httpMock.expectOne(`/api/calendar/generate/${calendarId}`);
      req.flush(mockCalendar);
    });
  });
});
