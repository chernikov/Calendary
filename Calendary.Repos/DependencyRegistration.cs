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
            services.AddScoped<IRepository<Order>, OrderRepository>();
            services.AddScoped<IRepository<Calendar>, CalendarRepository>();
            services.AddScoped<IRepository<Image>, ImageRepository>();
            services.AddScoped<IRepository<EventDate>, EventDateRepository>();
            services.AddScoped<IRepository<Holiday>, HolidayRepository>();
            services.AddScoped<IRepository<Country>, CountryRepository>();
            services.AddScoped<IRepository<UserSetting>, UserSettingRepository>();
            services.AddScoped<IRepository<PaymentInfo>, PaymentInfoRepository>();
            services.AddScoped<IRepository<Language>, LanguageRepository>();

            services.ApplyMigrationDb();

            return services;
        }
    }
}
