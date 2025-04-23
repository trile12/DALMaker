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

			ViewModel.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == nameof(ViewModel.GeneratedCode))
				{
					// You can add code here to handle generated code
					// For example, showing it in a text editor
				}
			};
		}

		private void InitializeTemplateGroups()
		{
			ViewModel.TemplateGroups.Clear();

			var modelGroup = new TemplateGroup("C# Models/DTOs");
			modelGroup.Templates.Add(new TemplateInfo { Name = "Entity Model", FileName = "csharp_model.template", IsSelected = false });
			modelGroup.Templates.Add(new TemplateInfo { Name = "DTO", FileName = "csharp_dto.cs.template", IsSelected = true });

			var webApiGroup = new TemplateGroup("C# Web API");
			webApiGroup.Templates.Add(new TemplateInfo { Name = "Controller", FileName = "csharp_controller.template", IsSelected = false });
			webApiGroup.Templates.Add(new TemplateInfo { Name = "Service", FileName = "csharp_service.template", IsSelected = false });
			webApiGroup.Templates.Add(new TemplateInfo { Name = "Repository", FileName = "csharp_repository.template", IsSelected = false });

			ViewModel.TemplateGroups.Add(modelGroup);
			ViewModel.TemplateGroups.Add(webApiGroup);
		}

		private void DatabaseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count > 0)
			{
				string selectedDatabase = e.AddedItems[0].ToString();
				if (!string.IsNullOrEmpty(selectedDatabase))
				{
					ViewModel.Database = selectedDatabase;
				}
			}
		}

		private void Database_CheckBox_Checked(object sender, RoutedEventArgs e)
		{
			var checkBox = sender as CheckBox;
			var table = checkBox.DataContext as TableInfo;

			foreach (var column in table.Columns)
			{
				column.IsSelected = true;
			}
		}

		private void Database_CheckBox_Unchecked(object sender, RoutedEventArgs e)
		{
			var checkBox = sender as CheckBox;
			var table = checkBox.DataContext as TableInfo;

			foreach (var column in table.Columns)
			{
				column.IsSelected = false;
			}
		}

		private void Table_CheckBox_Checked(object sender, RoutedEventArgs e)
		{
			var checkBox = sender as CheckBox;
			var column = checkBox.DataContext as ColumnInfo;

			var table = column.ParentTable;

			bool allColumnsSelected = true;
			foreach (var col in table.Columns)
			{
				if (!col.IsSelected)
				{
					allColumnsSelected = false;
					break;
				}
			}

			if (allColumnsSelected)
			{
				table.IsSelected = true;
			}
		}

		private void Table_CheckBox_Unchecked(object sender, RoutedEventArgs e)
		{
			var checkBox = sender as CheckBox;
			var column = checkBox.DataContext as ColumnInfo;

			var table = column.ParentTable;
			table.IsSelected = false;
		}

		private void Database_Button_Click(object sender, RoutedEventArgs e)
		{
			Button button = (Button)sender;
			TableInfo table = (TableInfo)button.DataContext;
			table.IsSelected = !table.IsSelected;
			foreach (var column in table.Columns)
			{
				column.IsSelected = table.IsSelected;
			}
			e.Handled = true;
		}

		private void Column_Button_Click(object sender, RoutedEventArgs e)
		{
			Button button = (Button)sender;
			ColumnInfo column = (ColumnInfo)button.DataContext;

			column.IsSelected = !column.IsSelected;

			var table = column.ParentTable;

			bool allColumnsSelected = true;
			foreach (var col in table.Columns)
			{
				if (!col.IsSelected)
				{
					allColumnsSelected = false;
					break;
				}
			}

			table.IsSelected = allColumnsSelected;

			e.Handled = true;
		}
	}
}