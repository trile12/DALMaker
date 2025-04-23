﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CodeGenTool.Converters
{
	public class BoolToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (value is true) ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (value is Visibility visibility && visibility == Visibility.Visible);
		}
	}
}