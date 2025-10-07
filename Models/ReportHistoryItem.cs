using System;

namespace IndustrialPanel.Models;

public class ReportHistoryItem
{
    public string FileName { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string DeviceId { get; set; } = string.Empty;
}
