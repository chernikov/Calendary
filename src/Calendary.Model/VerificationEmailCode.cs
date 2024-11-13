namespace Calendary.Model;

public class VerificationEmailCode
{
    public int Id { get; set; }
    public int UserId { get; set; } // Foreign key до користувача
    public User User { get; set; } = null!;   
    public string Code { get; set; } = null!;

    public DateTime ExpiryDate { get; set; }
    public bool IsUsed { get; set; } = false;
}
