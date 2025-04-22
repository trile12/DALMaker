using System.Collections.Generic;
using System.Threading.Tasks;

namespace CodeGenTool.Services
{
	using MySql.Data.MySqlClient;

	public class MySqlService
	{
		private readonly string _connectionString;

		public MySqlService(string server, string user, string password, string database = "")
		{
			_connectionString = $"Server={server};Uid={user};Pwd={password};" +
								(string.IsNullOrEmpty(database) ? "" : $"Database={database};");
		}

		public async Task<List<string>> GetDatabasesAsync()
		{
			var result = new List<string>();
			using var connection = new MySqlConnection(_connectionString);
			await connection.OpenAsync();

			var cmd = new MySqlCommand("SHOW DATABASES;", connection);
			using var reader = await cmd.ExecuteReaderAsync();
			while (await reader.ReadAsync())
			{
				result.Add(reader.GetString(0));
			}

			return result;
		}

		public async Task<List<string>> GetTablesAsync()
		{
			var result = new List<string>();
			using var connection = new MySqlConnection(_connectionString);
			await connection.OpenAsync();

			var cmd = new MySqlCommand("SHOW TABLES;", connection);
			using var reader = await cmd.ExecuteReaderAsync();
			while (await reader.ReadAsync())
			{
				result.Add(reader.GetString(0));
			}

			return result;
		}
	}
}