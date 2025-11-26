using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Data.SQLite;


namespace SensorSolutions.Tests // Renamed namespace
{

    public class SensorBehaviourSuite : IDisposable
    {
        private readonly string _tempConfigPath;
        private const string DbConn = "Data Source=SensorData.db;Version=3;";

        public SensorBehaviourSuite()
        {
            // Each test receives a uniquely named JSON file
            _tempConfigPath = Path.Combine(Path.GetTempPath(),
                $"SensorCfg_{Guid.NewGuid():N}.json");

            // Ensure a clean DB before each test group
            SensorData.ClearDatabase(DbConn);
            SensorData.EnsureDatabaseSetup(DbConn);
        }


        public void Dispose()
        {
            try
            {
                if (File.Exists(_tempConfigPath))
                    File.Delete(_tempConfigPath);
            }
            catch
            {
                // If Windows file locking delays deletion, we ignore it.
            }

            SensorData.ClearDatabase(DbConn);
        }


        [Fact]
        public void ConfigLoader_ShouldParseSensorsCorrectly()
        {
            var sampleJson = """
            {
                "Sensors": [
                    {
                        "Name": "A1",
                        "Location": "Rack-A",
                        "MinValue": 5,
                        "MaxValue": 55,
                        "DataSmoothingEnabled": true,
                        "SmoothingWindowSize": 6,
                        "AnomalyDetectionEnabled": true,
                        "AnomalyThreshold": 0.8,
                        "WarningThreshold": 45.0,
                        "CriticalThreshold": 50.0
                    },
                    {
                        "Name": "B2",
                        "Location": "Rack-B",
                        "MinValue": -2,
                        "MaxValue": 12,
                        "DataSmoothingEnabled": false,
                        "SmoothingWindowSize": 3,
                        "AnomalyDetectionEnabled": false,
                        "AnomalyThreshold": 0.4,
                        "WarningThreshold": 10.0,
                        "CriticalThreshold": 11.0
                    }
                ]
            }
            """;

            File.WriteAllText(_tempConfigPath, sampleJson);

            var cfg = SensorData.LoadSensorsConfig(_tempConfigPath);

            Assert.NotNull(cfg);
            Assert.Equal(2, cfg.Sensors.Count);

            // First sensor
            var first = cfg.Sensors[0];
            Assert.Equal("A1", first.Name);
            Assert.Equal("Rack-A", first.Location);
            Assert.True(first.DataSmoothingEnabled);
            Assert.Equal(6, first.SmoothingWindowSize);
            Assert.True(first.AnomalyDetectionEnabled);
            Assert.Equal(0.8, first.AnomalyThreshold);

            // Second sensor
            var second = cfg.Sensors[1];
            Assert.Equal("B2", second.Name);
            Assert.False(second.DataSmoothingEnabled);
            Assert.False(second.AnomalyDetectionEnabled);
            Assert.Equal(0.4, second.AnomalyThreshold);
        }

        [Fact]
        public void ConfigLoader_ShouldThrow_WhenFileMissing()
        {
            var missingPath = Path.Combine(Path.GetTempPath(),
                                             $"ghost_{Guid.NewGuid()}.json");

            var err = Assert.Throws<FileNotFoundException>(() =>
                SensorData.LoadSensorsConfig(missingPath));

            Assert.Equal("Sensor configuration file not found.", err.Message);
        }

        [Fact]
        public void ConfigLoader_ShouldThrow_ForMalformedJson()
        {
            var brokenJson = """
            {
                "Sensors" : [
                    {
                        "Name": "X",
                        "MinValue": "NotANumber",
                        "MaxValue": 30
                    }
                ]
            }
            """;

            File.WriteAllText(_tempConfigPath, brokenJson);

            var ex = Assert.Throws<JsonException>(() =>
                SensorData.LoadSensorsConfig(_tempConfigPath));

            Assert.Contains("Failed to deserialize", ex.Message);
        }
    }
}