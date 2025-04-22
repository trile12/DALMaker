using CodeGenTool.Models;
using CodeGenTool.Services;
using CommunityToolkit.Mvvm.Input;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
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

				using (var cmd = new MySqlCommand("SHOW DATABASES", connection))
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						string dbName = reader.GetString(0);
						if (dbName is not ("mysql" or "information_schema" or "performance_schema" or "sys"))
						{
							Databases.Add(new DatabaseInfo
							{
								Name = dbName,
								Tables = new ObservableCollection<Models.TableInfo>(),
								IsSelected = false
							});
						}
					}
				}

				int totalTables = 0;
				foreach (var db in Databases)
				{
					string dbConnStr = connectionString + $"Database={db.Name};";
					using var dbConn = new MySqlConnection(dbConnStr);
					dbConn.Open();

					using var tableCmd = new MySqlCommand("SHOW TABLES", dbConn);
					using var tblReader = tableCmd.ExecuteReader(CommandBehavior.CloseConnection);
					var tableList = new List<string>();

					while (tblReader.Read())
					{
						tableList.Add(tblReader.GetString(0));
					}

					foreach (var tableName in tableList)
					{
						var tableInfo = new Models.TableInfo
						{
							Name = tableName,
							ParentDatabase = db
						};

						string newDbString = connectionString + $"Database={db.Name};";
						using var newdbConn = new MySqlConnection(newDbString);
						newdbConn.Open();

						// Sử dụng một kết nối riêng biệt để lấy thông tin cột
						using var columnCmd = new MySqlCommand($"SHOW COLUMNS FROM `{tableName}`", newdbConn);
						using var colReader = columnCmd.ExecuteReader(CommandBehavior.CloseConnection); // Đảm bảo đóng kết nối khi đọc xong
						while (colReader.Read())
						{
							tableInfo.Columns.Add(new ColumnInfo
							{
								Name = colReader.GetString(0),
								DataType = colReader.GetString(1),
								IsNullable = colReader.GetString(2) == "YES",
								//Key = colReader.GetString(3)
							});
						}

						db.Tables.Add(tableInfo);
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
			var selectedTables = Databases.SelectMany(x => x.Tables).Where(t => t.IsSelected).ToList();
			if (!selectedTables.Any())
			{
				MessageBox.Show("Please select at least one table.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			//bool hasTemplate = TemplateGroups.Any(g => g.Templates.Any(t => t.IsSelected));
			//if (!hasTemplate)
			//{
			//	MessageBox.Show("Please select at least one template.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
			//	return;
			//}

			if (GenerateSqlFile && !Directory.Exists(OutputPath))
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
				string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "csharp_model.template");
				string templateContent = File.ReadAllText(templatePath);
				int filesGenerated = 0;
				string rendered = "";
				GeneratedCode = "";
				foreach (var table in selectedTables)
				{
					rendered = TemplateEngine.Render(templateContent, table);
					GeneratedCode += $"{rendered}\n";
				}
				StatusMessage = $"Generated {filesGenerated} files.";
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

		private string _password = "admin";

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

		private string _generatedCode;

		public string GeneratedCode
		{
			get => _generatedCode;
			set
			{
				_generatedCode = value;
				OnPropertyChanged(nameof(GeneratedCode));
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