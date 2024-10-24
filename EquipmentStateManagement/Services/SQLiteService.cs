using EquipmentStateManagement.Models;
using Microsoft.Data.Sqlite;

namespace EquipmentStateManagement.Services
    {
    public class SQLiteService
        {
        private readonly string _connectionString;
        public SQLiteService(string connectionString)
            {
            _connectionString = connectionString;

            using (var connection = new SqliteConnection(_connectionString))
                {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS EquipmentHistory (
                        HistoryId INTEGER PRIMARY KEY AUTOINCREMENT,
                        Id TEXT NOT NULL,
                        Name TEXT NOT NULL,
                        State TEXT NOT NULL,
                        Timestamp TEXT NOT NULL
                    )";
                command.ExecuteNonQuery();
                }
            }

        public async Task InsertEquipmentHistory(Equipment equipment)
            {
            var timestamp = DateTime.UtcNow.ToString("o");

            using (var connection = new SqliteConnection(_connectionString))
                {
                await connection.OpenAsync();
                var command = connection.CreateCommand();
                command.CommandText = @"
            INSERT INTO EquipmentHistory (Id, Name, State, Timestamp)
            VALUES (@Id, @Name, @State, @Timestamp)";
                command.Parameters.AddWithValue("@Id", equipment.Id.ToString());
                command.Parameters.AddWithValue("@Name", equipment.Name);
                command.Parameters.AddWithValue("@State", equipment.State);
                command.Parameters.AddWithValue("@Timestamp", timestamp);
                await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
