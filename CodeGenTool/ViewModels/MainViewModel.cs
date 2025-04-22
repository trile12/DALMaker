using CodeGenTool.Models;
using CodeGenTool.Services;
using CommunityToolkit.Mvvm.Input;
using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace CodeGenTool.ViewModels
{
	public class MainViewModel : INotifyPropertyChanged
	{
		public MainViewModel()
		{
			IsDarkTheme = ThemeManager.CurrentTheme == ThemeType.Dark;
			OutputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DALMaker");

			ConnectCommand = new RelayCommand(ExecuteConnect);
			GenerateCommand = new RelayCommand(ExecuteGenerate);
			BrowseOutputCommand = new RelayCommand(ExecuteBrowseOutput);
			ToggleThemeCommand = new RelayCommand<object>(ExecuteToggleTheme);
		}

		#region Methods

		private void ExecuteConnect()
		{
			IsLoading = true;
			StatusMessage = "Connecting to database server...";

			try
			{
				string connectionString = $"Server={Server};User ID={User};Password={Password};";
				using var connection = new MySqlConnection(connectionString);
				connection.Open();
				Databases.Clear();

				using var cmd = new MySqlCommand("SHOW DATABASES", connection);
				using var reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					string dbName = reader.GetString(0);
					if (dbName is not ("mysql" or "information_schema" or "performance_schema" or "sys"))
					{
						Databases.Add(new DatabaseInfo
						{
							Name = dbName,
							Tables = new ObservableCollection<TableInfo>(),
							IsSelected = false
						});
					}
				}

				int totalTables = 0;
				foreach (var db in Databases)
				{
					string dbConnStr = connectionString + $"Database={db.Name};";
					using var dbConn = new MySqlConnection(dbConnStr);
					dbConn.Open();
					using var tableCmd = new MySqlCommand("SHOW TABLES", dbConn);
					using var tblReader = tableCmd.ExecuteReader();
					while (tblReader.Read())
					{
						db.Tables.Add(new TableInfo
						{
							Name = tblReader.GetString(0),
							ParentDatabase = db
						});
						totalTables++;
					}
				}

				StatusMessage = $"Connected successfully. Found {Databases.Count} databases and {totalTables} tables.";
			}
			catch (Exception ex)
			{
				StatusMessage = $"Connection failed: {ex.Message}";
				MessageBox.Show($"Failed to connect to database.\n\n{ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				IsLoading = false;
			}
		}

		private void ExecuteGenerate()
		{
			var selectedTables = Tables.Where(t => t.IsSelected).ToList();
			if (!selectedTables.Any())
			{
				MessageBox.Show("Please select at least one table.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			bool hasTemplate = TemplateGroups.Any(g => g.Templates.Any(t => t.IsSelected));
			if (!hasTemplate)
			{
				MessageBox.Show("Please select at least one template.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			if (!Directory.Exists(OutputPath))
			{
				try
				{
					Directory.CreateDirectory(OutputPath);
				}
				catch (Exception ex)
				{
					MessageBox.Show($"Failed to create output directory.\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}
			}

			IsLoading = true;
			StatusMessage = "Generating code...";

			try
			{
				int filesGenerated = 0;
				foreach (var table in selectedTables)
				{
					foreach (var group in TemplateGroups)
					{
						foreach (var template in group.Templates)
						{
							if (!template.IsSelected) continue;
							string ext = template.FileName.EndsWith(".sql.template") ? ".sql" : ".cs";
							string fileName = $"{table.Name}_{template.Name.Replace(" ", "_")}{ext}";
							string path = Path.Combine(OutputPath, fileName);
							File.WriteAllText(path, $"// Generated code for {table.Name} using {template.Name} template\n");
							filesGenerated++;
						}
					}
				}

				StatusMessage = $"Generated {filesGenerated} files.";
				MessageBox.Show($"Successfully generated {filesGenerated} files.\n\nOutput: {OutputPath}", "Done", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (Exception ex)
			{
				StatusMessage = $"Generation failed: {ex.Message}";
				MessageBox.Show($"Failed to generate code.\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				IsLoading = false;
			}
		}

		private void ExecuteBrowseOutput()
		{
			var dialog = new System.Windows.Forms.FolderBrowserDialog();
			dialog.Description = "Select Output Folder";
			dialog.ShowNewFolderButton = true;
			if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				OutputPath = dialog.SelectedPath;
			}
		}

		private void ExecuteToggleTheme(object value)
		{
			IsDarkTheme = (bool)value;
			ThemeManager.ChangeTheme(IsDarkTheme ? ThemeType.Dark : ThemeType.Light);
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

					Tables.Clear();

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

		#endregion Methods

		#region Commands

		public ICommand ConnectCommand { get; }
		public ICommand GenerateCommand { get; }
		public ICommand BrowseOutputCommand { get; }
		public ICommand ToggleThemeCommand { get; }

		#endregion Commands

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

		private ObservableCollection<DatabaseInfo> _databases = new ObservableCollection<DatabaseInfo>();

		public ObservableCollection<DatabaseInfo> Databases
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

		private bool _isDarkTheme;

		public bool IsDarkTheme
		{
			get => _isDarkTheme;
			set
			{
				_isDarkTheme = value;
				OnPropertyChanged(nameof(IsDarkTheme));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion Properties
	}
}