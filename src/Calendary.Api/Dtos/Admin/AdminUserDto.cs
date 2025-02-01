namespace Calendary.Api.Dtos.Admin;

public abstract class AdminBaseUserDto
{
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
}

public class AdminUserDto : AdminBaseUserDto
{
    public int Id { get; set; }
    public Guid Identity { get; set; }

    public DateTime Created { get; set; }

    public bool CreatedByAdmin { get; set; }

    public bool IsEmailConfirmed { get; set; } 

    public bool IsPhoneNumberConfirmed { get; set; } 
}

public class AdminCreateUserDto : AdminBaseUserDto
{
}