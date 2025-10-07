using Avalonia.Controls;
using Avalonia.Input;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView.Avalonia;
using System;
using System.Diagnostics;

namespace IndustrialPanel.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var exportBtn = this.FindControl<Button>("ExportButton");
        if (exportBtn != null)
        {
            exportBtn.Click -= ExportBtn_Click;
            exportBtn.Click += ExportBtn_Click;
        }

        var exportBtnPanel = this.FindControl<Button>("ExportButtonPanel");
        if (exportBtnPanel != null)
        {
            exportBtnPanel.Click -= ExportBtn_Click;
            exportBtnPanel.Click += ExportBtn_Click;
        }


        var speedChart = this.FindControl<CartesianChart>("SpeedChart");
        var throughputChart = this.FindControl<CartesianChart>("ThroughputChart");

        void TuneChart(CartesianChart? chart)
        {
            if (chart == null) return;
            chart.AnimationsSpeed = TimeSpan.FromMilliseconds(250);
            chart.ZoomMode = ZoomAndPanMode.X;
        }

        TuneChart(speedChart);
        TuneChart(throughputChart);

        AddHandler(Button.ClickEvent, (_, e) =>
        {
            if (e.Source is Button b && b.Tag is string path && !string.IsNullOrWhiteSpace(path))
            {
                try
                {
                    if (b.Content?.ToString() == "Otwórz")
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = "open",
                            ArgumentList = { path },
                            UseShellExecute = false
                        });
                    }
                    else if (b.Content?.ToString() == "Pokaż w Finder")
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = "open",
                            ArgumentList = { "-R", path },
                            UseShellExecute = false
                        });
                    }
                }
                catch { }
            }
        });
    }
    
    private void DeviceCard_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (DataContext is IndustrialPanel.ViewModels.MainWindowViewModel vm && sender is Border border && border.Tag is string deviceId)
        {
            vm.SelectDeviceCommand.Execute(deviceId);
        }
    }

    private async void ExportBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
    if (DataContext is IndustrialPanel.ViewModels.MainWindowViewModel vm)
        {
#pragma warning disable CS0618
        var sfd = new SaveFileDialog
            {
                Title = "Zapisz raport Excel",
                InitialFileName = $"raport_{System.DateTime.Now:yyyyMMdd}.xlsx",
                DefaultExtension = "xlsx",
                Filters =
                {
                    new FileDialogFilter { Name = "Pliki Excel (*.xlsx)", Extensions = { "xlsx" } }
                }
            };
            var path = await sfd.ShowAsync(this);
#pragma warning restore CS0618
            if (!string.IsNullOrWhiteSpace(path))
            {
                await vm.ExportReportToPathCommand.ExecuteAsync(path);
            }
        }
    }
}