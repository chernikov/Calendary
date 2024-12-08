using Microsoft.Extensions.Configuration;

namespace Calendary.Core.Services;

public interface IPriceService
{
    public decimal GetPrice();
}

public class PriceService : IPriceService
{
    private decimal _calendarPrice;
    public PriceService(IConfiguration configuration)
    {
        _calendarPrice = decimal.Parse(configuration["Price:Calendar"]!);
    }
    public decimal GetPrice()
    {
        return _calendarPrice;
    }
}
