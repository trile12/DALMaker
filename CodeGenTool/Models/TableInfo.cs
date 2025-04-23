using System.Collections.ObjectModel;
using System.ComponentModel;

namespace CodeGenTool.Models
{
	public class TableInfo : INotifyPropertyChanged
	{
		private string _name;

		public string Name
		{
			get => _name;
			set
			{
				_name = value;
				OnPropertyChanged(nameof(Name));
			}
		}

		private bool _isSelected;

		public bool IsSelected
		{
			get => _isSelected;
			set
			{
				_isSelected = value;
				OnPropertyChanged(nameof(IsSelected));
			}
		}

		public string PrimaryKey { get; set; }
		public ObservableCollection<ColumnInfo> Columns { get; } = new ObservableCollection<ColumnInfo>();

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}