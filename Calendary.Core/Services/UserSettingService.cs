using Calendary.Model;
using Calendary.Repos.Repositories;

namespace Calendary.Core.Services;

public interface IUserSettingService
{
    Task<UserSetting?> GetSettingsByUserIdAsync(int userId);
    Task<bool> UpdateSettingAsync(int id, UserSetting updatedSettings);
}

public class UserSettingService(IUserSettingRepository userSettingRepository) : IUserSettingService
{
    public Task<UserSetting?> GetSettingsByUserIdAsync(int userId) => userSettingRepository.GetFullByUserIdAsync(userId);

    public async Task<bool> UpdateSettingAsync(int id, UserSetting updatedSettings)
    {
        var existingSetting = await userSettingRepository.GetByIdAsync(id);
        if (existingSetting == null)
        {
            return false;
        }

        existingSetting.FirstDayOfWeek = updatedSettings.FirstDayOfWeek;
        existingSetting.LanguageId = updatedSettings.Language.Id;
        existingSetting.CountryId = updatedSettings.Country.Id;

        await userSettingRepository.UpdateAsync(existingSetting);

        return true;
    }
}
