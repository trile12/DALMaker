using System;
using System.Windows;

namespace CodeGenTool.Services
{
	public enum ThemeType
	{
		Light,
		Dark
	}

	public static class ThemeManager
	{
		private static ResourceDictionary _lightTheme;
		private static ResourceDictionary _darkTheme;

		public static ThemeType CurrentTheme { get; private set; } = ThemeType.Light;

		static ThemeManager()
		{
			_lightTheme = new ResourceDictionary { Source = new Uri("pack://application:,,,/CodeGenTool;component/Themes/LightTheme.xaml") };
			_darkTheme = new ResourceDictionary { Source = new Uri("pack://application:,,,/CodeGenTool;component/Themes/DarkTheme.xaml") };
		}

		public static void ChangeTheme(ThemeType theme)
		{
			var appDictionaries = Application.Current.Resources.MergedDictionaries;

			if (appDictionaries.Contains(_lightTheme))
				appDictionaries.Remove(_lightTheme);

			if (appDictionaries.Contains(_darkTheme))
				appDictionaries.Remove(_darkTheme);

			if (theme == ThemeType.Dark)
				appDictionaries.Add(_darkTheme);
			else
				appDictionaries.Add(_lightTheme);

			CurrentTheme = theme;

			Properties.Settings.Default.Theme = theme.ToString();
			Properties.Settings.Default.Save();
		}

		public static void ToggleTheme()
		{
			if (CurrentTheme == ThemeType.Light)
				ChangeTheme(ThemeType.Dark);
			else
				ChangeTheme(ThemeType.Light);
		}

		public static void LoadSavedTheme()
		{
			try
			{
				if (Enum.TryParse(Properties.Settings.Default.Theme, out ThemeType savedTheme))
				{
					ChangeTheme(savedTheme);
				}
			}
			catch
			{
				ChangeTheme(ThemeType.Light);
			}
		}
	}
}