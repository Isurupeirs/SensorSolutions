using System;
using System.Collections.Generic;
using System.Linq;

namespace SensorSolutions
{
    public class Sensor
    {
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

        private readonly Random _rng = new();
        private bool _active = false;
        private double _temperature = double.NaN;

        private double _noiseLevel = 0.05;
        private Queue<double> _recentTemps;

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
            WindowSize = Math.Max(1, smoothingWindow);

            EnableAnomalyDetection = anomaly;
            AnomalyLimit = anomalyThresh;

            WarningLimit = warnThresh;
            CriticalLimit = critThresh;

            _recentTemps = new Queue<double>(WindowSize);
        }

        public void StartSensor()
        {
            _active = true;
            Console.WriteLine($"{Name} activated at {Location}.");
        }

        public void InjectFault()
        {
            _faultActive = true;
            _faultTicks = 0;
            Console.WriteLine($"Fault injected on {Name}. Temperature rising abnormally.");
        }

        public double SimulateData()
        {
            if (!_active)
                return double.NaN;

            if (double.IsNaN(_temperature))
            {
                _temperature = _rng.NextDouble() * (MaxValue - MinValue) + MinValue;
                _faultStartTemp = _temperature;
            }
            else
            {
                if (_faultActive)
                    HandleFaultBehaviour();
                else
                    ApplyNormalFluctuations();
            }

            if (_recentTemps.Count >= WindowSize)
                _recentTemps.Dequeue();

            _recentTemps.Enqueue(_temperature);

            return _temperature;
        }

        private void HandleFaultBehaviour()
        {
            _faultTicks++;

            double t = _faultTicks / 10.0;

            if (_faultTicks > MaxFaultTicks)
            {
                _faultActive = false;
                _faultTicks = 0;
                _coolingProgress = 0;
                Console.WriteLine($"{Name} fault has stabilized.");
                return;
            }

            if (_faultTicks > MaxFaultTicks - 10)
            {
                double step = (_faultStartTemp - _temperature) * _coolingSpeed;
                _temperature += step;

                _coolingProgress += _coolingSpeed;

                if (_coolingProgress >= 1)
                    _temperature = _faultStartTemp;

                return;
            }

            _temperature = _faultStartTemp * Math.Exp(_faultGrowthRate * t);
            if (_temperature > MaxFaultTemperature)
                _temperature = MaxFaultTemperature;
        }

        private void ApplyNormalFluctuations()
        {
            double drift = _rng.NextDouble() * _noiseLevel * 2 - _noiseLevel;

            _temperature += drift;
            _temperature = Math.Clamp(_temperature, MinValue, MaxValue);

            _noiseLevel += (_rng.NextDouble() * 0.2 - 0.1);
            _noiseLevel = Math.Clamp(_noiseLevel, 0.02, 0.3);

            _faultStartTemp = _temperature;
        }

        public double SmoothData()
        {
            if (_recentTemps.Count == 0)
                return double.NaN;

            return EnableSmoothing
                ? _recentTemps.Average()
                : _recentTemps.Last();
        }

        public bool ValidateData(double value)
        {
            if (double.IsNaN(value))
                return false;

            return value >= MinValue && value <= MaxValue;
        }

        public bool DetectAnomaly(double value)
        {
            if (!EnableAnomalyDetection)
                return false;

            double avg = SensorData.GetAverageOfRecentTemperatures(Name);

            if (double.IsNaN(avg) || avg == 0)
                return false;

            return Math.Abs(value - avg) > AnomalyLimit;
        }

        public string CheckThreshold(double v)
        {
            if (v > CriticalLimit)
                return $"\nCRITICAL — {Name} exceeded {CriticalLimit}°C";

            if (v > WarningLimit)
                return $"\nWarning — {Name} passed {WarningLimit}°C";

            return "";
        }

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
                _temperature = double.NaN;
                _noiseLevel = 0.05;
            }

            if (clearLogs)
                SensorData.ClearLogFileEntries(Name);

            if (clearDb)
                SensorData.ClearDatabaseEntries(Name);
        }
    }
}
