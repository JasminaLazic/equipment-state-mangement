using EquipmentStateManagement.Models;
using Microsoft.Data.Sqlite;

namespace EquipmentStateManagement.Services
{
    public class SQLiteService
    {
        private readonly SqliteConnection _connection;

        public SQLiteService(string connectionString)
        {
            _connection = new SqliteConnection(connectionString);
            _connection.Open();

            var command = _connection.CreateCommand();
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

        public async Task InsertEquipmentHistory(Equipment equipment)
        {
            var timestamp = DateTime.UtcNow.ToString("o");

            var command = _connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO EquipmentHistory (Id, Name, State, Timestamp)
                VALUES (@Id, @Name, @State, @Timestamp)";
            command.Parameters.AddWithValue("@Id", equipment.Id.ToString());
            command.Parameters.AddWithValue("@Name", equipment.Name);
            command.Parameters.AddWithValue("@State", equipment.State);
            command.Parameters.AddWithValue("@Timestamp", timestamp);
            await command.ExecuteNonQueryAsync();
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}
