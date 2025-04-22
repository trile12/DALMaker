using System.Collections.ObjectModel;
using System.ComponentModel;

namespace CodeGenTool.Models
{
	public class TemplateGroup : INotifyPropertyChanged
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

		private bool _isExpanded;

		public bool IsExpanded
		{
			get => _isExpanded;
			set
			{
				_isExpanded = value;
				OnPropertyChanged(nameof(IsExpanded));
			}
		}

		public ObservableCollection<TemplateInfo> Templates { get; } = new ObservableCollection<TemplateInfo>();

		public TemplateGroup(string name)
		{
			Name = name;
			IsExpanded = false;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}