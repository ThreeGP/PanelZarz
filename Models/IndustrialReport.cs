using System;
using System.Collections.Generic;

namespace IndustrialPanel.Models;

public class IndustrialReport
{
    public string ReportId { get; set; } = Guid.NewGuid().ToString();
    public DateTime GeneratedDate { get; set; } = DateTime.Now;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    
    // Statystyki
    public double AverageSpeed { get; set; }
    public double MaxSpeed { get; set; }
    public double MinSpeed { get; set; }
    public double AverageTonsPerHour { get; set; }
    public double TotalTons { get; set; }
    public TimeSpan TotalRuntime { get; set; }
    public int ErrorCount { get; set; }
    public int WarningCount { get; set; }
    
    // Dane szczegółowe
    public List<ConveyorBeltData> DetailedData { get; set; } = new();
}
