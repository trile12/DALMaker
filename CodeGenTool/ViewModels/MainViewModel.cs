using CodeGenTool.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenTool.ViewModels
{
	public class MainViewModel : INotifyPropertyChanged
	{
        public MainViewModel()
        {
			OutputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DALMaker");
		}

        private void LoadTables()
		{
			if (string.IsNullOrEmpty(Database))
				return;

			IsLoading = true;
			StatusMessage = $"Loading tables from {Database}...";

			try
			{
				string connectionString = $"Server={Server};User ID={User};Password={Password};Database={Database}";

				using (var connection = new MySqlConnection(connectionString))
				{
					connection.Open();

					// Clear existing tables
					Tables.Clear();

					// Get tables from selected database
					using (var cmd = new MySqlCommand("SHOW TABLES", connection))
					{
						using (var reader = cmd.ExecuteReader())
						{
							while (reader.Read())
							{
								string tableName = reader.GetString(0);
								Tables.Add(new TableInfo { Name = tableName });
							}
						}
					}

					// Get columns for each table
					foreach (var table in Tables)
					{
						using (var cmd = new MySqlCommand($"DESCRIBE `{table.Name}`", connection))
						{
							using (var reader = cmd.ExecuteReader())
							{
								while (reader.Read())
								{
									string fieldName = reader.GetString("Field");
									string dataType = reader.GetString("Type");
									bool isPrimaryKey = reader.GetString("Key") == "PRI";
									bool isNullable = reader.GetString("Null") == "YES";

									table.Columns.Add(new ColumnInfo
									{
										Name = fieldName,
										DataType = dataType,
										IsPrimaryKey = isPrimaryKey,
										IsNullable = isNullable
									});

									if (isPrimaryKey)
										table.PrimaryKey = fieldName;
								}
							}
						}
					}

					StatusMessage = $"Loaded {Tables.Count} tables from {Database}.";
				}
			}
			catch (Exception ex)
			{
				StatusMessage = $"Failed to load tables: {ex.Message}";
				//MessageBox.Show($"Failed to load tables from database.\n\n{ex.Message}",
				//				"Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				IsLoading = false;
			}
		}

		#region Properties
		private string _outputPath;
		public string OutputPath
		{
			get => _outputPath;
			set
			{
				_outputPath = value;
				OnPropertyChanged(nameof(OutputPath));
			}
		}

		private string _server = "localhost";
		public string Server
		{
			get => _server;
			set
			{
				_server = value;
				OnPropertyChanged(nameof(Server));
			}
		}

		private string _user = "root";
		public string User
		{
			get => _user;
			set
			{
				_user = value;
				OnPropertyChanged(nameof(User));
			}
		}

		private string _password;
		public string Password
		{
			get => _password;
			set
			{
				_password = value;
				OnPropertyChanged(nameof(Password));
			}
		}

		private string _database;
		public string Database
		{
			get => _database;
			set
			{
				_database = value;
				OnPropertyChanged(nameof(Database));
				LoadTables();
			}
		}

		private string _namespace = "DAL";
		public string Namespace
		{
			get => _namespace;
			set
			{
				_namespace = value;
				OnPropertyChanged(nameof(Namespace));
			}
		}

		private ObservableCollection<string> _databases = new ObservableCollection<string>();
		public ObservableCollection<string> Databases
		{
			get => _databases;
			set
			{
				_databases = value;
				OnPropertyChanged(nameof(Databases));
			}
		}

		private ObservableCollection<TableInfo> _tables = new ObservableCollection<TableInfo>();
		public ObservableCollection<TableInfo> Tables
		{
			get => _tables;
			set
			{
				_tables = value;
				OnPropertyChanged(nameof(Tables));
			}
		}

		private ObservableCollection<TemplateGroup> _templateGroups = new ObservableCollection<TemplateGroup>();
		public ObservableCollection<TemplateGroup> TemplateGroups
		{
			get => _templateGroups;
			set
			{
				_templateGroups = value;
				OnPropertyChanged(nameof(TemplateGroups));
			}
		}

		private bool _generateSqlFile = true;
		public bool GenerateSqlFile
		{
			get => _generateSqlFile;
			set
			{
				_generateSqlFile = value;
				OnPropertyChanged(nameof(GenerateSqlFile));
			}
		}

		private string _statusMessage;
		public string StatusMessage
		{
			get => _statusMessage;
			set
			{
				_statusMessage = value;
				OnPropertyChanged(nameof(StatusMessage));
			}
		}

		private bool _isLoading;
		public bool IsLoading
		{
			get => _isLoading;
			set
			{
				_isLoading = value;
				OnPropertyChanged(nameof(IsLoading));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		#endregion
	}
}
