using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using IndustrialPanel.Models;

namespace IndustrialPanel.Services;

public class ExcelExportService
{
    public void ExportToExcel(List<ConveyorBeltData> data, string filePath, string deviceName, System.Threading.CancellationToken cancellationToken = default)
    {
    using var workbook = new XLWorkbook();
    var worksheet = workbook.Worksheets.Add("Dane Przemysłowe");
        
        worksheet.Cell(1, 1).Value = "Raport Systemu Przemysłowego";
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 16;
        
        worksheet.Cell(2, 1).Value = $"Urządzenie: {deviceName}";
        worksheet.Cell(3, 1).Value = $"Data wygenerowania: {DateTime.Now:dd.MM.yyyy HH:mm:ss}";
        worksheet.Cell(4, 1).Value = $"Liczba rekordów: {data.Count}";
        
        int headerRow = 6;
        worksheet.Cell(headerRow, 1).Value = "Data i Czas";
        worksheet.Cell(headerRow, 2).Value = "Urządzenie";
        worksheet.Cell(headerRow, 3).Value = "Prędkość (m/s)";
        worksheet.Cell(headerRow, 4).Value = "Przepustowość (t/h)";
        worksheet.Cell(headerRow, 5).Value = "Status";
        worksheet.Cell(headerRow, 6).Value = "Temperatura (°C)";
        
        var headerRange = worksheet.Range(headerRow, 1, headerRow, 6);
        headerRange.Style.Font.Bold = true;
    headerRange.Style.Fill.BackgroundColor = XLColor.Lavender;
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        
        int currentRow = headerRow + 1;
        foreach (var item in data.OrderBy(d => d.Timestamp))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException(cancellationToken);
            }
            worksheet.Cell(currentRow, 1).Value = item.Timestamp.ToString("dd.MM.yyyy HH:mm:ss");
            worksheet.Cell(currentRow, 2).Value = item.DeviceName;
            worksheet.Cell(currentRow, 3).Value = item.SpeedMetersPerSecond;
            worksheet.Cell(currentRow, 4).Value = item.TonsPerHour;
            worksheet.Cell(currentRow, 5).Value = item.Status.ToString();
            worksheet.Cell(currentRow, 6).Value = item.Temperature ?? 0;
            
            var statusCell = worksheet.Cell(currentRow, 5);
            statusCell.Style.Font.Bold = true;
            statusCell.Style.Fill.BackgroundColor = item.Status switch
            {
                DeviceStatus.Running => XLColor.MediumPurple,
                DeviceStatus.Warning => XLColor.Plum,
                DeviceStatus.Error => XLColor.Purple,
                DeviceStatus.Stopped => XLColor.Gray,
                _ => XLColor.White
            };
            
            currentRow++;
        }
        
        int statsRow = currentRow + 2;
        worksheet.Cell(statsRow, 1).Value = "STATYSTYKI";
        worksheet.Cell(statsRow, 1).Style.Font.Bold = true;
        worksheet.Cell(statsRow, 1).Style.Font.FontSize = 14;
        
        statsRow++;
        worksheet.Cell(statsRow, 1).Value = "Średnia prędkość:";
        worksheet.Cell(statsRow, 2).Value = Math.Round(data.Average(d => d.SpeedMetersPerSecond), 2);
        worksheet.Cell(statsRow, 3).Value = "m/s";
        
        statsRow++;
        worksheet.Cell(statsRow, 1).Value = "Maks. prędkość:";
        worksheet.Cell(statsRow, 2).Value = Math.Round(data.Max(d => d.SpeedMetersPerSecond), 2);
        worksheet.Cell(statsRow, 3).Value = "m/s";
        
        statsRow++;
        worksheet.Cell(statsRow, 1).Value = "Min. prędkość:";
        worksheet.Cell(statsRow, 2).Value = Math.Round(data.Min(d => d.SpeedMetersPerSecond), 2);
        worksheet.Cell(statsRow, 3).Value = "m/s";
        
        statsRow++;
        worksheet.Cell(statsRow, 1).Value = "Średnia przepustowość:";
        worksheet.Cell(statsRow, 2).Value = Math.Round(data.Average(d => d.TonsPerHour), 1);
        worksheet.Cell(statsRow, 3).Value = "t/h";
        
        statsRow++;
        worksheet.Cell(statsRow, 1).Value = "Całkowita masa:";
    worksheet.Cell(statsRow, 2).Value = Math.Round(data.Sum(d => d.TonsPerHour) / 12, 1);
        worksheet.Cell(statsRow, 3).Value = "ton";
        
        statsRow++;
        worksheet.Cell(statsRow, 1).Value = "Liczba błędów:";
        worksheet.Cell(statsRow, 2).Value = data.Count(d => d.Status == DeviceStatus.Error);
        
        statsRow++;
        worksheet.Cell(statsRow, 1).Value = "Liczba ostrzeżeń:";
        worksheet.Cell(statsRow, 2).Value = data.Count(d => d.Status == DeviceStatus.Warning);
        
        worksheet.Columns().AdjustToContents();
        
        try
        {
            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException(cancellationToken);
            var dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
            if (File.Exists(filePath)) File.Delete(filePath);
            workbook.SaveAs(filePath);
        }
        catch (IOException)
        {
            var fallback = Path.Combine(Path.GetDirectoryName(filePath) ?? "", $"{Path.GetFileNameWithoutExtension(filePath)}_{DateTime.Now:HHmmss}.xlsx");
            workbook.SaveAs(fallback);
        }
    }
    
    public void ExportReportToExcel(IndustrialReport report, string filePath, System.Threading.CancellationToken cancellationToken = default)
    {
        ExportToExcel(report.DetailedData, filePath, report.DeviceName, cancellationToken);
    }
}
