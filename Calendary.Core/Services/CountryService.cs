using Calendary.Model;
using Calendary.Repos.Repositories;

namespace Calendary.Core.Services;

public interface ICountryService
{
    Task<IEnumerable<Country>> GetAllCountriesAsync();
}

public class CountryService(ICountryRepository countryRepository) : ICountryService
{
    public Task<IEnumerable<Country>> GetAllCountriesAsync() => countryRepository.GetAllAsync();
    
}
