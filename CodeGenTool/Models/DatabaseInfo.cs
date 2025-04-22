using System.Collections.ObjectModel;
using System.ComponentModel;

namespace CodeGenTool.Models
{
	public class DatabaseInfo : INotifyPropertyChanged
	{
		public DatabaseInfo()
		{ }

		private bool _isSelected;
		public string Name { get; set; }
		public ObservableCollection<TableInfo> Tables { get; set; }

		public bool IsSelected
		{
			get { return _isSelected; }
			set
			{
				if (_isSelected != value)
				{
					_isSelected = value;
					OnPropertyChanged(nameof(IsSelected));
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}