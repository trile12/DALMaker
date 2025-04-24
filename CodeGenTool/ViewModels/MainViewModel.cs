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
				Tables.Clear();

				using (var cmd = new MySqlCommand("SHOW DATABASES", connection))
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						string dbName = reader.GetString(0);
						if (dbName is not ("mysql" or "information_schema" or "performance_schema" or "sys"))
						{
							Databases.Add(dbName);
						}
					}
				}

				StatusMessage = $"Connected successfully. Found {Databases.Count} databases.";
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

		private void LoadTablesForSelectedDatabase()
		{
			if (string.IsNullOrWhiteSpace(Database))
				return;

			IsLoading = true;
			StatusMessage = $"Loading tables for database {Database}...";
			Tables.Clear();

			try
			{
				string connectionString = $"Server={Server};User ID={User};Password={Password};Database={Database};";
				using var connection = new MySqlConnection(connectionString);
				connection.Open();

				using var tableCmd = new MySqlCommand("SHOW TABLES", connection);
				using var tblReader = tableCmd.ExecuteReader();

				var tableNames = new List<string>();
				while (tblReader.Read())
				{
					tableNames.Add(tblReader.GetString(0));
				}

				tblReader.Close();

				foreach (var tableName in tableNames)
				{
					var tableInfo = new TableInfo
					{
						Name = tableName
					};

					using var columnCmd = new MySqlCommand($"SHOW COLUMNS FROM `{tableName}`", connection);
					using var colReader = columnCmd.ExecuteReader();

					while (colReader.Read())
					{
						tableInfo.Columns.Add(new ColumnInfo
						{
							Name = colReader.GetString(0),
							DataType = colReader.GetString(1),
							IsNullable = colReader.GetString(2) == "YES",
							ParentTable = tableInfo,
						});
					}

					Tables.Add(tableInfo);
				}

				StatusMessage = $"Loaded {Tables.Count} tables from database {Database}.";
			}
			catch (Exception ex)
			{
				StatusMessage = $"Failed to load tables: {ex.Message}";
				MessageBox.Show($"Failed to load tables from database.\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
				var selectedTemplates = TemplateGroups
					.SelectMany(g => g.Templates)
					.Where(t => t.IsSelected)
					.ToList();

				int filesGenerated = 0;
				GeneratedCode = "";

				foreach (var template in selectedTemplates)
				{
					string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", template.FileName);
					if (!File.Exists(templatePath))
					{
						StatusMessage = $"Template file not found: {template.Name}";
						continue;
					}

					string templateContent = File.ReadAllText(templatePath);

					foreach (var table in selectedTables)
					{
						var input = new TemplateInput
						{
							Namespace = !string.IsNullOrEmpty(Namespace)? Namespace :"{{namespace}}",
							TemplateFileName = template.Name,
							TableInfo = table
						};

						var (rendered, className) = TemplateEngine.Render(templateContent, input);
						GeneratedCode += $"{rendered}\n";

						string fileName = $"{className}.cs";
						string filePath = Path.Combine(OutputPath, fileName);

						File.WriteAllText(filePath, rendered);
						filesGenerated++;
					}
				}

				if (filesGenerated > 0)
				{
					string consolidatedFilePath = Path.Combine(OutputPath, "AllGeneratedCode.cs");
					File.WriteAllText(consolidatedFilePath, GeneratedCode);
					filesGenerated++;
				}

				StatusMessage = $"Generated {filesGenerated} files.";

				//System.Diagnostics.Process.Start("explorer.exe", OutputPath);
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
				if (_database != value)
				{
					_database = value;
					OnPropertyChanged(nameof(Database));

					if (!string.IsNullOrWhiteSpace(_database))
					{
						LoadTablesForSelectedDatabase();
					}
				}
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

		private string _namespace = "DAL";

		public string Namespace
		{
			get => _namespace;
			set
			{
				_namespace = value;
				OnPropertyChanged(nameof(_namespace));
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