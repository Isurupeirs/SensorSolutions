using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using SensorSolutions;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            const string DbConn = "Data Source=SensorData.db;";
            const string Config = "SensorConfig.json";

            // Setup DB
            SensorData.EnsureDatabaseSetup(DbConn);

            // Clear log file
            File.WriteAllText("SensorActivity.log", "");

            Console.WriteLine("\nLoading sensors...\n");

            var manager = new SensorData();
            var sensors = SensorData.InitialiseSensors(Config);

            manager.Sensors.AddRange(sensors);

            foreach (var s in sensors)
            {
                Console.WriteLine($"Sensor: {s.Name}");
                Console.WriteLine($"Location: {s.Location}");
                Console.WriteLine($"Range: {s.MinValue}°C to {s.MaxValue}°C\n");
            }

            foreach (var s in sensors) s.StartSensor();
            foreach (var s in sensors) s.InjectFault();

            Console.WriteLine("\nPress ESC to stop monitoring...\n");

            while (true)
            {
                List<string> rows = new();

                foreach (var sensor in sensors)
                {
                    double reading = sensor.SimulateData();
                    double smooth = sensor.SmoothData();

                    string log = manager.LogData(sensor, reading);

                    if (sensor.EnableAnomalyDetection && sensor.DetectAnomaly(reading))
                        log += "  >> ANOMALY";

                    string thresholdMsg = sensor.CheckThreshold(reading);

                    rows.Add($"{log} | Smoothed: {smooth:F1}°C {thresholdMsg}");
                }

                Console.WriteLine(string.Join("\n", rows) + "\n");

                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                    break;

                Thread.Sleep(1000);
            }

            Console.WriteLine("\nMonitoring stopped.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
