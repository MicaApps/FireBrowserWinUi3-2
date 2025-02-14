using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI;
using Fire.Browser.Core;
using Fire.Core.Exceptions;
using Fire.Core.Licensing;
using Fire.Core.Models;
using FireBrowserWinUi3.Pages.Patch;
using FireBrowserWinUi3.Services;
using FireBrowserWinUi3.Services.Messages;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace FireBrowserWinUi3.Pages.SettingsPages
{
	public sealed partial class SettingsHome : Page
	{
		private SettingsService SettingsService { get; set; }
		public static SettingsHome Instance { get; set; }
		private readonly AddonManager _addonManager;
		private readonly DispatcherQueue _dispatcherQueue;
		private IMessenger Messenger { get; set; }
		public bool IsPremium { get; set; }

		public SettingsHome()
		{
			SettingsService = App.GetService<SettingsService>();
			Messenger = App.GetService<IMessenger>();
			InitializeComponent();
			Instance = this;
			LoadUserDataAndSettings();
			_ = LoadUsernames();
			_addonManager = new AddonManager();
			_dispatcherQueue = DispatcherQueue.GetForCurrentThread();
			IsPremium = false;
			Version.Text = "App version: " + GetVersionDescription();

			_addonManager.Initialize(App.Current.m_window);
		}

		private string GetVersionDescription()
		{
			string appName = "AppDisplayName".GetLocalized();
			Package package = Package.Current;
			PackageId packageId = package.Id;
			PackageVersion version = packageId.Version;

			return $"{appName} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
		}

		public Task LoadUsernames()
		{
			List<string> usernames = AuthService.GetAllUsernames();
			string currentUsername = AuthService.CurrentUser?.Username;

			if (currentUsername != null && currentUsername.Contains("Private"))
			{
				UserListView.IsEnabled = false;
				Add.IsEnabled = false;
			}
			else
			{
				UserListView.IsEnabled = true;
				Add.IsEnabled = true;
				UserListView.ItemsSource = usernames.Where(username => username != currentUsername && !username.Contains("Private")).ToList();
			}
			return Task.CompletedTask;
		}

		private Fire.Browser.Core.User GetUser()
		{
			return AuthService.IsUserAuthenticated ? AuthService.CurrentUser : null;
		}

		private void LoadUserDataAndSettings()
		{
			User.Text = GetUser()?.Username ?? "DefaultUser";
		}

		private async void Add_Click(object sender, RoutedEventArgs e)
		{
			AppService.IsAppNewUser = string.IsNullOrEmpty(AuthService.NewCreatedUser?.Username);
			Window window = new AddUserWindow();
			await AppService.ConfigureSettingsWindow(window);
		}

		public static async void OpenNewWindow(Uri uri)
		{
			_ = await Windows.System.Launcher.LaunchUriAsync(uri);
		}

		private async void Switch_Click(object sender, RoutedEventArgs e)
		{
			if (sender is Button switchButton && switchButton.DataContext is string clickedUserName)
			{
				OpenNewWindow(new Uri($"firebrowseruser://{clickedUserName}"));
				Shortcut ct = new();
				await ct.CreateShortcut(clickedUserName);
			}
		}

		private async void Delete_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (sender is Button switchButton && switchButton.DataContext is string clickedUserName)
				{
					UserDataManager.DeleteUser(clickedUserName);
					UserListView.ItemsSource = null;
					await LoadUsernames();
					_ = (Messenger?.Send(new Message_Settings_Actions($"User: {clickedUserName} has been removed from FireBrowser", EnumMessageStatus.Removed)));
				}
			}
			catch (Exception ex)
			{
				ExceptionLogger.LogException(ex);
				_ = (Messenger?.Send(new Message_Settings_Actions("You may not remove a User that has an Active Session!", EnumMessageStatus.XorError)));
			}
		}

		private void UpdateApp()
		{
			try
			{
				ProcessStartInfo startInfo = new()
				{
					FileName = "winget",
					Arguments = "upgrade --name \"FireBrowserWinUi\"",
					RedirectStandardOutput = true,
					UseShellExecute = false,
					CreateNoWindow = true
				};

				using Process process = Process.Start(startInfo);
				using StreamReader reader = process.StandardOutput;
				string result = reader.ReadToEnd();
				string msg = Regex.Replace(result, @"[^a-zA-Z0-9\s]+", "");
				string msg2 = Regex.Replace(msg, @"[\r*\-\\]", "");
				_ = (Messenger?.Send(new Message_Settings_Actions($"Application update status\n\n{msg2.Trim()} !", EnumMessageStatus.Informational)));
			}
			catch (Exception ex)
			{
				ExceptionLogger.LogException(ex);
				_ = (Messenger?.Send(new Message_Settings_Actions("Application update failed !", EnumMessageStatus.XorError)));
			}
		}

		private void PatchBtn_Click(object sender, RoutedEventArgs e)
		{
			if (sender is Button button)
			{
				button.Visibility = Visibility.Collapsed;
				button.Opacity = .5;
				UpdateApp();
				button.Visibility = Visibility.Visible;
				button.Opacity = 1;
			}
		}

		private async void Reset_Click(object sender, RoutedEventArgs e)
		{
			SureReset dlg = new();
			dlg.XamlRoot = XamlRoot;
			_ = await dlg.ShowAsync();
		}

		private async void BackUpNow_Click(object sender, RoutedEventArgs e)
		{
			BackUpDialog dlg = new();
			dlg.XamlRoot = XamlRoot;
			_ = await dlg.ShowAsync();
		}

		private async void RestoreNow_Click(object sender, RoutedEventArgs e)
		{
			RestoreBackupDialog dlg = new();
			dlg.XamlRoot = XamlRoot;
			_ = await dlg.ShowAsync();
		}

		private void LogOutBtn_Click(object sender, RoutedEventArgs e)
		{
			AuthService.Logout();
			_ = Microsoft.Windows.AppLifecycle.AppInstance.Restart("");
		}

		private async void ImportBookmarksItem_Click(object sender, RoutedEventArgs e)
		{
			Fire.Data.Favorites.ImportBookMarks dialog = new();
			dialog.XamlRoot = XamlRoot;
			_ = await dialog.ShowAsync();
		}

		private async void Prm_Click(object sender, RoutedEventArgs e)
		{
			if (sender is Button button)
			{
				button.IsEnabled = false;
			}

			try
			{
				ContentDialog processingDialog = new()
				{
					Title = "Processing Purchase",
					Content = "Please complete the purchase in the Store window. This dialog will close automatically once the purchase is complete.",
					CloseButtonText = "Cancel",
					XamlRoot = XamlRoot
				};

				Windows.Foundation.IAsyncOperation<ContentDialogResult> dialogTask = processingDialog.ShowAsync();
				Task<bool> purchaseTask = _addonManager.PurchaseAddonAsync();

				Task completedTask = await Task.WhenAny(dialogTask.AsTask(), purchaseTask);

				if (completedTask == dialogTask.AsTask())
				{
					// User cancelled the operation
					return;
				}

				processingDialog.Hide();

				if (await purchaseTask)
				{
					IsPremium = true;
					ContentDialog successDialog = new()
					{
						Title = "Premium Activated",
						Content = "Thank you for upgrading to Premium!",
						CloseButtonText = "OK",
						XamlRoot = XamlRoot
					};
					_ = await successDialog.ShowAsync();
				}
				else
				{
					ContentDialog failureDialog = new()
					{
						Title = "Purchase Incomplete",
						Content = "The premium upgrade was not completed. Please try again later.",
						CloseButtonText = "OK",
						XamlRoot = XamlRoot
					};
					_ = await failureDialog.ShowAsync();
				}
			}
			catch (Exception ex)
			{
				ContentDialog errorDialog = new()
				{
					Title = "Purchase Error",
					Content = $"An error occurred: {ex.Message}",
					CloseButtonText = "OK",
					XamlRoot = XamlRoot
				};
				_ = await errorDialog.ShowAsync();
			}
			finally
			{

			}
		}
	}
}