using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using IndustrialPanel.ViewModels;
using IndustrialPanel.Views;
using IndustrialPanel.Services;

namespace IndustrialPanel;

public partial class App : Application
{
    private AuthenticationService? _authService;
    private IndustrialDataService? _dataService;
    private ExcelExportService? _excelService;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DisableAvaloniaDataAnnotationValidation();
            
            _authService = new AuthenticationService();
            _dataService = new IndustrialDataService();
            _excelService = new ExcelExportService();
            
            ShowLoginWindow(desktop);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ShowLoginWindow(IClassicDesktopStyleApplicationLifetime desktop)
    {
        var loginViewModel = new LoginViewModel(_authService!);
        var loginWindow = new LoginWindow
        {
            DataContext = loginViewModel
        };

        void OnLoginSuccess(object? s, System.EventArgs e)
        {
            loginViewModel.LoginSuccessful -= OnLoginSuccess;
            ShowMainWindow(desktop);
            loginWindow.Close();
        }
        loginViewModel.LoginSuccessful += OnLoginSuccess;

        desktop.MainWindow = loginWindow;
    }

    private void ShowMainWindow(IClassicDesktopStyleApplicationLifetime desktop)
    {
        if (desktop.MainWindow is MainWindow existing)
        {
            try { existing.Close(); } catch { /* ignore */ }
        }
        var mainViewModel = new MainWindowViewModel(_dataService!, _excelService!, _authService!);
        var mainWindow = new MainWindow
        {
            DataContext = mainViewModel
        };

        mainViewModel.LogoutRequested += (s, e) =>
        {
            ShowLoginWindow(desktop);
            mainWindow.Close();
        };

        desktop.MainWindow = mainWindow;
        if (!mainWindow.IsVisible)
            mainWindow.Show();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}