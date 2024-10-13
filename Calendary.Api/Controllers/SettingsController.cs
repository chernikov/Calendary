using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Core.Services;
using Calendary.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers;

[Authorize(Roles="User")]
[ApiController]
[Route("api/[controller]")]
public class SettingsController(IUserService userService,
                                IUserSettingService userSettingService,
                                IMapper mapper) 
    : BaseUserController(userService)
{
    [HttpGet]
    public async Task<IActionResult> GetUserSettings()
    {
        var user = await CurrentUser.Value;

        if (user == null)
        {
            return Unauthorized();
        }
        var settings = await userSettingService.GetSettingsByUserIdAsync(user.Id);
        if (settings is null)
        {
            return NotFound("Settings not found");
        }
        var result = mapper.Map<SettingDto>(settings);
        return Ok(result);
    }



    [HttpPut]
    public async Task<IActionResult> UpdateUserSettings([FromBody] SettingDto updatedSettings)
    {
        var user = await CurrentUser.Value;

        if (user == null)
        {
            return Unauthorized();
        }

        var existingSettings = await userSettingService.GetSettingsByUserIdAsync(user.Id);
        if (existingSettings == null)
        {
            return NotFound("Settings not found");
        }

        var newSettings = mapper.Map<UserSetting>(updatedSettings);


        var result = await userSettingService.UpdateSettingAsync(existingSettings.Id, newSettings);

        if (!result)
        {
            return StatusCode(500, "The error occurred while saving the settings.");
        }

        return NoContent(); 
    }
}
