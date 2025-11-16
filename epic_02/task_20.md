# Task 20: Інтеграція Nova Poshta (доставка)

**Epic**: [Epic 02 - Customer Portal](../epic_02.md)
**Статус**: TODO
**Пріоритет**: P0 (Критичний)
**Складність**: Середня-Висока
**Час**: 6-8 годин
**Відповідальний AI**: Claude
**Залежить від**: Task 18
**Паралельно з**: Task 19 (можна робити одночасно)

## Опис задачі

Інтегрувати Nova Poshta API для пошуку міст, відділень, розрахунку вартості доставки, створення ТТН (накладної).

## Проблема

Користувачі повинні мати можливість вибрати відділення Нової Пошти або замовити кур'єрську доставку з автоматичним розрахунком вартості.

## Що треба зробити

1. **Backend: Nova Poshta Service**
   - `src/Calendary.Application/Services/NovaPoshtaService.cs`
   - Інтеграція з Nova Poshta API 2.0
   - API Key в appsettings
   - HTTP client для запитів

2. **Імплементувати методи API:**

   **a) Пошук міст (Cities)**
   - `SearchCities(string query)` - пошук міста по назві
   - Return: список міст з Ref та Description

   **b) Пошук відділень (Warehouses)**
   - `GetWarehouses(string cityRef)` - отримати відділення міста
   - Return: список відділень з номером та адресою

   **c) Розрахунок вартості доставки**
   - `CalculateDeliveryCost(DeliveryRequest request)` - розрахувати ціну
   - Параметри: вага, об'єм, місто відправлення/призначення
   - Return: вартість доставки

   **d) Створення ТТН (Internet Document)**
   - `CreateInternetDocument(OrderShipment shipment)` - створити накладну
   - Після оплати замовлення
   - Return: номер ТТН для відстеження

3. **Backend: API Endpoints**
   - `GET /api/delivery/cities?search={query}` - пошук міст
   - `GET /api/delivery/warehouses?cityRef={ref}` - відділення міста
   - `POST /api/delivery/calculate` - розрахунок вартості
   - `POST /api/delivery/create-ttn` - створити ТТН (admin/system only)

4. **Frontend: Nova Poshta Components**
   - `NovaPoshtaCitySearch.tsx` - autocomplete для міст
   - `NovaPoshtaWarehouseSelect.tsx` - dropdown відділень
   - Інтеграція в DeliveryForm

5. **Автоматичний розрахунок доставки в checkout**
   - При виборі міста/відділення автоматично розрахувати вартість
   - Показувати в OrderSummary
   - Update total price

6. **Створення ТТН після оплати**
   - Автоматично створювати ТТН після успішної оплати
   - Зберігати tracking number в Order
   - Відправити email з ТТН користувачу

## Файли для створення/модифікації

- `src/Calendary.Core/Interfaces/INovaPoshtaService.cs`
- `src/Calendary.Application/Services/NovaPoshtaService.cs`
- `src/Calendary.API/Controllers/DeliveryController.cs`
- `src/Calendary.API/DTOs/Nova Poshta/` - DTOs для requests/responses
- `src/components/features/checkout/NovaPoshtaCitySearch.tsx`
- `src/components/features/checkout/NovaPoshtaWarehouseSelect.tsx`
- `src/services/deliveryService.ts` (frontend)
- `appsettings.json` - Nova Poshta API key

## Критерії успіху

- [ ] Можна знайти місто через autocomplete
- [ ] Можна вибрати відділення з dropdown
- [ ] Вартість доставки розраховується автоматично
- [ ] Ціна доставки додається до total в checkout
- [ ] ТТН створюється автоматично після оплати
- [ ] Tracking number зберігається в Order
- [ ] Email з ТТН відправляється користувачу

## Залежності

- Task 18: Checkout форма повинна бути готова

## Блокується наступні задачі

- Task 23: Історія замовлень потребує tracking number

## Технічні деталі

### Nova Poshta API Configuration
```csharp
// appsettings.json
{
  "NovaPoshta": {
    "ApiKey": "YOUR_API_KEY",
    "ApiUrl": "https://api.novaposhta.ua/v2.0/json/",
    "SenderCity": "8d5a980d-391c-11dd-90d9-001a92567626", // Київ
    "SenderWarehouse": "1ec09d88-e1c2-11e3-8c4a-0050568002cf",
    "SenderContact": "...", // контакт відправника
    "SenderPhone": "+380..."
  }
}
```

### NovaPoshtaService.cs
```csharp
public interface INovaPoshtaService
{
    Task<List<CityDto>> SearchCitiesAsync(string query);
    Task<List<WarehouseDto>> GetWarehousesAsync(string cityRef);
    Task<decimal> CalculateDeliveryCostAsync(DeliveryCalculationRequest request);
    Task<string> CreateInternetDocumentAsync(CreateTTNRequest request);
}

public class NovaPoshtaService : INovaPoshtaService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly string _apiKey;

    public NovaPoshtaService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
        _apiKey = config["NovaPoshta:ApiKey"];
    }

    public async Task<List<CityDto>> SearchCitiesAsync(string query)
    {
        var request = new
        {
            apiKey = _apiKey,
            modelName = "Address",
            calledMethod = "searchSettlements",
            methodProperties = new
            {
                CityName = query,
                Limit = 10
            }
        };

        var response = await _httpClient.PostAsJsonAsync(
            _config["NovaPoshta:ApiUrl"],
            request
        );

        var result = await response.Content.ReadFromJsonAsync<NovaPoshtaResponse<List<CityDto>>>();

        if (!result.Success)
            throw new Exception($"Nova Poshta API error: {result.Errors}");

        return result.Data[0].Addresses;
    }

    public async Task<List<WarehouseDto>> GetWarehousesAsync(string cityRef)
    {
        var request = new
        {
            apiKey = _apiKey,
            modelName = "AddressGeneral",
            calledMethod = "getWarehouses",
            methodProperties = new
            {
                CityRef = cityRef,
                Limit = 100
            }
        };

        var response = await _httpClient.PostAsJsonAsync(
            _config["NovaPoshta:ApiUrl"],
            request
        );

        var result = await response.Content.ReadFromJsonAsync<NovaPoshtaResponse<List<WarehouseDto>>>();

        if (!result.Success)
            throw new Exception($"Nova Poshta API error: {result.Errors}");

        return result.Data;
    }

    public async Task<decimal> CalculateDeliveryCostAsync(DeliveryCalculationRequest req)
    {
        var request = new
        {
            apiKey = _apiKey,
            modelName = "InternetDocument",
            calledMethod = "getDocumentPrice",
            methodProperties = new
            {
                CitySender = _config["NovaPoshta:SenderCity"],
                CityRecipient = req.RecipientCityRef,
                Weight = req.Weight,
                ServiceType = "WarehouseWarehouse",
                Cost = req.DeclaredValue,
                CargoType = "Cargo",
                SeatsAmount = 1
            }
        };

        var response = await _httpClient.PostAsJsonAsync(
            _config["NovaPoshta:ApiUrl"],
            request
        );

        var result = await response.Content.ReadFromJsonAsync<NovaPoshtaResponse<List<DeliveryCostDto>>>();

        if (!result.Success)
            throw new Exception($"Nova Poshta API error: {result.Errors}");

        return result.Data[0].Cost;
    }

    public async Task<string> CreateInternetDocumentAsync(CreateTTNRequest req)
    {
        var request = new
        {
            apiKey = _apiKey,
            modelName = "InternetDocument",
            calledMethod = "save",
            methodProperties = new
            {
                PayerType = "Recipient",
                PaymentMethod = "Cash",
                DateTime = DateTime.Now.ToString("dd.MM.yyyy"),
                CargoType = "Cargo",
                VolumeGeneral = "0.01",
                Weight = req.Weight,
                ServiceType = "WarehouseWarehouse",
                SeatsAmount = 1,
                Description = req.Description,
                Cost = req.DeclaredValue,
                CitySender = _config["NovaPoshta:SenderCity"],
                Sender = _config["NovaPoshta:SenderContact"],
                SenderAddress = _config["NovaPoshta:SenderWarehouse"],
                ContactSender = _config["NovaPoshta:SenderContact"],
                SendersPhone = _config["NovaPoshta:SenderPhone"],
                CityRecipient = req.RecipientCityRef,
                Recipient = req.RecipientName,
                RecipientAddress = req.RecipientWarehouseRef,
                ContactRecipient = req.RecipientName,
                RecipientsPhone = req.RecipientPhone
            }
        };

        var response = await _httpClient.PostAsJsonAsync(
            _config["NovaPoshta:ApiUrl"],
            request
        );

        var result = await response.Content.ReadFromJsonAsync<NovaPoshtaResponse<List<TTNDto>>>();

        if (!result.Success)
            throw new Exception($"Nova Poshta API error: {string.Join(", ", result.Errors)}");

        return result.Data[0].IntDocNumber; // Tracking number
    }
}
```

### Frontend: NovaPoshtaCitySearch.tsx
```typescript
'use client'

import { useState, useCallback } from 'react'
import { Command, CommandInput, CommandList, CommandItem } from '@/components/ui/command'
import { deliveryService } from '@/services/deliveryService'
import { debounce } from 'lodash'

interface City {
  ref: string
  description: string
}

interface NovaPoshtaCitySearchProps {
  onSelect: (cityRef: string) => void
}

export default function NovaPoshtaCitySearch({ onSelect }: NovaPoshtaCitySearchProps) {
  const [cities, setCities] = useState<City[]>([])
  const [loading, setLoading] = useState(false)

  const searchCities = useCallback(
    debounce(async (query: string) => {
      if (query.length < 2) {
        setCities([])
        return
      }

      try {
        setLoading(true)
        const results = await deliveryService.searchCities(query)
        setCities(results)
      } catch (error) {
        console.error('Failed to search cities:', error)
      } finally {
        setLoading(false)
      }
    }, 500),
    []
  )

  return (
    <Command>
      <CommandInput
        placeholder="Почніть вводити назву міста..."
        onValueChange={searchCities}
      />
      <CommandList>
        {loading && <div className="p-2 text-sm text-gray-500">Завантаження...</div>}
        {cities.map((city) => (
          <CommandItem
            key={city.ref}
            onSelect={() => onSelect(city.ref)}
          >
            {city.description}
          </CommandItem>
        ))}
      </CommandList>
    </Command>
  )
}
```

## Примітки

- Nova Poshta API 2.0 використовує POST requests для всіх методів
- API Key можна отримати в особистому кабінеті Nova Poshta
- ТТН створюється автоматично після оплати
- Tracking number дозволяє відстежувати посилку

## Чому Claude?

Складна інтеграція з третьою стороною:
- External API integration
- Request/response mapping
- Error handling для API
- Business logic для ТТН
- Потрібне розуміння delivery workflow

---

**Створено**: 2025-11-16
**Оновлено**: 2025-11-16
**Виконано**: -
