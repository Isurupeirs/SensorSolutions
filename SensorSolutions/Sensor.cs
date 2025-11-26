namespace SensorSolutons;

using System.Text.Json;
using System.Collections.Generic;
using System.IO;
using System.Data.SQLite;

public class Sensor
{
    // Public configuration fields
    public string Name { get; init; }
    public string Location { get; init; }
    public int MinValue { get; init; }
    public int MaxValue { get; init; }

    public bool EnableSmoothing { get; init; }
    public int WindowSize { get; init; }

    public bool EnableAnomalyDetection { get; init; }
    public double AnomalyLimit { get; init; }

    public double WarningLimit { get; init; }
    public double CriticalLimit { get; init; }

    // Internal sensor state
    private readonly Random _rng = new Random();
    private bool _active = false;
    private double _temperature = 0;

    private double _noiseLevel = 0.05;
    private readonly Queue<double> _recentTemps;

    // Fault mode data
    private bool _faultActive = false;
    private double _faultStartTemp = 0;
    private double _faultGrowthRate = 0.1;
    private int _faultTicks = 0;

    private double _coolingProgress = 0;
    private double _coolingSpeed = 0.1;

    private const int MaxFaultTicks = 50;
    private const double MaxFaultTemperature = 50;

    public Sensor(
        string name,
        string location,
        int min,
        int max,
        bool smoothing,
        int smoothingWindow,
        bool anomaly,
        double anomalyThresh,
        double warnThresh,
        double critThresh)
    {
        Name = name;
        Location = location;
        MinValue = min;
        MaxValue = max;

        EnableSmoothing = smoothing;
        WindowSize = smoothingWindow;

        EnableAnomalyDetection = anomaly;
        AnomalyLimit = anomalyThresh;

        WarningLimit = warnThresh;
        CriticalLimit = critThresh;

        _recentTemps = new Queue<double>(WindowSize);
    }

    // Activate the sensor so it can produce readings
    public void StartSensor()
    {
        _active = true;
        Console.WriteLine($"{Name} activated at {Location}.");
    }

    // Trigger a fault (temperature will spike)
    public void InjectFault()
    {
        _faultActive = true;
        Console.WriteLine($"Fault injected on {Name}. Temperature rising abnormally.");
    }

    // Main reading generator
    public double SimulateData()
    {
        if (!_active)
            return double.NaN;

        // First reading
        if (_temperature == 0)
        {
            _temperature = _rng.NextDouble() * (MaxValue - MinValue) + MinValue;
            _faultStartTemp = _temperature;
        }
        else
        {
            if (_faultActive)
            {
                HandleFaultBehaviour();
            }
            else
            {
                ApplyNormalFluctuations();
            }
        }

        // Maintain smoothing history
        if (_recentTemps.Count >= WindowSize)
            _recentTemps.Dequeue();

        _recentTemps.Enqueue(_temperature);

        return _temperature;
    }

    private void HandleFaultBehaviour()
    {
        _faultTicks++;

        double t = _faultTicks / 10.0;

        // Fault duration exceeded → cooling back down
        if (_faultTicks > MaxFaultTicks)
        {
            _faultActive = false;
            _faultTicks = 0;
            _coolingProgress = 0;
            Console.WriteLine($"{Name} fault has stabilized.");
            return;
        }

        // Cooling period (returning to original)
        if (_faultTicks > MaxFaultTicks - 10)
        {
            if (_coolingProgress < 1)
            {
                double step = (_faultStartTemp - _temperature) * _coolingSpeed;
                _temperature += step;
                _coolingProgress += _coolingSpeed;

                if (_coolingProgress >= 1)
                    _temperature = _faultStartTemp;
            }
            return;
        }

        // Exponential temperature spike
        _temperature = _faultStartTemp * Math.Exp(_faultGrowthRate * t);

        if (_temperature > MaxFaultTemperature)
            _temperature = MaxFaultTemperature;
    }

    private void ApplyNormalFluctuations()
    {
        // Natural drift
        double drift = _rng.NextDouble() * _noiseLevel * 2 - _noiseLevel;
        _temperature += drift;

        // Keep inside allowed limits
        if (_temperature < MinValue) _temperature = MinValue;
        if (_temperature > MaxValue) _temperature = MaxValue;

        // Randomly adjust noise level
        _noiseLevel += (_rng.NextDouble() * 0.2 - 0.1);
        _noiseLevel = Math.Clamp(_noiseLevel, 0.02, 0.3);

        _faultStartTemp = _temperature;
    }

    // Moving average smoothing
    public double SmoothData()
    {
        if (_recentTemps.Count == 0)
            return double.NaN;

        if (!EnableSmoothing)
            return _recentTemps.Last();

        return _recentTemps.Average();
    }

    // Check if reading is inside allowed boundaries
    public bool ValidateData(double value)
    {
        if (double.IsNaN(value))
            return false;

        return value >= MinValue && value <= MaxValue;
    }

    // Compare current reading to recent average to check anomalies
    public bool DetectAnomaly(double value, string db = "Data Source=SensorData.db;Version=3;")
    {
        double avg = SensorData.GetAverageOfRecentTemperatures(Name, db);

        if (double.IsNaN(avg) || avg == 0)
            return false;

        return Math.Abs(value - avg) > AnomalyLimit;
    }

    // Return text alert based on thresholds
    public string CheckThreshold(double v)
    {
        if (v > CriticalLimit)
            return $"\nCRITICAL! {Name} at {Location} exceeded {CriticalLimit}°C.";

        if (v > WarningLimit)
            return $"\nWarning! {Name} at {Location} passed {WarningLimit}°C.";

        return $"\n{Name} at {Location} is normal.";
    }

    // Turn off sensor and optionally reset or wipe data
    public void ShutdownSensor(bool reset = false, bool clearLogs = false, bool clearDb = false)
    {
        if (!_active)
        {
            Console.WriteLine($"{Name} already inactive.");
            return;
        }

        _active = false;
        Console.WriteLine($"{Name} shut down.");

        if (reset)
        {
            _recentTemps.Clear();
            _temperature = 0;
            _noiseLevel = 0.05;
        }

        if (clearLogs)
            SensorData.ClearLogFileEntries(Name);

        if (clearDb)
            SensorData.ClearDatabaseEntries(Name);
    }
}
