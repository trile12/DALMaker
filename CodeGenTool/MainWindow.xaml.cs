using CodeGenTool.Models;
using CodeGenTool.ViewModels;
using System.IO;
using System;
using System.Linq;
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

			string templatesRootPath = Path.Combine(AppContext.BaseDirectory, "Templates");
			if (!Directory.Exists(templatesRootPath)) return;

			var groupDirs = Directory.GetDirectories(templatesRootPath);

			foreach (var groupDir in groupDirs)
			{
				string groupName = Path.GetFileName(groupDir);
				var group = new TemplateGroup(groupName);

				var templateFiles = Directory.GetFiles(groupDir, "*.template");
				foreach (var file in templateFiles)
				{
					string fileName = Path.GetFileName(file); 
					string displayName = fileName.Replace(".template", ""); 

					group.Templates.Add(new TemplateInfo
					{
						Name = displayName,
						FileName = Path.Combine(groupName, fileName), 
						IsSelected = false
					});
				}

				if (group.Templates.Count > 0)
				{
					group.Templates[0].IsSelected = true;
				}

				ViewModel.TemplateGroups.Add(group);
			}
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

			if (isCheckFromColumn)
			{
				isCheckFromColumn = false;
				return;
			}

			foreach (var column in table.Columns)
			{
				column.IsSelected = true;
			}
		}

		bool isCheckFromColumn = false;
		private void Column_CheckBox_Checked(object sender, RoutedEventArgs e)
		{
			var checkBox = sender as CheckBox;
			var column = checkBox.DataContext as ColumnInfo;
			var table = column.ParentTable;

			if (!table.IsSelected)
			{
				isCheckFromColumn = true;
				table.IsSelected = true;
			}
		}

		private void Column_CheckBox_Unchecked(object sender, RoutedEventArgs e)
		{
			var checkBox = sender as CheckBox;
			var column = checkBox.DataContext as ColumnInfo;
			var table = column.ParentTable;

			if (!table.Columns.Any(c => c.IsSelected))
			{
				table.IsSelected = false;
			}
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

			e.Handled = true;
		}
	}
}