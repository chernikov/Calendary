using Calendary.Model;
using Calendary.Repos.Repositories;

namespace Calendary.Core.Services;

public interface IWebHookService
{
    Task AddAsync(WebHook webHook);
}

public class WebHookService(IWebHookRepository webHookRepository) : IWebHookService
{
    public Task AddAsync(WebHook webHook)
        => webHookRepository.AddAsync(webHook);
}

