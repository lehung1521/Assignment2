namespace B2BWholesale.Data.Models;

public class User
{
    public int UserID { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;   // SHA-256 hash, never plain text
    public bool Lock { get; set; }
    public string Role { get; set; } = "Agent";             // "Admin" or "Agent"
    public int? AgentID { get; set; }                       // null for Admin users
    public int FailedLoginCount { get; set; }

    public Agent? Agent { get; set; }
}