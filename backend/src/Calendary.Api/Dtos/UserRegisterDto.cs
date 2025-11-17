namespace Calendary.Api.Dtos
{
    public class UserRegisterDto
    {
        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty; 

        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
