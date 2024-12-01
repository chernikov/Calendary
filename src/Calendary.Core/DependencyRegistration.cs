using Calendary.Core.Senders;
using Calendary.Core.Services;
using Calendary.Core.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Calendary.Core;

public static class DependencyRegistration
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();

        services.Configure<ReplicateSettings>(configuration.GetSection("ReplicateSettings"));

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserSettingService, UserSettingService>();
        services.AddScoped<ICountryService, CountryService>();
        services.AddScoped<ILanguageService, LanguageService>();
        services.AddScoped<IEventDateService, EventDateService>();
        services.AddScoped<ICalendarService, CalendarService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IImageService, ImageService>();
        services.AddScoped<IHolidayService, HolidayService>();
        services.AddScoped<IPdfGeneratorService, PdfGeneratorService>();
        services.AddScoped<IImageRotatorService, ImageRotatorService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPriceService, PriceService>();
        services.AddScoped<IWebHookService, WebHookService>();
        services.AddScoped<IPromptThemeService, PromptThemeService>();
        services.AddScoped<IPromptService, PromptService>();
        services.AddScoped<IPhotoService, PhotoService>();
        services.AddScoped<IFluxModelService, FluxModelService>();
        services.AddScoped<ITrainingService, TrainingService>();
        services.AddScoped<IJobService, JobService>();
        services.AddScoped<IJobTaskService, JobTaskService>();
        services.AddScoped<ITestPromptService, TestPromptService>();


        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ISmsService, SmsService>();

        services.AddHttpClient<INovaPostService, NovaPostService>();

        services.AddScoped<IEmailSender, SendGridSender>();
        services.AddScoped<IRabbitMqSender, RabbitMqSender>();
        services.AddHttpClient<ISmsSender, SmsClubSender>();

        services.AddHttpClient<IPaymentService, MonoPaymentService>();
        services.AddHttpClient<IReplicateService, ReplicateService>();

        return services;
    }
}
