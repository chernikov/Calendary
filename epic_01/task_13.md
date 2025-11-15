# Task 13: AI-покращення промптів

**Epic**: [Epic 01 - Перший реліз](../epic_01.md)
**Статус**: TODO
**Пріоритет**: P1 (Високий)
**Складність**: Середня
**Час**: 4-5 годин
**Відповідальний AI**: Gemini

## Опис задачі

Реалізувати сервіс для покращення промптів через Claude API або OpenAI GPT.

## Що треба зробити

1. **PromptEnhancerService**:
   ```csharp
   public async Task<string> EnhancePromptAsync(string userPrompt)
   {
       var systemPrompt = "You are a Flux AI prompt expert. Enhance this prompt...";
       var response = await _anthropicClient.CreateMessage(systemPrompt, userPrompt);
       return response.Content;
   }
   ```

2. **API Endpoint**:
   - `POST /api/prompts/enhance`
   - Body: `{ prompt: string }`
   - Response: `{ enhancedPrompt: string, suggestions: [] }`

3. **Integration**:
   - Anthropic Claude API (recommended) або OpenAI
   - Rate limiting (max 10 requests/min per user)
   - Caching часто використовуваних prompts

## Файли для створення

- `src/Calendary.Core/Services/PromptEnhancerService.cs`
- `src/Calendary.Api/Controllers/PromptController.cs`

## Що тестувати

- [ ] Enhanced prompt генерується
- [ ] API response <3 секунди
- [ ] Rate limiting працює
- [ ] Помилки обробляються
- [ ] Кешування працює

---

**Створено**: 2025-11-15
