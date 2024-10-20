﻿using Calendary.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calendary.Core;

public static class DependencyRegistration
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
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

        return services;
    }
}
