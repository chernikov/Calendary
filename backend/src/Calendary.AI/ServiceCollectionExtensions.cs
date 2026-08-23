using Calendary.AI.Clients;
using Calendary.AI.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Calendary.AI;

public static class ServiceCollectionExtensions
{
    /// Registers AiOptions (bound to the "AI" appsettings section) and a single IAiImageClient —
    /// whichever provider AiOptions.Provider selects. Swapping providers is a config change only;
    /// no code/DI changes needed.
    public static IServiceCollection AddCalendaryAi(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AiOptions>(configuration.GetSection(AiOptions.SectionName));

        var provider = configuration.GetSection(AiOptions.SectionName)
            .GetValue<AiProvider?>(nameof(AiOptions.Provider)) ?? AiProvider.OpenAI;

        switch (provider)
        {
            case AiProvider.Gemini:
                services.AddHttpClient<IAiImageClient, GeminiImageClient>();
                break;
            case AiProvider.OpenAI:
            default:
                services.AddHttpClient<IAiImageClient, OpenAiImageClient>();
                break;
        }

        return services;
    }
}
