using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IndustrialPanel.Models;
using IndustrialPanel.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace IndustrialPanel.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IndustrialDataService _dataService;
    private readonly ExcelExportService _excelService;
    private readonly AuthenticationService _authService;
    
    [ObservableProperty]
    private ObservableCollection<ConveyorBeltData> _currentDevices = new();
    
    [ObservableProperty]
    private string _selectedDeviceId = "BELT-001";
    
    [ObservableProperty]
    private ConveyorBeltData? _selectedDeviceData;
    
    [ObservableProperty]
    private ObservableCollection<ConveyorBeltData> _historicalData = new();
    
    [ObservableProperty]
    private string _currentUserName = string.Empty;
    
    [ObservableProperty]
    private bool _isRealTimeEnabled = false;
    
    public ObservableCollection<ISeries> SpeedSeries { get; set; } = new();
    public ObservableCollection<ISeries> ThroughputSeries { get; set; } = new();
    public ObservableCollection<IndustrialPanel.Models.ReportHistoryItem> ReportHistory { get; } = new();

    // Status eksportu (banner w UI)
    [ObservableProperty]
    private string _exportStatusMessage = string.Empty;

    [ObservableProperty]
    private bool _isExportStatusVisible = false;

    [ObservableProperty]
    private bool _isExporting = false;
    
    // Bufory wartości do płynnych aktualizacji wykresów
    private readonly ObservableCollection<double> _speedValues = new();
    private readonly ObservableCollection<double> _throughputValues = new();
    private readonly ObservableCollection<DateTime> _labels = new();
    private System.Threading.CancellationTokenSource? _exportCts;

    // Skonfigurowane osie dla czytelności wykresów
    public Axis[] XAxes { get; private set; } = Array.Empty<Axis>();
    public Axis[] SpeedYAxes { get; private set; } = Array.Empty<Axis>();
    public Axis[] ThroughputYAxes { get; private set; } = Array.Empty<Axis>();
    
    public event EventHandler? LogoutRequested;
    
    public MainWindowViewModel()
    {
        _dataService = new IndustrialDataService();
        _excelService = new ExcelExportService();
        _authService = new AuthenticationService();
        
        CurrentUserName = "Administrator (Design)";
        
    var devices = _dataService.GetCurrentData();
    CurrentDevices = new ObservableCollection<ConveyorBeltData>(devices);

    ConfigureAxes();
    InitializeSeries();
    }
    
    public MainWindowViewModel(
        IndustrialDataService dataService, 
        ExcelExportService excelService,
        AuthenticationService authService)
    {
        _dataService = dataService;
        _excelService = excelService;
        _authService = authService;
        
    CurrentUserName = _authService.CurrentUser?.FullName ?? "Użytkownik";
        
    ConfigureAxes();
    InitializeSeries();
    LoadInitialData();
    StartRealTimeUpdates();
    }
    
    private void LoadInitialData()
    {
        var devices = _dataService.GetCurrentData();
        CurrentDevices = new ObservableCollection<ConveyorBeltData>(devices);
        
        LoadHistoricalData();
    }
    
    [RelayCommand]
    private void LoadHistoricalData()
    {
        var data = _dataService.GetRecentData(SelectedDeviceId, 12);
        HistoricalData = new ObservableCollection<ConveyorBeltData>(data);

        // wypełnij bufory wartości bez rekonstrukcji serii
        _speedValues.Clear();
        _throughputValues.Clear();
        _labels.Clear();
        foreach (var d in HistoricalData)
        {
            _speedValues.Add(d.SpeedMetersPerSecond);
            _throughputValues.Add(d.TonsPerHour);
            _labels.Add(d.Timestamp);
        }
        SelectedDeviceData = HistoricalData.LastOrDefault();
    }

    private void InitializeSeries()
    {
        SpeedSeries.Clear();
        SpeedSeries.Add(new LineSeries<double>
        {
            Values = _speedValues,
            Name = "Prędkość (m/s)",
                Fill = new SolidColorPaint(new SKColor(126, 87, 194, 56)),
                Stroke = new SolidColorPaint(new SKColor(126, 87, 194)) { StrokeThickness = 2.5f },
                GeometrySize = 0.5,
                LineSmoothness = 0.35
        });
        
        ThroughputSeries.Clear();
        ThroughputSeries.Add(new LineSeries<double>
        {
            Values = _throughputValues,
            Name = "Przepustowość (t/h)",
                Fill = new SolidColorPaint(new SKColor(171, 71, 188, 56)),
                Stroke = new SolidColorPaint(new SKColor(171, 71, 188)) { StrokeThickness = 2.5f },
                GeometrySize = 0.5,
                LineSmoothness = 0.35
        });
    }

    private void ConfigureAxes()
    {
        // Oś X: czas (etykiety HH:mm), z lekkim obrotem i siatką
        var xAxis = new Axis
        {
            LabelsRotation = 15,
            MinStep = 1,
            SeparatorsPaint = new SolidColorPaint(new SKColor(230, 230, 230)) { StrokeThickness = 1 },
            Labeler = value =>
            {
                var idx = (int)Math.Round(value);
                if (idx >= 0 && idx < _labels.Count)
                    return _labels[idx].ToString("HH:mm");
                return string.Empty;
            }
        };
        XAxes = new[] { xAxis };

        // Oś Y dla prędkości
        SpeedYAxes = new[]
        {
            new Axis
            {
                Name = "m/s",
                MinStep = 0.1,
                SeparatorsPaint = new SolidColorPaint(new SKColor(240, 240, 240)) { StrokeThickness = 1 }
            }
        };

        // Oś Y dla przepustowości
        ThroughputYAxes = new[]
        {
            new Axis
            {
                Name = "t/h",
                MinStep = 10,
                SeparatorsPaint = new SolidColorPaint(new SKColor(240, 240, 240)) { StrokeThickness = 1 }
            }
        };
    }
    
    private void StartRealTimeUpdates()
    {
        IsRealTimeEnabled = true;
        
        _dataService.StartRealTimeSimulation(data =>
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                var existingDevice = CurrentDevices.FirstOrDefault(d => d.DeviceId == data.DeviceId);
                if (existingDevice != null)
                {
                    int index = CurrentDevices.IndexOf(existingDevice);
                    CurrentDevices[index] = data;
                }
                
                if (data.DeviceId == SelectedDeviceId)
                {
                    HistoricalData.Add(data);
                    _speedValues.Add(data.SpeedMetersPerSecond);
                    _throughputValues.Add(data.TonsPerHour);
                    _labels.Add(data.Timestamp);

                    const int maxPoints = 240;
                    if (_speedValues.Count > maxPoints)
                    {
                        _speedValues.RemoveAt(0);
                        _throughputValues.RemoveAt(0);
                        _labels.RemoveAt(0);
                        HistoricalData.RemoveAt(0);
                    }
                }
                
                if (data.DeviceId == SelectedDeviceId)
                {
                    SelectedDeviceData = data;
                }
            });
        });
    }
    
    [RelayCommand]
    private void ToggleRealTime()
    {
        if (IsRealTimeEnabled)
        {
            _dataService.StopRealTimeSimulation();
            IsRealTimeEnabled = false;
        }
        else
        {
            StartRealTimeUpdates();
        }
    }
    
    [RelayCommand]
    private async Task ExportToExcel()
    {
        try
        {
            _exportCts?.Cancel();
            _exportCts = new System.Threading.CancellationTokenSource();
            string fileName = $"raport_{DateTime.Now:yyyyMMdd}.xlsx";
            string filePath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                fileName
            );
            
            var deviceName = CurrentDevices.FirstOrDefault(d => d.DeviceId == SelectedDeviceId)?.DeviceName ?? SelectedDeviceId;
            var exportData = HistoricalData.Count > 0
                ? HistoricalData.OrderBy(d => d.Timestamp).ToList()
                : _dataService.GetRecentData(SelectedDeviceId, 12);

            await Task.Run(() =>
            {
                _excelService.ExportToExcel(exportData, filePath, deviceName, _exportCts.Token);
            });
            System.Diagnostics.Debug.WriteLine($"Eksport zakończony: {filePath}");
            ReportHistory.Insert(0, new IndustrialPanel.Models.ReportHistoryItem
            {
                FileName = System.IO.Path.GetFileName(filePath),
                FullPath = filePath,
                CreatedAt = DateTime.Now,
                DeviceId = SelectedDeviceId
            });
            ShowExportStatus("Eksport zakończony");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Błąd eksportu: {ex.Message}");
            ShowExportStatus("Błąd eksportu");
        }
    }

    [RelayCommand]
    private async Task ExportReportToPath(string? filePath)
    {
        try
        {
            _exportCts?.Cancel();
            _exportCts = new System.Threading.CancellationTokenSource();
            var deviceName = CurrentDevices.FirstOrDefault(d => d.DeviceId == SelectedDeviceId)?.DeviceName ?? SelectedDeviceId;
            var exportData = HistoricalData.Count > 0
                ? HistoricalData.OrderBy(d => d.Timestamp).ToList()
                : _dataService.GetRecentData(SelectedDeviceId, 12);

            var pathCandidate = filePath;
            if (string.IsNullOrWhiteSpace(pathCandidate))
            {
                var fallbackName = $"raport_{DateTime.Now:yyyyMMdd}.xlsx";
                pathCandidate = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fallbackName);
            }

            await Task.Run(() =>
            {
                _excelService.ExportToExcel(exportData, pathCandidate!, deviceName, _exportCts.Token);
            });
            System.Diagnostics.Debug.WriteLine($"Eksport zakończony: {pathCandidate}");
            ReportHistory.Insert(0, new IndustrialPanel.Models.ReportHistoryItem
            {
                FileName = System.IO.Path.GetFileName(pathCandidate!),
                FullPath = pathCandidate!,
                CreatedAt = DateTime.Now,
                DeviceId = SelectedDeviceId
            });
            ShowExportStatus("Eksport zakończony");
        }
        catch (OperationCanceledException)
        {
            ShowExportStatus("Eksport anulowany");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Błąd eksportu: {ex.Message}");
            ShowExportStatus("Błąd eksportu");
        }
    }

    private void ShowExportStatus(string message)
    {
        ExportStatusMessage = message;
        IsExportStatusVisible = true;
        DispatcherTimer.RunOnce(() => { IsExportStatusVisible = false; }, TimeSpan.FromSeconds(4));
    }
    
    [RelayCommand]
    private void SelectDevice(string deviceId)
    {
        SelectedDeviceId = deviceId;
        LoadHistoricalData();
    }
    
    [RelayCommand]
    private void Logout()
    {
        _dataService.StopRealTimeSimulation();
        _authService.Logout();
        LogoutRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void CancelExport()
    {
    _exportCts?.Cancel();
    IsExporting = false;
    ShowExportStatus("Eksport anulowany");
    }
}
