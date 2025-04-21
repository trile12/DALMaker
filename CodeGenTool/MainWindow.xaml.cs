using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CodeGenTool.Models;
using CodeGenTool.ViewModels;
using MaterialDesignThemes.Wpf;
using MySql.Data.MySqlClient;

namespace CodeGenTool
{
	public partial class MainWindow : Window
	{
		MainViewModel ViewModel => (MainViewModel)DataContext;
		public MainWindow()
		{
			InitializeComponent();
			DataContext = new MainViewModel();
			InitializeTemplateGroups();
		}

		private void InitializeTemplateGroups()
		{
			ViewModel.TemplateGroups.Clear();

			// SQL Templates
			var mysqlGroup = new TemplateGroup("MySQL SP");
			mysqlGroup.Templates.Add(new TemplateInfo { Name = "CRUD Operations", FileName = "mysql_crud.template", IsSelected = false });
			mysqlGroup.Templates.Add(new TemplateInfo { Name = "Basic Select", FileName = "mysql_select.template", IsSelected = false });

			var mariaGroup = new TemplateGroup("MariaDB SP");
			mariaGroup.Templates.Add(new TemplateInfo { Name = "CRUD Operations", FileName = "mariadb_crud.template", IsSelected = false });

			var postgresGroup = new TemplateGroup("PostgreSQL SP");
			postgresGroup.Templates.Add(new TemplateInfo { Name = "CRUD Operations", FileName = "postgres_crud.template", IsSelected = false });

			var msqlGroup = new TemplateGroup("Microsoft SQL SP");
			msqlGroup.Templates.Add(new TemplateInfo { Name = "CRUD Operations", FileName = "mssql_crud.template", IsSelected = false });

			// C# Templates
			var modelGroup = new TemplateGroup("C# Models/DTOs");
			modelGroup.Templates.Add(new TemplateInfo { Name = "Entity Model", FileName = "csharp_model.template", IsSelected = false });
			modelGroup.Templates.Add(new TemplateInfo { Name = "DTO", FileName = "csharp_dto.template", IsSelected = false });

			var webApiGroup = new TemplateGroup("C# Web API");
			webApiGroup.Templates.Add(new TemplateInfo { Name = "Controller", FileName = "csharp_controller.template", IsSelected = false });
			webApiGroup.Templates.Add(new TemplateInfo { Name = "Service", FileName = "csharp_service.template", IsSelected = false });
			webApiGroup.Templates.Add(new TemplateInfo { Name = "Repository", FileName = "csharp_repository.template", IsSelected = false });

			ViewModel.TemplateGroups.Add(mysqlGroup);
			ViewModel.TemplateGroups.Add(mariaGroup);
			ViewModel.TemplateGroups.Add(postgresGroup);
			ViewModel.TemplateGroups.Add(msqlGroup);
			ViewModel.TemplateGroups.Add(modelGroup);
			ViewModel.TemplateGroups.Add(webApiGroup);
		}

		private void ConnectBtn_Click(object sender, RoutedEventArgs e)
		{
			ViewModel.IsLoading = true;
			ViewModel.StatusMessage = "Connecting to database server...";

			try
			{
				// Build connection string without database
				string connectionString = $"Server={ViewModel.Server};User ID={ViewModel.User};Password={ViewModel.Password};";

				using (var connection = new MySqlConnection(connectionString))
				{
					connection.Open();

					// Get list of databases
					ViewModel.Databases.Clear();
					using (var cmd = new MySqlCommand("SHOW DATABASES", connection))
					{
						using (var reader = cmd.ExecuteReader())
						{
							while (reader.Read())
							{
								string dbName = reader.GetString(0);
								// Skip system databases
								if (dbName != "information_schema" && dbName != "mysql" &&
									dbName != "performance_schema" && dbName != "sys")
								{
									ViewModel.Databases.Add(dbName);
								}
							}
						}
					}

					ViewModel.StatusMessage = $"Connected successfully. Found {ViewModel.Databases.Count} databases.";
				}
			}
			catch (Exception ex)
			{
				ViewModel.StatusMessage = $"Connection failed: {ex.Message}";
				MessageBox.Show($"Failed to connect to the database server.\n\n{ex.Message}",
								"Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				ViewModel.IsLoading = false;
			}
		}

		private void GenerateBtn_Click(object sender, RoutedEventArgs e)
		{
			// Check if at least one table is selected
			var selectedTables = ViewModel.Tables.Where(t => t.IsSelected).ToList();
			if (selectedTables.Count == 0)
			{
				MessageBox.Show("Please select at least one table.", "Selection Required",
								MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			// Check if at least one template is selected
			bool hasSelectedTemplate = false;
			foreach (var group in ViewModel.TemplateGroups)
			{
				if (group.Templates.Any(t => t.IsSelected))
				{
					hasSelectedTemplate = true;
					break;
				}
			}

			if (!hasSelectedTemplate)
			{
				MessageBox.Show("Please select at least one template.", "Selection Required",
								MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			// Create output directory if it doesn't exist
			if (!Directory.Exists(ViewModel.OutputPath))
			{
				try
				{
					Directory.CreateDirectory(ViewModel.OutputPath);
				}
				catch (Exception ex)
				{
					MessageBox.Show($"Failed to create output directory.\n\n{ex.Message}",
									"Error", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}
			}

			ViewModel.IsLoading = true;
			ViewModel.StatusMessage = "Generating code...";

			try
			{
				int filesGenerated = 0;

				// In a real implementation, this would process templates and generate code
				// For demonstration, we'll just create placeholder files
				foreach (var table in selectedTables)
				{
					foreach (var group in ViewModel.TemplateGroups)
					{
						foreach (var template in group.Templates)
						{
							if (template.IsSelected)
							{
								// Generate filename based on template and table
								string extension = template.FileName.EndsWith(".sql.template") ? ".sql" : ".cs";
								string fileName = $"{table.Name}_{template.Name.Replace(" ", "_")}{extension}";
								string filePath = Path.Combine(ViewModel.OutputPath, fileName);

								// In a real implementation, you'd process the template here
								// For now, we'll just create an empty file
								File.WriteAllText(filePath, $"// Generated code for {table.Name} using {template.Name} template\n");

								filesGenerated++;
							}
						}
					}
				}

				ViewModel.StatusMessage = $"Generated {filesGenerated} files.";
				MessageBox.Show($"Successfully generated {filesGenerated} files.\n\nOutput directory: {ViewModel.OutputPath}",
								"Generation Complete", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (Exception ex)
			{
				ViewModel.StatusMessage = $"Generation failed: {ex.Message}";
				MessageBox.Show($"Failed to generate code.\n\n{ex.Message}",
								"Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				ViewModel.IsLoading = false;
			}
		}

		private void BrowseFolderBtn_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new System.Windows.Forms.FolderBrowserDialog();
			dialog.Description = "Select Output Folder";
			dialog.ShowNewFolderButton = true;

			if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				ViewModel.OutputPath = dialog.SelectedPath;
			}
		}

	}
}