using MySql.Data.MySqlClient;
using System;
using System.Threading.Tasks;

public class DatabaseManager
{
    private readonly string _connectionString;

    public DatabaseManager(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task InitializeDatabaseAsync()
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        string createTableQuery = @"
            CREATE TABLE IF NOT EXISTS Users (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Username VARCHAR(50) NOT NULL,
                Password VARCHAR(50) NOT NULL
            );";

        using var command = new MySqlCommand(createTableQuery, connection);
        await command.ExecuteNonQueryAsync();
    }

    public async Task InsertUserAsync(string username, string password)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        string insertQuery = "INSERT INTO Users (Username, Password) VALUES (@username, @password)";
        using var command = new MySqlCommand(insertQuery, connection);
        command.Parameters.AddWithValue("@username", username);
        command.Parameters.AddWithValue("@password", password);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<bool> ValidateUserAsync(string username, string password)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        string selectQuery = "SELECT COUNT(*) FROM Users WHERE Username = @username AND Password = @password";
        using var command = new MySqlCommand(selectQuery, connection);
        command.Parameters.AddWithValue("@username", username);
        command.Parameters.AddWithValue("@password", password);

        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        return count > 0;
    }

    internal async Task<(string username, string password)> GetFirstUserAsync()
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        string query = "SELECT Username, Password FROM Users LIMIT 1";
        using var command = new MySqlCommand(query, connection);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            string username = reader.GetString(0); // Columna "Username"
            string password = reader.GetString(1); // Columna "Password"
            return (username, password);
        }
        else
        {
            throw new InvalidOperationException("No users found in the database.");
        }
    }

}

