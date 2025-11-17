namespace Calendary.Model;

public class ResetToken
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;

    public int UserId { get; set; }
    public User User { get; set; } = null!;
}
