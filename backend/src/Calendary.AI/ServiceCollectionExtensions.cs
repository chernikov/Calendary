using Calendary.AI.Clients;
using Calendary.AI.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Calendary.AI;

public static class ServiceCollectionExtensions
{
    /// Registers AiOptions (bound to the "AI" appsettings section) and both concrete clients as
    /// keyed IAiImageClient services ("OpenAI" / "Gemini"), so the caller (Calendary.Infrastructure's
    /// DynamicImageGenerationService) can resolve whichever one is currently selected at runtime —
    /// this project has no knowledge of where that selection is stored (a DB setting, outside
    /// Calendary.AI's zero-dependency scope). Both AI:OpenAI:ApiKey and AI:Gemini:ApiKey must be
    /// configured for both real options to work, since either can be selected at any time.
    public static IServiceCollection AddCalendaryAi(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AiOptions>(configuration.GetSection(AiOptions.SectionName));

        services.AddHttpClient<OpenAiImageClient>();
        services.AddHttpClient<GeminiImageClient>();

        services.AddKeyedScoped<IAiImageClient>("OpenAI", (sp, _) => sp.GetRequiredService<OpenAiImageClient>());
        services.AddKeyedScoped<IAiImageClient>("Gemini", (sp, _) => sp.GetRequiredService<GeminiImageClient>());

        return services;
    }
}
