using Fire.Browser.Core;
using Fire.Core.Exceptions;
using Fire.Data.Core.Actions;
using FireBrowserWinUi3.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;

namespace FireBrowserWinUi3.Controls;
public sealed partial class DownloadFlyout : Flyout
{
	public DownloadService DownloadService { get; set; }
	public DownloadFlyout()
	{
		DownloadService = App.GetService<DownloadService>();
		// Let service tell page when changes happen always stay in sync with all download compontents. a. replace list everytime -> flyouts aren't derived from a UIElement, hence to x:Bind..
		DownloadService.Handler_DownItemsChange += (_, _) => GetDownloadItems();

		InitializeComponent();

		GetDownloadItems();
	}

	public void TriggerFlyoutUpdate()
	{
		DownloadItemsListView.Items.Clear();
		GetDownloadItems();
	}

	public async void GetDownloadItems()
	{
		try
		{
			DownloadItemsListView.Items.Clear();
			DownloadActions downloadActions = new(AuthService.CurrentUser.Username);
			List<Fire.Data.Core.Models.DownloadItem> items = await downloadActions.GetAllDownloadItems();

			if (items.Count > 0)
			{
				items.ForEach(t =>
				{
					DownloadItem downloadItem = new(t.current_path);
					downloadItem.ServiceDownloads = DownloadService;
					DownloadItemsListView.Items.Insert(0, downloadItem);
				});
			};

		}
		catch (Exception ex)
		{
			// Handle any exceptions, such as file access or database errors
			ExceptionLogger.LogException(ex);
			Console.WriteLine($"Error accessing database: {ex.Message}");
		}
	}

	private void ShowDownloads_Click(object sender, RoutedEventArgs e)
	{
		string downloadPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
		string pathToExecutable = @"C:\Windows\explorer.exe";
		string arguments = string.Empty;

		_ = ProcessStarter.StartProcess(pathToExecutable, arguments, $"{downloadPath}");
	}

	private void OpenDownloadsItem_Click(object sender, RoutedEventArgs e)
	{
		MainWindow window = (Application.Current as App)?.m_window as MainWindow;
		window.UrlBox.Text = "firebrowser://downloads";
		_ = window.TabContent.Navigate(typeof(FireBrowserWinUi3.Pages.TimeLinePages.MainTimeLine));
	}
}
