using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace SensorSolutions
{
    public class SensorData
    {
        public List<Sensor> Sensors { get; set; } = new();

        private const string DbConn = "Data Source=SensorData.db;";

        // ---------------- DATABASE SETUP ---------------------

        public static void EnsureDatabaseSetup(string conn)
        {
            using var db = new SqliteConnection(conn);
            db.Open();

            string sql = @"
                CREATE TABLE IF NOT EXISTS Readings (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    SensorName TEXT,
                    Timestamp TEXT,
                    Value REAL
                );
            ";

            using var cmd = new SqliteCommand(sql, db);
            cmd.ExecuteNonQuery();
        }

        // ---------------- JSON CONFIG -----------------------

        public static List<Sensor> InitialiseSensors(string jsonFile)
        {
            if (!File.Exists(jsonFile))
                throw new FileNotFoundException(jsonFile);

            string json = File.ReadAllText(jsonFile);
            return JsonSerializer.Deserialize<List<Sensor>>(json)!;
        }

        // ---------------- LOGGING ---------------------------

        public string LogData(Sensor sensor, double value)
        {
            using var db = new SqliteConnection(DbConn);
            db.Open();

            string sql = "INSERT INTO Readings (SensorName, Timestamp, Value) VALUES (@n, @t, @v)";
            using var cmd = new SqliteCommand(sql, db);

            cmd.Parameters.AddWithValue("@n", sensor.Name);
            cmd.Parameters.AddWithValue("@t", DateTime.Now.ToString("O"));
            cmd.Parameters.AddWithValue("@v", value);
            cmd.ExecuteNonQuery();

            string log = $"{sensor.Name} | {DateTime.Now:HH:mm:ss} | {value:F1}°C";
            File.AppendAllText("SensorActivity.log", log + Environment.NewLine);

            return log;
        }

        // ---------------- ANALYTICS --------------------------

        public static double GetAverageOfRecentTemperatures(string sensorName, string db = DbConn)
        {
            using var con = new SqliteConnection(db);
            con.Open();

            string sql = @"SELECT Value FROM Readings
                           WHERE SensorName=@name
                           ORDER BY Id DESC
                           LIMIT 10";

            using var cmd = new SqliteCommand(sql, con);
            cmd.Parameters.AddWithValue("@name", sensorName);

            var values = new List<double>();

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                    values.Add(reader.GetDouble(0));
            }

            return values.Count > 0 ? values.Average() : double.NaN;
        }

        // ---------------- CLEANUP ----------------------------

        public static void ClearLogFileEntries(string sensorName)
        {
            if (!File.Exists("SensorActivity.log"))
                return;

            var lines = File.ReadAllLines("SensorActivity.log")
                            .Where(l => !l.Contains(sensorName));

            File.WriteAllLines("SensorActivity.log", lines);
        }

        public static void ClearDatabaseEntries(string sensorName)
        {
            using var db = new SqliteConnection(DbConn);
            db.Open();

            string sql = "DELETE FROM Readings WHERE SensorName=@n";
            using var cmd = new SqliteCommand(sql, db);

            cmd.Parameters.AddWithValue("@n", sensorName);
            cmd.ExecuteNonQuery();
        }
    }
}
