using CodeGenTool.Models;
using CodeGenTool.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace CodeGenTool
{
	public partial class MainWindow : Window
	{
		private MainViewModel ViewModel => (MainViewModel)DataContext;

		public MainWindow()
		{
			InitializeComponent();
			DataContext = new MainViewModel();
			InitializeTemplateGroups();
			ThemeToggle.IsChecked = ViewModel.IsDarkTheme;
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

		private void Database_CheckBox_Checked(object sender, RoutedEventArgs e)
		{
			var checkBox = sender as CheckBox;
			var database = checkBox.DataContext as DatabaseInfo;

			foreach (var table in database.Tables)
			{
				table.IsSelected = true;
			}
		}

		private void Database_CheckBox_Unchecked(object sender, RoutedEventArgs e)
		{
			var checkBox = sender as CheckBox;
			var database = checkBox.DataContext as DatabaseInfo;

			foreach (var table in database.Tables)
			{
				table.IsSelected = false;
			}
		}

		private void Table_CheckBox_Checked(object sender, RoutedEventArgs e)
		{
			var checkBox = sender as CheckBox;
			var table = checkBox.DataContext as TableInfo;

			bool allTablesSelected = true;
			foreach (var siblingTable in table.ParentDatabase.Tables)
			{
				if (!siblingTable.IsSelected)
				{
					allTablesSelected = false;
					break;
				}
			}

			if (allTablesSelected)
			{
				table.ParentDatabase.IsSelected = true;
			}
		}

		private void Table_CheckBox_Unchecked(object sender, RoutedEventArgs e)
		{
			var checkBox = sender as CheckBox;
			var table = checkBox.DataContext as TableInfo;

			table.ParentDatabase.IsSelected = false;
		}

		private void Database_Button_Click(object sender, RoutedEventArgs e)
		{
			Button button = (Button)sender;
			DatabaseInfo database = (DatabaseInfo)button.DataContext;

			database.IsSelected = !database.IsSelected;

			foreach (var table in database.Tables)
			{
				table.IsSelected = database.IsSelected;
			}

			e.Handled = true;
		}

		private void Table_Button_Click(object sender, RoutedEventArgs e)
		{
			Button button = (Button)sender;
			TableInfo table = (TableInfo)button.DataContext;

			table.IsSelected = !table.IsSelected;

			bool allTablesSelected = true;
			foreach (var siblingTable in table.ParentDatabase.Tables)
			{
				if (!siblingTable.IsSelected)
				{
					allTablesSelected = false;
					break;
				}
			}

			table.ParentDatabase.IsSelected = allTablesSelected;

			e.Handled = true;
		}
	}
}