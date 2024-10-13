using Calendary.Model;
using Calendary.Repos.Repositories;

namespace Calendary.Core.Services;

public interface ILanguageService
{
    Task<IEnumerable<Language>> GetAllLanguagesAsync();
}

public class LanguageService(ILanguageRepository languageRepository) : ILanguageService
{
    public Task<IEnumerable<Language>> GetAllLanguagesAsync() => languageRepository.GetAllAsync();
}
