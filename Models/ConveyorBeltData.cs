using System;

namespace IndustrialPanel.Models;

public class ConveyorBeltData
{
    public string DeviceId { get; set; } = string.Empty;
    
    public string DeviceName { get; set; } = string.Empty;
    
    public double SpeedMetersPerSecond { get; set; }

    public double TonsPerHour { get; set; }

    public DateTime Timestamp { get; set; }

    public DeviceStatus Status { get; set; }

    public double? Temperature { get; set; }
}

public enum DeviceStatus
{
    Stopped,
    Running,
    Warning,
    Error
}
