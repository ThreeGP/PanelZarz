using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IndustrialPanel.Services;

namespace IndustrialPanel.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly AuthenticationService _authService;
    
    [ObservableProperty]
    private string _username = string.Empty;
    
    [ObservableProperty]
    private string _password = string.Empty;
    
    [ObservableProperty]
    private string _errorMessage = string.Empty;
    
    [ObservableProperty]
    private bool _isLoading = false;
    
    public event EventHandler? LoginSuccessful;
    
    // Konstruktor bez parametrów dla design-time
    public LoginViewModel()
    {
        _authService = new AuthenticationService();
        System.Diagnostics.Debug.WriteLine("=== LoginViewModel Design Constructor ===");
    }
    
    public LoginViewModel(AuthenticationService authService)
    {
        _authService = authService;
        System.Diagnostics.Debug.WriteLine("=== LoginViewModel Runtime Constructor ===");
    }
    
    [RelayCommand]
    private void Login()
    {
        ErrorMessage = string.Empty;
        
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Proszę podać nazwę użytkownika i hasło";
            return;
        }
        
        IsLoading = true;
        
        bool success = _authService.Login(Username, Password);
        
        IsLoading = false;
        
        if (success)
        {
            LoginSuccessful?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            ErrorMessage = "Nieprawidłowa nazwa użytkownika lub hasło";
            Password = string.Empty;
        }
    }
}
