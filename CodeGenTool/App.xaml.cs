using CodeGenTool.Services;
using System.Windows;

namespace CodeGenTool
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);
			ThemeManager.LoadSavedTheme();
		}
	}
}