﻿using Calendary.Model;
using Calendary.Repos.Repositories;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace Calendary.Core.Services;

public interface IUserSettingService
{
    Task<UserSetting?> GetSettingsByUserIdAsync(int userId);
    
    Task<bool> UpdateSettingAsync(UserSetting updatedSettings);

    Task UpdateDeliveryAsync(UserSetting updatedSettings);
}

public class UserSettingService(IUserSettingRepository userSettingRepository) : IUserSettingService
{
    public Task<UserSetting?> GetSettingsByUserIdAsync(int userId) => userSettingRepository.GetFullByUserIdAsync(userId);

    public async Task<bool> UpdateSettingAsync(UserSetting updatedSettings)
    {
        var existingSetting = await userSettingRepository.GetByIdAsync(updatedSettings.Id);
        if (existingSetting is null)
        {
            return false;
        }

        existingSetting.FirstDayOfWeek = updatedSettings.FirstDayOfWeek;
        existingSetting.LanguageId = updatedSettings.Language.Id;
        existingSetting.CountryId = updatedSettings.Country.Id;

        await userSettingRepository.UpdateAsync(existingSetting);

        return true;
    }

    public async Task UpdateDeliveryAsync(UserSetting updatedSettings)
    {
        var existingSetting = await userSettingRepository.GetByIdAsync(updatedSettings.Id);
        if (existingSetting is null)
        {
            return;
        }

        existingSetting.DeliveryRaw = updatedSettings.DeliveryRaw;
        existingSetting.DeliveryAddress = updatedSettings.DeliveryAddress;

        await userSettingRepository.UpdateAsync(existingSetting);
    }
}
