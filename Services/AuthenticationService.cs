using System.Collections.Generic;
using System.Linq;
using IndustrialPanel.Models;

namespace IndustrialPanel.Services;

public class AuthenticationService
{
    private readonly PasswordHashService _passwordHashService;
    private readonly List<User> _users;
    private User? _currentUser;
    
    public User? CurrentUser => _currentUser;
    
    public AuthenticationService()
    {
        _passwordHashService = new PasswordHashService();
        _users = new List<User>();
        
        InitializeDefaultUser();
    }
    
    private void InitializeDefaultUser()
    {
        string salt = _passwordHashService.GenerateSalt();
        string passwordHash = _passwordHashService.HashPassword("admin123", salt);
        
        var defaultUser = new User
        {
            Username = "admin",
            PasswordHash = passwordHash,
            Salt = salt,
            FullName = "Administrator Systemu",
            Role = "Administrator"
        };
        
        _users.Add(defaultUser);
    }
    
    public bool Login(string username, string password)
    {
        var user = _users.FirstOrDefault(u => u.Username == username);
        
        if (user == null)
            return false;
        
        bool isPasswordValid = _passwordHashService.VerifyPassword(
            password, 
            user.Salt, 
            user.PasswordHash
        );
        
        if (isPasswordValid)
        {
            _currentUser = user;
            user.LastLogin = System.DateTime.Now;
            return true;
        }
        
        return false;
    }
    
    public void Logout()
    {
        _currentUser = null;
    }
    
    public bool IsAuthenticated()
    {
        return _currentUser != null;
    }
}
