using System.ComponentModel;

namespace CodeGenTool.Models
{
	public class ColumnInfo : INotifyPropertyChanged
	{
		public TableInfo ParentTable { get; set; }

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

		public string DataType { get; set; }
		public bool IsPrimaryKey { get; set; }
		public bool IsRequired { get; set; }
		public int MaxLength { get; set; }
		public bool IsNullable { get; set; }

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

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}