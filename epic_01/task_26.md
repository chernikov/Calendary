# Task 26: Оптимізація розміру зображень для PDF

**Epic**: [Epic 01 - Перший реліз](../epic_01.md)
**Статус**: TODO
**Пріоритет**: P1 (Високий)
**Складність**: Середня
**Час**: 3-4 години
**Відповідальний AI**: Gemini

## Опис задачі

Оптимізувати розмір зображень перед додаванням в PDF щоб зменшити final size.

## Що треба зробити

1. **ImageOptimizer**:
   ```csharp
   public class ImageOptimizer
   {
       public async Task<byte[]> OptimizeForPdfAsync(
           string imageUrl,
           int maxWidth = 2480, // A4 @ 300 DPI
           int quality = 85
       )
       {
           var image = await DownloadImage(imageUrl);
           var resized = ResizeImage(image, maxWidth);
           var compressed = CompressJpeg(resized, quality);
           return compressed;
       }
   }
   ```

2. **Formats**:
   - JPEG для фотографій (quality 85%)
   - PNG only якщо transparency потрібна
   - WebP для ще меншого розміру (якщо підтримується)

3. **Caching**:
   - Зберігати optimized версії
   - Не re-optimize при кожній генерації PDF

## Файли для створення

- `src/Calendary.Core/Services/ImageOptimizer.cs`

## Що тестувати

- [ ] Зображення стискаються
- [ ] Якість залишається прийнятною
- [ ] Розмір зменшується на 50-70%
- [ ] Cache працює

---

**Створено**: 2025-11-15
