# Task 30: Тестування генерації PDF

**Epic**: [Epic 01 - Перший реліз](../epic_01.md)
**Статус**: TODO
**Пріоритет**: P1 (Високий)
**Складність**: Середня
**Час**: 3-4 години
**Відповідальний AI**: Claude

## Опис задачі

Ретельно протестувати генерацію PDF календарів.

## Що треба зробити

1. **Unit Tests для PdfGenerator**:
   ```csharp
   [Fact]
   public async Task GeneratePdf_ShouldCreate12Pages()
   {
       var generator = new PdfCalendarGenerator();
       var pdf = await generator.GenerateAsync(testData);
       Assert.Equal(12, pdf.PageCount);
   }
   ```

2. **Integration Tests**:
   - End-to-end від calendar data до PDF file
   - Різні сценарії (різні presets, мови, форми)
   - Edge cases (missing images, invalid dates)

3. **Manual Testing Checklist**:
   - [ ] PDF відкривається в Adobe Reader
   - [ ] Всі зображення відображаються
   - [ ] Текст читабельний
   - [ ] Holidays виділені
   - [ ] Watermark видимий
   - [ ] Metadata коректна
   - [ ] Розмір файлу <15MB
   - [ ] Якість зображень 300 DPI
   - [ ] Друк працює (test print)

4. **Performance Tests**:
   - Генерація <10 секунд
   - Memory usage <500MB
   - Concurrent generations (3-5 одночасно)

## Файли для створення

- `tests/Calendary.Core.Tests/Services/PdfCalendarGeneratorTests.cs`
- `tests/integration/pdf-generation.test.ts`

## Що тестувати

- [ ] All unit tests pass
- [ ] Integration tests pass
- [ ] Manual checklist completed
- [ ] Performance benchmarks met

---

**Створено**: 2025-11-15
