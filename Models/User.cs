using System;

namespace IndustrialPanel.Models;

public class User
{
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = "Operator";
    public DateTime LastLogin { get; set; }
}
