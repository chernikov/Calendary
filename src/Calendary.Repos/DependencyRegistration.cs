using Calendary.Model;
using Calendary.Repos.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Calendary.Repos
{
    public static class DependencyRegistration
    {
        public static IServiceCollection AddCalendaryRepositories(this IServiceCollection services, string connectionString)
        {
            // Реєстрація DbContext
            services.AddDbContext<ICalendaryDbContext, CalendaryDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Реєстрація репозиторіїв
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IUserSettingRepository, UserSettingRepository>();
            services.AddScoped<ICountryRepository, CountryRepository>();
            services.AddScoped<ILanguageRepository, LanguageRepository>();
            services.AddScoped<IEventDateRepository, EventDateRepository>();
            services.AddScoped<ICalendarRepository, CalendarRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOrderItemRepository, OrderItemRepository>();
            services.AddScoped<IImageRepository, ImageRepository>();
            services.AddScoped<IHolidayRepository, HolidayRepository>();
            services.AddScoped<IVerificationEmailCodeRepository, VerificationEmailCodeRepository>();
            services.AddScoped<IVerificationPhoneCodeRepository, VerificationPhoneCodeRepository>();
            services.AddScoped<IRepository<PaymentInfo>, PaymentInfoRepository>();
            services.AddScoped<IMonoWebhookEventRepository, MonoWebhookEventRepository>();

            services.ApplyMigrationDb();

            return services;
        }
    }
}
