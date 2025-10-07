using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IndustrialPanel.Models;

namespace IndustrialPanel.Services;

public class IndustrialDataService
{
    private readonly Random _random = new();
    private readonly List<ConveyorBeltData> _historicalData = new();
    private readonly List<string> _deviceIds = new() { "BELT-001", "BELT-002", "BELT-003" };
    private readonly Dictionary<string, string> _deviceNames = new()
    {
        { "BELT-001", "Taśma Główna A" },
        { "BELT-002", "Taśma Główna B" },
        { "BELT-003", "Taśma Pomocnicza C" }
    };
    
    private CancellationTokenSource? _simulationCts;
    
    public IndustrialDataService()
    {
        GenerateHistoricalData();
    }
    
    private void GenerateHistoricalData()
    {
        var startDate = DateTime.Now.AddHours(-24);
        
        foreach (var deviceId in _deviceIds)
        {
            for (int i = 0; i < 288; i++)
            {
                var data = GenerateDataPoint(deviceId, startDate.AddMinutes(i * 5));
                _historicalData.Add(data);
            }
        }
    }
    
    private ConveyorBeltData GenerateDataPoint(string deviceId, DateTime timestamp)
    {
    double baseSpeed = deviceId == "BELT-003" ? 1.5 : 2.0;
    double baseTonsPerHour = deviceId == "BELT-003" ? 150 : 200;
        
        double speedVariation = baseSpeed * (_random.NextDouble() * 0.2 - 0.1);
        double tonsVariation = baseTonsPerHour * (_random.NextDouble() * 0.2 - 0.1);
        
        var status = DeviceStatus.Running;
        if (_random.NextDouble() < 0.02)
            status = DeviceStatus.Warning;
        else if (_random.NextDouble() < 0.01)
            status = DeviceStatus.Error;
        
        return new ConveyorBeltData
        {
            DeviceId = deviceId,
            DeviceName = _deviceNames[deviceId],
            SpeedMetersPerSecond = Math.Round(baseSpeed + speedVariation, 2),
            TonsPerHour = Math.Round(baseTonsPerHour + tonsVariation, 1),
            Timestamp = timestamp,
            Status = status,
            Temperature = Math.Round(45 + _random.NextDouble() * 15, 1)
        };
    }
    
    public List<ConveyorBeltData> GetCurrentData()
    {
    var data = _deviceIds.Select(deviceId => 
            GenerateDataPoint(deviceId, DateTime.Now)
        ).ToList();
        
        return data;
    }
    
    public ConveyorBeltData GetDeviceData(string deviceId)
    {
        return GenerateDataPoint(deviceId, DateTime.Now);
    }
    
    public List<ConveyorBeltData> GetHistoricalData(string deviceId, DateTime startDate, DateTime endDate)
    {
        return _historicalData
            .Where(d => d.DeviceId == deviceId && 
                       d.Timestamp >= startDate && 
                       d.Timestamp <= endDate)
            .OrderBy(d => d.Timestamp)
            .ToList();
    }
    
    public List<ConveyorBeltData> GetRecentData(string deviceId, int hours)
    {
        var startDate = DateTime.Now.AddHours(-hours);
        return GetHistoricalData(deviceId, startDate, DateTime.Now);
    }
    
  
    public List<(string DeviceId, string DeviceName)> GetDevices()
    {
        return _deviceIds.Select(id => (id, _deviceNames[id])).ToList();
    }
    

    public void StartRealTimeSimulation(Action<ConveyorBeltData> onDataReceived)
    {
        _simulationCts?.Cancel();
        _simulationCts = new CancellationTokenSource();
        
        Task.Run(async () =>
        {
            while (!_simulationCts.Token.IsCancellationRequested)
            {
                foreach (var deviceId in _deviceIds)
                {
                    var data = GenerateDataPoint(deviceId, DateTime.Now);
                    _historicalData.Add(data);
                    onDataReceived?.Invoke(data);
                }
                
                await Task.Delay(2000, _simulationCts.Token);
            }
        }, _simulationCts.Token);
    }
    

    public void StopRealTimeSimulation()
    {
        _simulationCts?.Cancel();
    }
}
