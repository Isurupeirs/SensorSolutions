Project Overview

DataCentreTempMonitor is a .NET console-based application that simulates how temperature sensors operate inside a data centre. It generates temperature readings, validates and logs them, stores historical data, and provides additional functionality such as anomaly detection, fault injection, and warning/critical alerts.


Contents

Project Overview

Contents

Features

Installation

Usage

Configuration

Testing

Features

Sensor Setup: Loads virtual sensor definitions from a JSON config file.

Temperature Simulation: Produces temperature readings with randomness and natural fluctuations.

Range Validation: Ensures readings stay within a safe operating range (22–24°C).

Logging: Writes valid readings with timestamps to both the console and a log file.

Historical Data Storage: Saves processed readings into an SQLite database.

Moving Average Smoothing: Reduces noise by calculating smoothed temperature values.

Anomaly Detection: Identifies readings that deviate significantly from recent averages.

Sensor Reset & Shutdown: Allows clearing internal history and resetting values.

Fault Injection (Extra): Mimics sensor malfunction or abnormal temperature spikes.

Threshold Alerts (Extra): Warning and critical notifications when temperatures exceed limits.

Custom Thresholds (Extra): User-configurable alert thresholds.

Unit Testing: xUnit tests covering core system behaviours.

Installation

After cloning the project, complete the following setup steps:

Restore dependencies

dotnet restore


Build the solution

dotnet build


Run the main application

dotnet run --project DataCentreTempMonitor


Execute tests

dotnet test


Check test coverage (requires Coverage Gutters extension)

dotnet test --collect:"XPlat Code Coverage"


Then open the generated XML file inside the TestResults directory to view coverage in VS Code.

Usage

Once launched, the application begins simulating temperature data continuously. All readings are:

validated

logged to SensorDataLog.txt

stored in SensorData.db (overwritten each run unless database clearing is disabled)

To run:

dotnet run --project DataCentreTempMonitor


Key behaviour:

Smoothed readings are stored in the database.

By default, the database resets every run.
To retain previous data, comment out:
SensorData.ClearDatabase(connectionString);

Sensor behaviour (thresholds, ranges, smoothing, anomalies, etc.) is controlled through SensorConfig.json.

Configuration

The application uses a JSON configuration file to define the behaviour of each sensor.

Configurable Properties

Sensors: List of all simulated sensors.

Name: Sensor identifier.

Location: Physical/virtual location in the data centre.

MinValue / MaxValue: Expected operational temperature range.

DataSmoothingEnabled: Enables moving average smoothing.

SmoothingWindowSize: Number of readings used for smoothing.

AnomalyDetectionEnabled: Toggles anomaly checks.

AnomalyThreshold: Maximum allowed deviation from long-term average.

WarningThreshold: Temperature at which a warning is triggered.

CriticalThreshold: Temperature at which a critical alert is triggered.

Modifying or Adding Sensors

Duplicate an existing sensor entry in the JSON file.

Update names, ranges, and thresholds as needed.

Smoothing and anomaly detection can be enabled/disabled per sensor.

The application automatically reads the configuration file at startup and adjusts sensor behaviour accordingly.

Testing

All core functionality is validated using the xUnit testing framework.

To run tests:

dotnet test


To generate code coverage:

dotnet test --collect:"XPlat Code Coverage"

Test Coverage Includes:

Sensor Construction: Ensures configuration values load correctly.

Data Simulation: Checks temperature values remain within valid ranges.

Validation Logic: Confirms out-of-range values fail validation.

Logging: Ensures valid readings are properly recorded.

Database Storage: Tests insertion and updating of historical data.

Smoothing Algorithm: Confirms moving average produces expected output.

Anomaly Detection: Verifies detection when readings change sharply.

Shutdown/Reset: Ensures history clearing and resets work correctly.

Fault Simulation: Confirms spikes or faults behave as intended.

Threshold Alerts: Checks correct messages for warning/critical states.

Custom Thresholds: Validates user-defined alert values operate correctly.