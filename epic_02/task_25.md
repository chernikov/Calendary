# Task 25: Завантаження PDF (цифровий продукт)

**Epic**: [Epic 02 - Customer Portal](../epic_02.md)
**Статус**: TODO
**Пріоритет**: P1 (Високий)
**Складність**: Низька
**Час**: 3-4 години
**Відповідальний AI**: Codex
**Залежить від**: Task 24
**Паралельно з**: Task 21, 22, 23, 26

## Опис задачі

Створити download endpoint для PDF, обмеження доступу (тільки після оплати або для цифрового продукту).

## Проблема

Користувачі повинні мати можливість завантажити PDF календаря після оплати або як цифровий продукт.

## Що треба зробити

1. **Backend: Download Endpoint**
   - `GET /api/calendars/{id}/download-pdf`
   - Authorization: тільки власник календаря
   - Перевірка: чи оплачено замовлення (якщо фізичний)
   - Для цифрового: доступно відразу після оплати
   - Return: PDF file stream

2. **Access Control**
   - Якщо календар у замовленні - перевірити payment status
   - Якщо digital product - перевірити payment
   - Генерувати одноразові download links (опціонально)
   - Rate limiting (запобігти зловживанню)

3. **Frontend: Download Button**
   - В Order Details сторінці
   - В Profile → My Calendars
   - Disabled якщо не оплачено
   - Loading state під час генерації PDF

4. **Digital Product Flow**
   - Checkbox в checkout "Тільки PDF (без друку)"
   - Нижча ціна
   - Миттєве завантаження після оплати
   - Email з download link

5. **Download Links з Expiry**
   - Генерувати signed URLs
   - Valid 24 години
   - Можна згенерувати повторно

6. **Analytics**
   - Track downloads
   - Скільки разів завантажено
   - Якщо багато - можливо sharing (investigate)

## Файли для створення/модифікації

- `src/Calendary.API/Controllers/CalendarsController.cs` - download endpoint
- `src/Calendary.Application/Services/DownloadLinkService.cs`
- `src/components/features/orders/DownloadPdfButton.tsx`
- `src/components/features/checkout/DigitalProductOption.tsx`
- `src/services/calendarService.ts` - frontend download

## Критерії успіху

- [ ] Можна завантажити PDF після оплати
- [ ] Access control працює (тільки власник)
- [ ] Digital product option в checkout працює
- [ ] Email з download link відправляється
- [ ] Rate limiting працює
- [ ] Download button показує правильний стан

## Залежності

- Task 24: PDF generation повинна працювати

## Технічні деталі

### CalendarsController Download Endpoint
```csharp
[HttpGet("{id}/download-pdf")]
[Authorize]
public async Task<IActionResult> DownloadPdf(Guid id)
{
    var userId = User.GetUserId();

    var calendar = await _context.UserCalendars
        .Include(c => c.User)
        .FirstOrDefaultAsync(c => c.Id == id);

    if (calendar == null)
        return NotFound();

    // Check ownership
    if (calendar.UserId != userId)
        return Forbid();

    // Check if paid (if part of order)
    var order = await _context.Orders
        .FirstOrDefaultAsync(o =>
            o.Items.Any(i => i.CalendarId == id) &&
            o.Status == OrderStatus.Paid
        );

    if (order == null && calendar.PdfUrl == null)
        return BadRequest("Calendar not paid or PDF not generated");

    // Check if PDF exists
    if (string.IsNullOrEmpty(calendar.PdfUrl))
    {
        // Trigger generation if not exists
        await _pdfService.GeneratePdfAsync(id, PdfFormat.A4);
        return Accepted("PDF generation in progress");
    }

    // Download PDF
    var pdfBytes = await _fileStorage.DownloadAsync(calendar.PdfUrl);

    return File(pdfBytes, "application/pdf", $"calendar-{id}.pdf");
}
```

### Signed Download Links
```csharp
public class DownloadLinkService
{
    public string GenerateSignedLink(Guid calendarId, int expiryHours = 24)
    {
        var payload = new
        {
            calendarId,
            expiresAt = DateTime.UtcNow.AddHours(expiryHours)
        };

        var token = EncryptPayload(JsonSerializer.Serialize(payload));

        return $"/api/download/pdf?token={token}";
    }

    public async Task<IActionResult> DownloadWithToken(string token)
    {
        try
        {
            var payload = DecryptPayload(token);
            var data = JsonSerializer.Deserialize<DownloadPayload>(payload);

            if (data.ExpiresAt < DateTime.UtcNow)
                return new UnauthorizedResult();

            // Download PDF...
        }
        catch
        {
            return new UnauthorizedResult();
        }
    }
}
```

### Frontend: DownloadPdfButton.tsx
```typescript
'use client'

import { useState } from 'react'
import { Button } from '@/components/ui/button'
import { Download, Loader2 } from 'lucide-react'
import { calendarService } from '@/services/calendarService'
import { toast } from 'sonner'

interface DownloadPdfButtonProps {
  calendarId: string
  isPaid: boolean
}

export default function DownloadPdfButton({ calendarId, isPaid }: DownloadPdfButtonProps) {
  const [downloading, setDownloading] = useState(false)

  const handleDownload = async () => {
    try {
      setDownloading(true)

      const blob = await calendarService.downloadPdf(calendarId)

      // Create download link
      const url = window.URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = url
      link.download = `calendar-${calendarId}.pdf`
      link.click()

      window.URL.revokeObjectURL(url)

      toast.success('PDF завантажено!')
    } catch (error) {
      if (error.response?.status === 202) {
        toast.info('PDF генерується... Спробуйте через 1-2 хвилини')
      } else {
        toast.error('Помилка завантаження')
      }
    } finally {
      setDownloading(false)
    }
  }

  return (
    <Button
      onClick={handleDownload}
      disabled={!isPaid || downloading}
    >
      {downloading ? (
        <>
          <Loader2 className="w-4 h-4 mr-2 animate-spin" />
          Завантаження...
        </>
      ) : (
        <>
          <Download className="w-4 h-4 mr-2" />
          Завантажити PDF
        </>
      )}
    </Button>
  )
}
```

### calendarService.ts
```typescript
export const calendarService = {
  async downloadPdf(calendarId: string): Promise<Blob> {
    const response = await api.get(
      `/calendars/${calendarId}/download-pdf`,
      { responseType: 'blob' }
    )
    return response.data
  },
}
```

## Примітки

- Blob responseType для файлів
- window.URL.createObjectURL для download
- 202 Accepted якщо PDF ще генерується
- Rate limiting важливий (запобігти зловживанню)

## Чому Codex?

Стандартна download функціональність:
- File stream endpoint
- Frontend download button
- Simple access control
- Типовий file download pattern

---

**Створено**: 2025-11-16
**Оновлено**: 2025-11-16
**Виконано**: -
