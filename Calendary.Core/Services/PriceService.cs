namespace Calendary.Core.Services;

public interface IPriceService
{
    public decimal GetPrice();
}

public class PriceService : IPriceService
{
    public decimal GetPrice()
    {
        return 500m;
    }
}
