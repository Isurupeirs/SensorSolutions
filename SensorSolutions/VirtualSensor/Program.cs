using System;
using System.Threading;

namespace Sensors
{
    internal static class Program
    {
        private static readonly Random Random = new();

        private static void Main(string[] args)
        {

            string sensorName = "testSensor";

            // TODO: needs to initialize name from file
            
            Console.WriteLine($"Simulation started for sensor : {sensorName}");




            // simulation loop
            while (true)
            {
                double tempC = GenerateTemperature();
                Console.WriteLine($"{DateTime.Now:HH:mm:ss}  Temperature: {tempC:F1} °C");
                Thread.Sleep(1000);
                var reading = new Reading();
                reading.SensorName = sensorName;
                reading.DateTime = DateTime.Now;
                reading.Value = tempC;
              

            }
        }

        private static double GenerateTemperature()
        {
            const double mean = 22.0;
            const double variation = 5.0;
            return mean + (Random.NextDouble() * 2 - 1) * variation;
        }
    }
}
