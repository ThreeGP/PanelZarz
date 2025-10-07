using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using IndustrialPanel.ViewModels;

namespace IndustrialPanel.Views;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
        AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
    }
    
    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            if (DataContext is LoginViewModel viewModel)
            {
                viewModel.LoginCommand.Execute(null);
            }
        }
    }
}
