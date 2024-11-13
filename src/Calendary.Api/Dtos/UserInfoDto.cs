namespace Calendary.Api.Dtos;

public class UserInfoDto
{
    public string? UserName { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public bool IsEmailConfirmed { get; set; }

    public bool IsPhoneNumberConfirmed { get; set; }

}

