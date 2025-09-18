namespace EmailServiceAPI.Models;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAttempt { get; set; }
    public int LoginAttempts { get; set; } = 0;
}