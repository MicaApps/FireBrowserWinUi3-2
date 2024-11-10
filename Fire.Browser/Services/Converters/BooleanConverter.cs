﻿using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace FireBrowserWinUi3.Services.Converters
{
	public class BooleanVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (value is bool boolValue)
			{
				return boolValue ? Visibility.Visible : Visibility.Collapsed;
			}
			return Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			if (value is Visibility visibility)
			{
				return visibility == Visibility.Visible;
			}
			return false;
		}
	}
}
