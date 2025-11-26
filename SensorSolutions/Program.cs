using SensorSolutions;

try
{
    // Path to the SQLite database file
    var dbPath = "Data Source=SensorData.db;Version=3;";

    // Clear any previous database data and recreate the tables
    SensorData.ClearDatabase(dbPath);
    SensorData.EnsureDatabaseSetup(dbPath);

    // Clear the log file so the program starts fresh
    File.WriteAllText("SensorActivity.log", string.Empty);

    // The JSON file that contains the sensor settings
    string configFile = "SensorConfig.json";

    // Create an object that will manage all sensor data
    var manager = new SensorData();

    // Load all sensors from the JSON config file
    var sensorList = SensorData.InitialiseSensors(configFile);

    // Add the loaded sensors to the manager
    manager.Sensors.AddRange(sensorList);

    Console.WriteLine("=== Sensors Loaded ===\n");

    // Show all sensor details on the screen
    foreach (var s in sensorList)
    {
        Console.WriteLine($"Sensor: {s.Name}");
        Console.WriteLine($"Location: {s.Location}");
        Console.WriteLine($"Range: {s.MinValue}°C to {s.MaxValue}°C\n");
    }

    // Start each sensor
    foreach (var s in sensorList)
        s.StartSensor();

    Console.WriteLine("\nFault injection enabled.\n");

    // Turn on fault simulation for each sensor (makes readings sometimes incorrect)
    foreach (var s in sensorList)
        s.InjectFault();

    // Tell the user how to exit the live display
    Console.WriteLine("\x1B[1mPress ESC to stop monitoring...\x1B[0m\n");

    // Keep showing live readings until the user presses ESC
    while (true)
    {
        var buffer = new List<string>();

        // Get a reading from each sensor
        foreach (var sensor in sensorList)
        {
            double reading = sensor.SimulateData();

            // Log the reading in the system
            string record = manager.LogData(sensor, reading);

            // Check if the reading is unusual
            if (sensor.DetectAnomaly(reading))
                record += "  >> ANOMALY";

            buffer.Add(record);
        }

        // Print all sensor readings on the screen
        Console.WriteLine(string.Join("\n", buffer) + "\n");

        // If the user presses ESC, stop the program
        if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
            break;

        // Wait 1 second before the next reading
        Thread.Sleep(1000);
    }

    Console.WriteLine("\nMonitoring stopped.");
}
catch (Exception ex)
{
    // Show any errors that happen during the program
    Console.WriteLine($"Error: {ex.Message}");
}