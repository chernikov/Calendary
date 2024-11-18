using Calendary.Core.Senders;
using Calendary.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Calendary.Core;

public static class DependencyRegistration
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddMemoryCache();

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserSettingService, UserSettingService>();
        services.AddScoped<ICountryService, CountryService>();
        services.AddScoped<ILanguageService, LanguageService>();
        services.AddScoped<IEventDateService, EventDateService>();
        services.AddScoped<ICalendarService, CalendarService>();
        services.AddScoped<IImageService, ImageService>();
        services.AddScoped<IHolidayService, HolidayService>();
        services.AddScoped<IPdfGeneratorService, PdfGeneratorService>();
        services.AddScoped<IImageRotatorService, ImageRotatorService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPriceService, PriceService>();
        services.AddScoped<IWebHookService, WebHookService>();

        services.AddScoped<IEmailSender, SendGridSender>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ISmsService, SmsService>();
        
        services.AddHttpClient<INovaPostService, NovaPostService>();

        services.AddHttpClient<ISmsSender, SmsClubSender>();

        services.AddHttpClient<IPaymentService, MonoPaymentService>();



        return services;
    }
}
