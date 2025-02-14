using Fire.Browser.Core;
using FireBrowserWinUi3.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FireBrowserWinUi3
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class SetupAccess : Page
	{
		public SetupAccess()
		{
			InitializeComponent();
			Langue.SelectedItem = "en-US";
			Langue.Text = "en-US";
		}

		private Fire.Browser.Core.User GetUser()
		{
			// Check if the user is authenticated.
			if (AuthService.IsUserAuthenticated)
			{
				// Return the authenticated user.
				return AuthService.CurrentUser;
			}

			// If no user is authenticated, return null or handle as needed.
			return null;
		}

		private void LiteMode_Toggled(object sender, RoutedEventArgs e)
		{
			if (sender is ToggleSwitch toggleSwitch)
			{

				// Assuming 'url' and 'selection' have been defined earlier
				bool autoSettingValue = toggleSwitch.IsOn;

				AppService.AppSettings.LightMode = autoSettingValue;

			}
		}

		private void Langue_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			string selection = e.AddedItems[0].ToString();
			string type = selection switch
			{
				"nl-NL" => "nl-NL",
				"en-US" => "en-US",
				// Add other cases for different search engines.
				_ => "en-US",// Handle the case when selection doesn't match any of the predefined options.
			};
			if (!string.IsNullOrEmpty(type))
			{
				AppService.AppSettings.Lang = type;
			}
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			_ = Frame.Navigate(typeof(SetupWebView));
		}
	}
}
