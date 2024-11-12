namespace Calendary.Api.Dtos;

public class UserInfoDto
{
    public string UserName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public bool IsEmailConfirmed { get; set; }

    public bool IsPhoneNumberConfirmed { get; set; }


    public string? Token { get; set; }
}
