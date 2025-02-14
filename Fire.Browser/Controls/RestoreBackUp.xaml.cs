using Fire.Browser.Core;
using Fire.Core.Helpers;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Graphics;
using WinRT.Interop;

namespace FireBrowserWinUi3.Controls;
public sealed partial class RestoreBackUp : Window
{

	private AppWindow appWindow;
	private AppWindowTitleBar titleBar;

	public RestoreBackUp()
	{
		InitializeComponent();
		InitializeWindow();
		_ = StartRestoreProcess().ConfigureAwait(false);
	}


	private async Task StartRestoreProcess()
	{
		try
		{
			string tempPath = Path.GetTempPath();
			string restoreFilePath = Path.Combine(tempPath, "restore.fireback");

			if (File.Exists(restoreFilePath))
			{
				string backupFilePath = await File.ReadAllTextAsync(restoreFilePath);
				await RestoreBackup(backupFilePath);
			}
			else
			{
				await ShowErrorMessage("Restore file not found.");
			}
		}
		catch (Exception ex)
		{
			await ShowErrorMessage($"An error occurred: {ex.Message}");
		}
	}

	private async Task RestoreBackup(string backupFilePath)
	{
		await Task.Delay(100);
		StatusTextBlock.Text = "Restoring backup...";
		await Task.Delay(100);

		_ = await BackupManager.RestoreBackup();

		// Delete the restore file once done
		string tempPath = Path.GetTempPath();
		string restoreFilePath = Path.Combine(tempPath, "restore.fireback");
		if (File.Exists(restoreFilePath))
		{
			File.Delete(restoreFilePath);
		}

		// Finalize the process
		StatusTextBlock.Text = "Backup restored successfully!";
		await Task.Delay(100);

		_ = Microsoft.Windows.AppLifecycle.AppInstance.Restart(""); // Optionally restart the app if needed
	}



	private void InitializeWindow()
	{
		nint hWnd = WindowNative.GetWindowHandle(this);
		WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
		appWindow = AppWindow.GetFromWindowId(windowId);

		appWindow.MoveAndResize(new RectInt32(500, 500, 850, 500));
		Fire.Core.Helpers.Windowing.Center(this);
		appWindow.SetPresenter(AppWindowPresenterKind.CompactOverlay);
		appWindow.MoveInZOrderAtTop();
		appWindow.SetIcon("backup.ico");
		appWindow.ShowOnceWithRequestedStartupState();
		Windowing.Center(this);


		if (!AppWindowTitleBar.IsCustomizationSupported())
		{
			throw new Exception("Unsupported OS version.");
		}
		else
		{
			titleBar = appWindow.TitleBar;
			titleBar.ExtendsContentIntoTitleBar = true;
			Windows.UI.Color btnColor = Colors.Transparent;
			titleBar.BackgroundColor = btnColor;
			titleBar.ButtonBackgroundColor = btnColor;
			titleBar.InactiveBackgroundColor = btnColor;
			titleBar.ButtonInactiveBackgroundColor = btnColor;
		}
	}

	private async Task ShowErrorMessage(string message)
	{
		ContentDialog errorDialog = new()
		{
			Title = "Error",
			Content = message,
			CloseButtonText = "OK"
		};

		_ = await errorDialog.ShowAsync();
		Close();
	}
}