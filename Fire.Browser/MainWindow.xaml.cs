using CommunityToolkit.WinUI.Behaviors;
using Fire.Browser.Core;
using Fire.Browser.Core.Helper;
using Fire.Core.CoreUi;
using Fire.Core.Exceptions;
using Fire.Core.Helpers;
using Fire.Core.Models;
using Fire.Core.ShareHelper;
using Fire.Data.Core.Actions;
using Fire.Data.Favorites;
using FireBrowserDatabase;
using FireBrowserWinUi3.Controls;
using FireBrowserWinUi3.Pages;
using FireBrowserWinUi3.Services;
using FireBrowserWinUi3.Services.ViewModels;
using FireBrowserWinUi3.ViewModels;
using FireBrowserWinUi3QrCore;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Security.Credentials.UI;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.System;
using WinRT.Interop;
using Settings = Fire.Browser.Core.Settings;
using User = Fire.Browser.Core.User;
using Windowing = Fire.Core.Helpers.Windowing;

namespace FireBrowserWinUi3;

public sealed partial class MainWindow : Window
{

	private AppWindow appWindow;
	public DownloadFlyout DownloadFlyout { get; set; }
	public ProfileCommander Commander { get; set; }
	public DownloadService ServiceDownloads { get; set; }
	public SettingsService SettingsService { get; set; }
	public MainWindowViewModel ViewModelMain { get; set; }
	public string PicturePath { get; set; }

	public string BackDropType = "Base";
	public MainWindow()
	{
		appWindow = AppWindow;

		ServiceDownloads = App.GetService<DownloadService>();
		SettingsService = App.GetService<SettingsService>();
		SettingsService.Initialize();
		ViewModelMain = App.GetService<MainWindowViewModel>();
		ViewModelMain.IsActive = true;
		ViewModelMain.MainView = this;
		ViewModelMain.ProfileImage = new ImageHelper().LoadImage("profile_image.jpg");

		Commander = new ProfileCommander(ViewModelMain);

		InitializeComponent();

		ArgsPassed();
		LoadUserDataAndSettings(); // Load data and settings for the new user
		_ = LoadUserSettings();
		Init();
		DownloadFlyout = new DownloadFlyout();

		Closed += (s, e) =>
		{
			if (AuthService.CurrentUser.Username != "__Admin__" && AuthService.CurrentUser.Username != "Private")
				AppService.Admin_Delete_Account();
		};
		SizeChanged += async (s, e) =>
		{
			SemaphoreSlim semaphoreSlim = new(3);
			IntPtr hWnd = Windowing.GetForegroundWindow();
			await Task.Delay(100);

			if (hWnd != IntPtr.Zero)
			{
				await semaphoreSlim.WaitAsync();

				_ = Windowing.GetWindowRect(hWnd, out Windowing.RECT rect);

				Windows.Graphics.SizeInt32? sizeWindow = await Windowing.SizeWindow();
				// Get the monitor dimensions
				int screenWidth = sizeWindow.Value.Width;
				int screenHeight = sizeWindow.Value.Height;

				// Calculate the maximum allowed dimensions
				int maxWidth = screenWidth / 4;
				int maxHeight = screenHeight / 3;

				if (appWindow.Size.Width < maxWidth)
				{
					await Task.Delay(60);
					_ = Windowing.SetWindowPos(hWnd, IntPtr.Zero, rect.left, rect.top, maxWidth, appWindow.Size.Height, Windowing.SWP_NOZORDER | Windowing.SWP_SHOWWINDOW);
					Windowing.FlashWindow(hWnd);

				}
				if (appWindow.Size.Height < maxHeight)
				{
					await Task.Delay(60);
					_ = Windowing.SetWindowPos(hWnd, IntPtr.Zero, rect.left, rect.top, appWindow.Size.Width, maxHeight, Windowing.SWP_NOZORDER | Windowing.SWP_SHOWWINDOW);
					Windowing.FlashWindow(hWnd);
				}
				_ = semaphoreSlim.Release();
			}
			e.Handled = true;
		};

		appWindow.Closing += AppWindow_Closing;
	}

	public void LoadDependencies()
	{


	}
	public async void Init()
	{
		await Fire.Core.Models.Data.Init();
		_ = Directory.GetParent(Windows.ApplicationModel.Package.Current.InstalledLocation.Path).Parent.Parent.Parent.Parent.Parent.FullName;
		//string workerProjectName = "FireAuthService";
		//string workerPath = Path.Combine(solutionDir, workerProjectName, "bin", "Release", "net8.0", "publish", "FireAuthService.exe");
		//string nameService = nameof(FireAuthService).ToString();

	}


	public void setColorsTool()
	{
		if (SettingsService.CoreSettings.ColorTV is "#000000" or "#FF000000")
		{
			Tabs.Background = new SolidColorBrush(Colors.Transparent);
		}
		else
		{
			string colorw = SettingsService.CoreSettings.ColorTV;
			Windows.UI.Color color = (Windows.UI.Color)XamlBindingHelper.ConvertValue(typeof(Windows.UI.Color), colorw);
			SolidColorBrush brush = new(color);
			Tabs.Background = brush;
		}
		if (SettingsService.CoreSettings.ColorTool is "#000000" or "#FF000000")
		{
			ClassicToolbar.Background = new SolidColorBrush(Colors.Transparent);
		}
		else
		{
			string colorw = SettingsService.CoreSettings.ColorTool;
			Windows.UI.Color color = (Windows.UI.Color)XamlBindingHelper.ConvertValue(typeof(Windows.UI.Color), colorw);
			SolidColorBrush brush = new(color);
			ClassicToolbar.Background = brush;
		}
	}

	private async void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
	{


		if (Tabs.TabItems?.Count > 1)
		{
			if (SettingsService.CoreSettings.ConfirmCloseDlg)
			{
				try
				{
					args.Cancel = true;

					if (Application.Current is not App currentApp || currentApp.m_window is not MainWindow mainWindow)
					{
						return;
					}

					Fire.Core.CoreUi.ConfirmAppClose quickConfigurationDialog = new()
					{
						XamlRoot = mainWindow.Content.XamlRoot
					};

					quickConfigurationDialog.PrimaryButtonClick += async (_, _) =>
					{
						quickConfigurationDialog.Hide();

						await Task.Delay(250);

						Application.Current.Exit();

					};
					_ = await quickConfigurationDialog.ShowAsync();
				}
				catch (Exception ex)
				{
					ExceptionLogger.LogException(ex);
				}
			}
			return;
		}
		args.Cancel = false;
	}

	private bool incog = false;

	private async void ArgsPassed()
	{
		TitleTop();

		if (!string.IsNullOrEmpty(AppArguments.UrlArgument) &&
			Uri.TryCreate(AppArguments.UrlArgument, UriKind.Absolute, out Uri uri))
		{
			Tabs.TabItems.Add(CreateNewTab(typeof(WebContent), uri));
			return;
		}

		if (!string.IsNullOrEmpty(AppArguments.FireBrowserArgument) ||
			!string.IsNullOrEmpty(AppArguments.FireUser))
		{
			Tabs.TabItems.Add(CreateNewTab(typeof(NewTab)));
			return;
		}

		if (!string.IsNullOrEmpty(AppArguments.FireBrowserPdf))
		{
			StorageFile file = await StorageFile.GetFileFromPathAsync(AppArguments.FireBrowserPdf);
			ReadOnlyCollection<IStorageItem> files = new List<IStorageItem> { file }.AsReadOnly();
			if (files.Count > 0)
			{
				Tabs.TabItems.Add(CreateNewTab(typeof(WebContent), files[1]));
			}
			return;
		}

		if (!string.IsNullOrEmpty(AppArguments.FireBrowserIncog))
		{
			Tabs.TabItems.Add(CreateNewIncog(typeof(InPrivate)));
			Control[] controlsToDisable = new Control[] { Fav, His, History, Down, DownBtn, FavoritesButton, UserFrame };
			foreach (Control control in controlsToDisable)
			{
				control.IsEnabled = false;
			}

			NewTab.Visibility = Visibility.Collapsed;
			NewWindow.Visibility = Visibility.Collapsed;
			//Profile.IsEnabled = false;
			WebContent.IsIncognitoModeEnabled = true;
			Profile.IsEnabled = false;
			AddFav.IsEnabled = false;
			UserDataManager.DeleteUser("Private");
			InPrivateUser();
			incog = true;
			return;
		}

		Tabs.TabItems.Add(CreateNewTab(typeof(NewTab)));
	}


	private void InPrivateUser()
	{
		User newUser = new()
		{
			Id = Guid.NewGuid(),
			Username = "Private",
			IsFirstLaunch = true,
			UserSettings = null
		};

		AuthService.AddUser(newUser);
		UserFolderManager.CreateUserFolders(newUser);
		AuthService.CurrentUser.Username = newUser.Username;
		_ = AuthService.Authenticate(newUser.Username);
	}

	public void LoadUsernames()
	{
		string currentUsername = AuthService.CurrentUser?.Username;
		UserListView.Items.Clear();

		if (!(UserListView.IsEnabled = currentUsername is null || !currentUsername.Contains("Private")))
		{
			return;
		}

		AuthService.GetAllUsernames()
			.Where(u => u != currentUsername && !u.Contains("Private"))
			.Where(u => u != currentUsername && !u.Contains("__Admin__"))
			.ToList()
			.ForEach(UserListView.Items.Add);


	}

	public void SmallUpdates()
	{
		string source = TabWebView.CoreWebView2.Source?.ToString() ?? string.Empty;
		UrlBox.Text = ViewModel.Securitytype = source;

		(ViewModel.SecurityIcon, ViewModel.SecurityIcontext, ViewModel.Securitytext) = source switch
		{
			string s when s.Contains("https") => ("\uE72E", "Https Secured Website",
				"This Page Is Secured By A Valid SSL Certificate, Trusted By Root Authorities"),
			string s when s.Contains("http") => ("\uE785", "Http UnSecured Website",
				"This Page Is Unsecured By A Non-Valid SSL Certificate, Please Be Careful"),
			_ => ("", "", "")
		};
	}

	public void TitleTop()
	{
		nint hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
		WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
		appWindow = AppWindow.GetFromWindowId(windowId);
		appWindow.SetIcon("logo.ico");

		if (!AppWindowTitleBar.IsCustomizationSupported())
		{
			throw new Exception("Unsupported OS version.");
		}

		AppWindowTitleBar titleBar = appWindow.TitleBar;
		titleBar.ExtendsContentIntoTitleBar = true;
		Windows.UI.Color btnColor = Colors.Transparent;
		titleBar.BackgroundColor = titleBar.ButtonBackgroundColor =
			titleBar.InactiveBackgroundColor = titleBar.ButtonInactiveBackgroundColor =
			titleBar.ButtonHoverBackgroundColor = btnColor;

		ViewModel = new() { CurrentAddress = "", SecurityIcon = "\uE946", SecurityIcontext = "FireBrowser NewTab", Securitytext = "This The Default Home Page Of FireBrowser Internal Pages Secure", Securitytype = "Link - FireBrowser://NewTab" };
	}

	public static string launchurl { get; set; }
	public static string SearchUrl { get; set; }

	public bool isFull = false;

	public void GoFullScreenWeb(bool fullscreen)
	{
		IntPtr hWnd = WindowNative.GetWindowHandle(this);
		Microsoft.UI.WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
		AppWindow view = AppWindow.GetFromWindowId(wndId);
		Thickness margin = fullscreen ? new Thickness(0, -40, 0, 0) : new Thickness(0, 35, 0, 0);

		view.SetPresenter(fullscreen ? AppWindowPresenterKind.FullScreen : AppWindowPresenterKind.Default);

		ClassicToolbar.Height = fullscreen ? 0 : 36;

		TabContent.Margin = margin;
	}

	public void GoFullScreen(bool fullscreen)
	{
		IntPtr hWnd = WindowNative.GetWindowHandle(this);
		Microsoft.UI.WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
		AppWindow view = AppWindow.GetFromWindowId(wndId);

		view.SetPresenter(fullscreen ? AppWindowPresenterKind.FullScreen : AppWindowPresenterKind.Default);
		isFull = fullscreen;
		TextFull.Text = fullscreen ? "Exit FullScreen" : "Full Screen";
	}

	public Task LoadUserSettings()
	{
		LoadUsernames();
		UpdateUIBasedOnSettings();
		setColorsTool();
		return Task.CompletedTask;
	}

	private void LoadUserDataAndSettings()
	{
		Fire.Browser.Core.User currentUser = AuthService.IsUserAuthenticated ? AuthService.CurrentUser : null;

		if (currentUser == null || (!AuthService.IsUserAuthenticated && !AuthService.Authenticate(currentUser?.Username)))
		{
			UserName.Text = "DefaultUser";
			return;
		}
		UserName.Text = currentUser.Username ?? "DefaultUser";
	}

	private void UpdateUIBasedOnSettings()
	{
		Settings coreSet = SettingsService.CoreSettings; //  UserFolderManager.LoadcoreSet(AuthService.CurrentUser);

		SetVisibility(AdBlock, coreSet.AdblockBtn is not false);
		SetVisibility(ReadBtn, coreSet.ReadButton is not false);
		SetVisibility(BtnTrans, coreSet.Translate is not false);
		SetVisibility(BtnDark, coreSet.DarkIcon is not false);
		SetVisibility(ToolBoxMore, coreSet.ToolIcon is not false);
		SetVisibility(AddFav, coreSet.FavoritesL is not false);
		SetVisibility(FavoritesButton, coreSet.Favorites is not false);
		SetVisibility(DownBtn, coreSet.Downloads is not false);
		SetVisibility(History, coreSet.Historybtn is not false);
		SetVisibility(QrBtn, coreSet.QrCode is not false);
		SetVisibility(BackBtn, coreSet.BackButton is not false);
		SetVisibility(ForwBtn, coreSet.ForwardButton is not false);
		SetVisibility(ReloadBtn, coreSet.RefreshButton is not false);
		SetVisibility(HomeBtn, coreSet.HomeButton is not false);
	}

	private void SetVisibility(UIElement element, bool isVisible)
	{
		element.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
	}

	private readonly int maxTabItems = 20;
	private void TabView_AddTabButtonClick(TabView sender, object args)
	{
		if (sender.TabItems.Count < maxTabItems)
		{
			sender.TabItems.Add(incog == true ? CreateNewIncog(typeof(InPrivate)) : CreateNewTab(typeof(NewTab)));
		}
	}

	#region toolbar

	public ToolbarViewModel ViewModel { get; set; }
	public class Passer
	{
		public FireBrowserTabViewItem Tab { get; set; }
		public FireBrowserTabViewContainer TabView { get; set; }
		public object Param { get; set; }
		public ToolbarViewModel ViewModel { get; set; }
	}

	#endregion

	public FireBrowserTabViewItem CreateNewTab(Type? page = null, object param = null, int index = -1)
	{
		_ = Tabs.TabItems.Count;

		FireBrowserTabViewItem newItem = new()
		{
			Header = "NewTab",
			IconSource = new Microsoft.UI.Xaml.Controls.SymbolIconSource { Symbol = Symbol.Home },
			Style = (Style)Microsoft.UI.Xaml.Application.Current.Resources["FloatingTabViewItemStyle"]
		};

		Passer passer = new()
		{
			Tab = newItem,
			TabView = Tabs,
			ViewModel = new ToolbarViewModel(),
			Param = param,
		};


		passer.ViewModel.CurrentAddress = "";

		double margin = ClassicToolbar.Height;
		Frame frame = new()
		{
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Stretch,
			Margin = new Thickness(0, margin, 0, 0)
		};

		if (page != null)
		{
			_ = frame.Navigate(page, passer);
		}

		newItem.Content = frame;

		return newItem;
	}


	public Frame TabContent => (Tabs.SelectedItem as FireBrowserTabViewItem)?.Content as Frame;
	public WebView2 TabWebView => (TabContent?.Content as WebContent)?.WebViewElement;
	public FireBrowserTabViewContainer TabViewContainer => Tabs;
	private double GetScaleAdjustment()
	{
		nint hWnd = WindowNative.GetWindowHandle(this);
		WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
		DisplayArea displayArea = DisplayArea.GetFromWindowId(wndId, DisplayAreaFallback.Primary);
		nint hMonitor = Win32Interop.GetMonitorFromDisplayId(displayArea.DisplayId);

		// Get the effective DPI for the monitor
		_ = Windowing.GetDpiForMonitor(hMonitor, Windowing.Monitor_DPI_Type.MDT_Effective_DPI, out uint dpiX, out uint dpiY);

		// Calculate the average DPI scaling factor
		double scaleX = dpiX / 96.0;
		double scaleY = dpiY / 96.0;

		// Depending on your UI, you may want to return the average or just one axis
		return (scaleX + scaleY) / 2.0; // Average of X and Y scaling
	}


	private void Tabs_Loaded(object sender, RoutedEventArgs e)
	{
		Apptitlebar.SizeChanged += Apptitlebar_SizeChanged;
		Apptitlebar_LayoutUpdated(sender, e);
	}

	private void Apptitlebar_SizeChanged(object sender, SizeChangedEventArgs e)
	{
		try
		{
			double scaleAdjustment = GetScaleAdjustment();
			Apptitlebar.Measure(new Windows.Foundation.Size(double.PositiveInfinity, double.PositiveInfinity));
			Windows.Foundation.Point customDragRegionPosition = Apptitlebar.TransformToVisual(null).TransformPoint(new Windows.Foundation.Point(0, 0));

			Windows.Graphics.RectInt32[] dragRects = new Windows.Graphics.RectInt32[2];

			for (int i = 0; i < 2; i++)
			{
				dragRects[i] = new Windows.Graphics.RectInt32
				{
					X = (int)((customDragRegionPosition.X + (i * Apptitlebar.ActualWidth / 2)) * scaleAdjustment),
					Y = (int)(customDragRegionPosition.Y * scaleAdjustment),
					Height = (int)((Apptitlebar.ActualHeight - customDragRegionPosition.Y) * scaleAdjustment),
					Width = (int)(Apptitlebar.ActualWidth / 2 * scaleAdjustment)
				};
			}

			appWindow.TitleBar?.SetDragRectangles(dragRects);
		}
		catch (Exception)
		{
			throw;
		}
	}

	private void Apptitlebar_LayoutUpdated(object sender, object e)
	{
		double scaleAdjustment = GetScaleAdjustment();
		Apptitlebar.Measure(new Windows.Foundation.Size(double.PositiveInfinity, double.PositiveInfinity));
		Windows.Foundation.Point customDragRegionPosition = Apptitlebar.TransformToVisual(null).TransformPoint(new Windows.Foundation.Point(0, 0));

		List<Windows.Graphics.RectInt32> dragRectsList = new();

		for (int i = 0; i < 2; i++)
		{
			Windows.Graphics.RectInt32 dragRect = new()
			{
				X = (int)((customDragRegionPosition.X + (i * Apptitlebar.ActualWidth / 2)) * scaleAdjustment),
				Y = (int)(customDragRegionPosition.Y * scaleAdjustment),
				Height = (int)((Apptitlebar.ActualHeight - customDragRegionPosition.Y) * scaleAdjustment),
				Width = (int)(Apptitlebar.ActualWidth / 2 * scaleAdjustment)
			};

			dragRectsList.Add(dragRect);
		}

		Windows.Graphics.RectInt32[] dragRects = dragRectsList.ToArray();

		appWindow.TitleBar?.SetDragRectangles(dragRects);
	}
	private void Tabs_TabItemsChanged(TabView sender, IVectorChangedEventArgs args)
	{
		if (sender.TabItems.Count <= 0)
		{
			Application.Current.Exit();
		}
		else
		{
			sender.CanReorderTabs = sender.CanDragTabs = sender.TabItems.Count > 1;
		}
	}
	public Passer CreatePasser(object parameter = null)
	{
		return new()
		{
			Tab = Tabs.SelectedItem as FireBrowserTabViewItem,
			TabView = Tabs,
			ViewModel = ViewModel,
			Param = parameter,
		};
	}
	public void SelectNewTab()
	{
		Tabs.SelectedIndex = Tabs.TabItems.Count - 1;
	}

	public void FocusUrlBox(string text)
	{
		UrlBox.Text = text;
		_ = UrlBox.Focus(FocusState.Programmatic);
	}
	public void FocusWebView()
	{
		_ = TabWebView.Focus(FocusState.Programmatic);
	}

	public void NavigateToUrl(string uri)
	{
		try
		{
			if (TabContent.Content is not WebContent webContent)
			{
				launchurl ??= uri;
				_ = TabContent.Navigate(typeof(WebContent), CreatePasser(uri));
				// newTab is not browsing to new site.
				_ = DispatcherQueue.TryEnqueue(async () => await MainWinSaveResources());

				return;
			}

			webContent.WebViewElement.CoreWebView2.Navigate(uri.ToString());
		}
		catch (Exception ex)
		{
			ExceptionLogger.LogException(ex);
		}
	}
	private async void UrlBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
	{
		// reload the settings due to changes from outside view. 
		SettingsService.Initialize();
		try
		{
			if (Fire.Core.Helpers.UrlValidater.GetValidateUrl(UrlBox.Text.ToLowerInvariant()) is Uri browserTo)
			{
				switch (UrlBox.Text.ToLowerInvariant())
				{
					case "firebrowser://newtab":
						Tabs.TabItems.Add(CreateNewTab(typeof(NewTab)));
						SelectNewTab();
						break;
					case "firebrowser://settings":
						Tabs.TabItems.Add(CreateNewTab(typeof(SettingsPage)));
						SelectNewTab();
						break;
					case "firebrowser://modules":
						Tabs.TabItems.Add(CreateNewTab(typeof(Pluginss)));
						SelectNewTab();
						break;
					//case "firebrowser://vault":
					//   Tabs.TabItems.Add(CreateNewTab(typeof(SecureVault)));
					//   SelectNewTab();
					//   break;
					// case "firebrowser://api-route":
					//  Tabs.TabItems.Add(CreateNewTab(typeof(ApiDash)));
					//  SelectNewTab();
					//  break;
					default:
						// default behavior
						if (await Fire.Core.Helpers.UrlValidater.IsUrlReachable(browserTo))
						{
							NavigateToUrl(browserTo.AbsoluteUri);
						}
						else
						{
							string searchurl = SearchUrl ?? $"{SettingsService.CoreSettings.SearchUrl}";
							string query = searchurl + UrlEncoder.Default.Encode(UrlBox.Text.ToLowerInvariant());
							NavigateToUrl(query);

						}
						break;
				}

			}
			else
			{
				string searchurl = SearchUrl ?? $"{SettingsService.CoreSettings.SearchUrl}";
				string query = searchurl + UrlEncoder.Default.Encode(UrlBox.Text.ToLowerInvariant());

				NavigateToUrl(query);

			}

			//string input = UrlBox.Text.ToString();
			//string inputtype = UrlHelper.GetInputType(input);

			//try
			//{
			//    if (input.Contains("firebrowser://"))
			//    {
			//        switch (input)
			//        {
			//            case "firebrowser://newtab":
			//                Tabs.TabItems.Add(CreateNewTab(typeof(NewTab)));
			//                SelectNewTab();
			//                break;
			//            case "firebrowser://settings":
			//                Tabs.TabItems.Add(CreateNewTab(typeof(SettingsPage)));
			//                SelectNewTab();
			//                break;
			//            case "firebrowser://modules":
			//                Tabs.TabItems.Add(CreateNewTab(typeof(Pluginss)));
			//                SelectNewTab();
			//                break;
			//            //case "firebrowser://vault":
			//            //   Tabs.TabItems.Add(CreateNewTab(typeof(SecureVault)));
			//            //   SelectNewTab();
			//            //   break;
			//            // case "firebrowser://api-route":
			//            //  Tabs.TabItems.Add(CreateNewTab(typeof(ApiDash)));
			//            //  SelectNewTab();
			//            //  break;
			//            default:
			//                // default behavior
			//                break;
			//        }
			//    }
			//    else if (inputtype is "url" or "urlNOProtocol")
			//    {
			//        string url = default;

			//        if (input.Contains("ms-appx:") || input.Contains("ms-appx-web:"))
			//            url = input.Trim();
			//        else
			//        {
			//            url = inputtype == "url" ? input.Trim() : "https://" + input.Trim();
			//        }
			//        NavigateToUrl(url);
			//    }
			//    else
			//    {
			//        //Settings userSettings = UserFolderManager.LoadUserSettings(AuthService.CurrentUser);
			//        string searchurl = SearchUrl ?? $"{SettingsService.CoreSettings.SearchUrl}";
			//        string query = searchurl + input;
			//        query = UrlEncoder.Default.Encode(query);
			//        NavigateToUrl(query);
			//    }
		}
		catch (Exception ex)
		{
			ExceptionLogger.LogException(ex);
		}
	}

	#region cangochecks

	private bool CanNavigate(bool isBack)
	{
		bool canNavigate = isBack
			? (TabContent?.Content is WebContent ? TabWebView?.CoreWebView2.CanGoBack ?? false : TabContent?.CanGoBack ?? false)
			: (TabContent?.Content is WebContent ? TabWebView?.CoreWebView2.CanGoForward ?? false : TabContent?.CanGoForward ?? false);

		// Update the view model with the navigation states
		ViewModel.UpdateNavigationState(
			canNavigate,
			isBack ? ViewModel.CanGoForward : ViewModel.CanGoBack
		);

		return canNavigate;
	}

	private void Go(bool isBack)
	{
		if (CanNavigate(isBack) && TabContent != null)
		{
			if (TabContent.Content is WebContent)
			{
				if (isBack)
				{
					TabWebView.CoreWebView2.GoBack();
				}
				else
				{
					TabWebView.CoreWebView2.GoForward();
				}
			}
			else
			{
				if (isBack)
				{
					TabContent.GoBack();
				}
				else
				{
					TabContent.GoForward();
				}
			}
		}
	}

	private bool CanGoBack()
	{
		return CanNavigate(true);
	}

	private bool CanGoForward()
	{
		return CanNavigate(false);
	}

	private void GoBack()
	{
		Go(true);
	}

	private void GoForward()
	{
		Go(false);
	}

	#endregion

	#region click
	private async void ToolbarButtonClick(object sender, RoutedEventArgs e)
	{
		Passer passer = new()
		{
			Tab = Tabs.SelectedItem as FireBrowserTabViewItem,
			TabView = Tabs,
			ViewModel = ViewModel
		};

		switch ((sender as Button).Tag)
		{
			case "Back":
				GoBack();
				break;
			case "Forward":
				GoForward();
				break;
			case "Refresh" when TabContent.Content is WebContent:
				TabWebView.CoreWebView2.Reload();
				_ = NotificationQueue.Show("Refreshing...", 1200);

				break;
			case "Home" when TabContent.Content is WebContent:
				if (incog == true)
				{
					_ = TabContent.Navigate(typeof(InPrivate));
				}
				else
				{
					_ = TabContent.Navigate(typeof(NewTab));
				}
				UrlBox.Text = "";
				passer.Tab.Header = WebContent.IsIncognitoModeEnabled ? "Incognito" : "NewTab";
				passer.Tab.IconSource = new Microsoft.UI.Xaml.Controls.SymbolIconSource()
				{
					Symbol = WebContent.IsIncognitoModeEnabled ? Symbol.BlockContact : Symbol.Home
				};
				ViewModel.CurrentAddress = "";
				break;
			case "Translate" when TabContent.Content is WebContent:
				string url = (TabContent.Content as WebContent).WebViewElement.CoreWebView2.Source.ToString();
				(TabContent.Content as WebContent).WebViewElement.CoreWebView2.Navigate($"https://translate.google.com/translate?hl&u={url}");
				break;
			case "QRCode" when TabContent.Content is WebContent:
				try
				{
					QRCodeGenerator qrGenerator = new();
					QRCodeData qrCodeData = qrGenerator.CreateQrCode((TabContent.Content as WebContent).WebViewElement.CoreWebView2.Source.ToString(), QRCodeGenerator.ECCLevel.M);
					BitmapByteQRCode qrCodeBmp = new(qrCodeData);
					byte[] qrCodeImageBmp = qrCodeBmp.GetGraphic(20);

					using InMemoryRandomAccessStream stream = new();
					using DataWriter writer = new(stream.GetOutputStreamAt(0));
					writer.WriteBytes(qrCodeImageBmp);
					_ = await writer.StoreAsync();

					BitmapImage image = new();
					await image.SetSourceAsync(stream);

					QRCodeImage.Source = image;
				}
				catch
				{
					QRCodeFlyout.Hide();
				}
				break;
			case "ReadingMode" when TabContent.Content is WebContent:
				Uri uriread = new("ms-appx:///Services/WebHelpers/ReadabilityWeb.js");
				StorageFile ReadabilityWebFile = await StorageFile.GetFileFromApplicationUriAsync(uriread);

				// Read the contents of the DarkModeWeb.js file asynchronously
				string jscript = await FileIO.ReadTextAsync(ReadabilityWebFile);
				_ = await (TabContent.Content as WebContent).WebViewElement.CoreWebView2.ExecuteScriptAsync(jscript);
				break;
			case "AdBlock":

			case "AddFavoriteFlyout" when TabContent.Content is WebContent:
				FavoriteTitle.Text = TabWebView.CoreWebView2.DocumentTitle;
				FavoriteUrl.Text = TabWebView.CoreWebView2.Source;
				break;
			case "AddFavorite":

				if (string.IsNullOrEmpty(FavoriteTitle.Text) || string.IsNullOrEmpty(FavoriteUrl.Text)) break;

				FavManager fv = new();
				fv.SaveFav(FavoriteTitle.Text.ToString(), FavoriteUrl.Text.ToString());
				AddFav.Flyout?.Hide();
				Notification note = new()
				{
					Title = $"Added To Favorites",
					Message = FavoriteTitle.Text.ToString(),
					Severity = InfoBarSeverity.Informational,
					Duration = TimeSpan.FromSeconds(1.5)
				};
				_ = NotificationQueue.Show(note);
				break;
			case "Favorites":
				FavManager fs = new();
				List<FavItem> favorites = fs.LoadFav();
				FavoritesListView.ItemsSource = favorites;
				break;
			case "DarkMode" when TabContent.Content is WebContent:
				Uri uri = new("ms-appx:///Services/WebHelpers/DarkModeWeb.js");
				StorageFile darkmodeFile = await StorageFile.GetFileFromApplicationUriAsync(uri);

				// Read the contents of the DarkModeWeb.js file asynchronously
				string jscriptdark = await FileIO.ReadTextAsync(darkmodeFile);
				_ = await (TabContent.Content as WebContent).WebViewElement.CoreWebView2.ExecuteScriptAsync(jscriptdark);

				break;
			case "History":
				FetchBrowserHistory();
				break;
		}
	}


	#endregion


	private async Task MainWinSaveResources()
	{
		if (SettingsService?.CoreSettings?.ResourceSave == false)
		{
			return;
		}

		List<object> list = TabViewContainer?.TabItems?.ToList();

		list!.ForEach(async (tab) =>
		{
			FireBrowserTabViewItem CurrentTab = tab as FireBrowserTabViewItem;

			// covers all tabs.- in future if we are selecting desired tab that's playing we could add a check NO TO Stop video from playing hence were selecting that tab.

			if (CurrentTab?.Content is Frame frame)
			{
				if (frame?.Content is WebContent web)
				{
					// webview needs a source to have a CoreWebView 
					if (web.WebView?.Source is not null)
					{
						await web.WebView.EnsureCoreWebView2Async();
						_ = await web.WebView.CoreWebView2!.ExecuteScriptAsync(
										@"(function() { 
                                try
                                {
                                    const videos = document.querySelectorAll('video');
                                    videos.forEach((video) => { video.pause();});
                                    console.log('WINUI3_CoreWebView2: YES_VIDEOS_CLOSED');
                                    return true; 

                                }
                                catch(error) {
                                    console.log('WINUI3_CoreWebView2: NO_VIDEOS_CLOSED');
                                    return error.message; 
                                }
                            })();");
					}
				}
			}
		});

		await Task.CompletedTask;
	}
	private async void Tabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		Settings coreSet = SettingsService.CoreSettings; // UserFolderManager.LoadcoreSet(AuthService.CurrentUser);

		if (TabContent?.Content is WebContent webContent)
		{
			TabWebView.NavigationStarting += (_, _) => ViewModel.CanRefresh = false;
			TabWebView.NavigationCompleted += (_, _) => ViewModel.CanRefresh = true;

			await TabWebView.EnsureCoreWebView2Async();

			// 2014-02-04 added to stop a video from playing when selection is made to a different tab / save on memory resources. 
			if (e.RemovedItems.Count > 0)
			{
				_ = e.RemovedItems.All((tab) =>
				{
					if (tab is FireBrowserTabViewItem viewedItem)
					{
						//need pip mode automaticcly open en close 
						_ = DispatcherQueue.TryEnqueue(async () => await MainWinSaveResources());
					}
					return false;
				});
			}

			SmallUpdates();
		}
		else
		{
			ViewModel.CanRefresh = false;
			ViewModel.CurrentAddress = null;
		}
	}

	//public static async void OpenNewWindow(Uri uri) =>  await Windows.System.Launcher.LaunchUriAsync(uri);
	public static async void OpenNewWindow(Uri uri)
	{
		_ = await Windows.System.Launcher.LaunchUriAsync(uri);
	}

	public void ShareUi(string Url, string Title)
	{
		nint hWnd = WindowNative.GetWindowHandle(this);
		ShareUIHelper.ShowShareUIURL(Url, Title, hWnd);
	}

	private void TabMenuClick(object sender, RoutedEventArgs e)
	{
		switch ((sender as Button).Tag)
		{
			case "NewTab":
				Tabs.TabItems.Add(CreateNewTab(typeof(NewTab)));
				SelectNewTab();
				break;
			case "NewWindow":
				OpenNewWindow(new Uri($"firebrowseruser://{UserName.Text}"));
				break;
			case "Share" when TabContent.Content is WebContent:
				ShareUi(TabWebView.CoreWebView2.DocumentTitle, TabWebView.CoreWebView2.Source);
				break;
			case "DevTools":
				if (TabContent.Content is WebContent)
				{
					(TabContent.Content as WebContent).WebViewElement.CoreWebView2.OpenDevToolsWindow();
				}
				break;
			case "Settings":
				Tabs.TabItems.Add(CreateNewTab(typeof(SettingsPage)));
				SelectNewTab();
				break;
			case "FullScreen":
				GoFullScreen(isFull != true);
				break;
			case "Downloads":
				UrlBox.Text = "firebrowser://downloads";
				_ = TabContent.Navigate(typeof(FireBrowserWinUi3.Pages.TimeLinePages.MainTimeLine));
				break;
			case "History":
				UrlBox.Text = "firebrowser://history";
				_ = TabContent.Navigate(typeof(FireBrowserWinUi3.Pages.TimeLinePages.MainTimeLine));
				break;
			case "InPrivate":
				OpenNewWindow(new Uri("firebrowserincog://"));
				break;
			case "SplitTab":
				_ = TabContent.Navigate(typeof(SplitTabPage));
				break;
			case "Favorites":
				UrlBox.Text = "firebrowser://favorites";
				_ = TabContent.Navigate(typeof(FireBrowserWinUi3.Pages.TimeLinePages.MainTimeLine));
				break;
		}

		ViewModelMain?.CloseMoreFlyout();
	}

	#region database

	private async void ClearDb()
	{
		try
		{
			HistoryActions historyActions = new(AuthService.CurrentUser.Username);
			await historyActions.DeleteAllHistoryItems();

			HistoryTemp.ItemsSource = null;
		}
		catch (Exception ex)
		{
			ExceptionLogger.LogException(ex);
		}


	}

	private ObservableCollection<HistoryItem> browserHistory;

	private async void FetchBrowserHistory()
	{
		//Fire.Browser.Core.User user = AuthService.CurrentUser;
		try
		{
			HistoryActions historyActions = new(AuthService.CurrentUser.Username);
			browserHistory = await historyActions.GetAllHistoryItems();
			HistoryTemp.ItemsSource = browserHistory;
		}
		catch (Exception ex)
		{
			ExceptionLogger.LogException(ex);
		}

	}

	#endregion

	public FireBrowserTabViewItem CreateNewIncog(Type? page = null, object? param = null, int index = -1)
	{
		_ = Tabs.TabItems.Count;
		UrlBox.Text = "";

		FireBrowserTabViewItem newItem = new()
		{
			Header = $"Incognito",
			IconSource = new Microsoft.UI.Xaml.Controls.SymbolIconSource { Symbol = Symbol.BlockContact },
			Style = (Style)Microsoft.UI.Xaml.Application.Current.Resources["FloatingTabViewItemStyle"]
		};


		Passer passer = new()
		{
			Tab = newItem,
			TabView = Tabs,
			ViewModel = new ToolbarViewModel(),
			Param = param
		};

		Frame frame = new()
		{
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Stretch,
			Margin = new Thickness(0, 37, 0, 0)
		};

		_ = page != null ? frame.Navigate(page, passer) : frame.Navigate(typeof(FireBrowserWinUi3.Pages.InPrivate), passer);

		newItem.Content = frame;
		return newItem;
	}

	private void Tabs_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
	{
		if (args.Tab?.Content is not Frame tabContent)
		{
			return;
		}

		if (tabContent.Content is WebContent webContent && webContent.WebViewElement != null)
		{
			webContent.WebViewElement.Close();
		}

		_ = (sender?.TabItems?.Remove(args.Tab));
	}

	private string selectedHistoryItem;
	private void Grid_RightTapped(object sender, Microsoft.UI.Xaml.Input.RightTappedRoutedEventArgs e)
	{
		if (((FrameworkElement)sender).DataContext is not HistoryItem historyItem)
		{
			return;
		}

		selectedHistoryItem = historyItem.Url;

		MenuFlyout flyout = new();

		MenuFlyoutItem deleteMenuItem = new()
		{
			Text = "Delete This Record",
			Icon = new FontIcon { Glyph = "\uE74D" }
		};

		deleteMenuItem.Click += async (s, args) =>
		{
			if (AuthService.CurrentUser is not Fire.Browser.Core.User user)
			{
				return;
			}

			string username = user.Username;
			string databasePath = Path.Combine(
				UserDataManager.CoreFolderPath,
				UserDataManager.UsersFolderPath,
				username,
				"Database",
				"History.db"
			);

			HistoryActions historyActions = new(AuthService.CurrentUser.Username);
			await historyActions.DeleteHistoryItem(selectedHistoryItem);
			await Task.Delay(1000);

			if (HistoryTemp.ItemsSource is ObservableCollection<HistoryItem> historyItems)
			{
				HistoryItem itemToRemove = historyItems.FirstOrDefault(item => item.Url == selectedHistoryItem);
				if (itemToRemove != null)
				{
					_ = historyItems.Remove(itemToRemove);
				}
			}
		};

		flyout.Items.Add(deleteMenuItem);

		flyout.ShowAt((FrameworkElement)sender, e.GetPosition((FrameworkElement)sender));
	}
	private void ClearHistoryDataMenuItem_Click(object sender, RoutedEventArgs e) { ClearDb(); }
	private void SearchHistoryMenuFlyout_Click(object sender, RoutedEventArgs e)
	{

		HistorySearchMenuItem.Visibility = HistorySearchMenuItem.Visibility == Visibility.Collapsed
			? Visibility.Visible
			: Visibility.Collapsed;

		HistorySmallTitle.Visibility = HistorySmallTitle.Visibility == Visibility.Collapsed
			? Visibility.Visible
			: Visibility.Collapsed;


		if (HistorySearchMenuItem.Visibility is Visibility.Visible)
		{
			_ = HistorySearchMenuItem.Focus(FocusState.Programmatic);
		}
	}
	private void FilterBrowserHistory(string searchText)
	{
		if (browserHistory == null)
		{
			return;
		}

		HistoryTemp.ItemsSource = null;

		ObservableCollection<HistoryItem> filteredHistory = new(browserHistory
			.Where(item => item.Url.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
						   item.Title?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true));

		HistoryTemp.ItemsSource = filteredHistory;
	}

	private void HistorySearchMenuItem_TextChanged(object sender, TextChangedEventArgs e)
	{
		string searchText = HistorySearchMenuItem.Text;
		FilterBrowserHistory(searchText);
	}

	private void FavoritesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (sender is not ListView listView || listView.ItemsSource == null)
		{
			return;
		}

		if (listView.SelectedItem is FavItem item)
		{
			string launchurlfav = item.Url;
			if (TabContent.Content is WebContent webContent)
			{
				webContent.WebViewElement.CoreWebView2.Navigate(launchurlfav);
			}
			else
			{
				_ = TabContent.Navigate(typeof(WebContent), CreatePasser(launchurlfav));
			}
		}

		listView.ItemsSource = null;
		FavoritesFly.Hide();
	}

	private void HistoryTemp_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (sender is not ListView listView || listView.ItemsSource == null)
		{
			return;
		}

		if (listView.SelectedItem is not HistoryItem item)
		{
			return;
		}

		string launchurlfav = item.Url;
		if (TabContent.Content is WebContent webContent)
		{
			webContent.WebViewElement.CoreWebView2.Navigate(launchurlfav);
		}
		else
		{
			_ = TabContent.Navigate(typeof(WebContent), CreatePasser(launchurlfav));
		}

		listView.ItemsSource = null;
		HistoryFlyoutMenu.Hide();
	}

	private void DownBtn_Click(object sender, RoutedEventArgs e)
	{
		Microsoft.UI.Xaml.Controls.Primitives.FlyoutShowOptions options = new()
		{ Placement = Microsoft.UI.Xaml.Controls.Primitives.FlyoutPlacementMode.Bottom };
		DownloadFlyout.ShowAt(DownBtn, options);
	}
	private void OpenHistoryMenuItem_Click(object sender, RoutedEventArgs e) { _ = (UrlBox.Text = "firebrowser://history") + TabContent.Navigate(typeof(FireBrowserWinUi3.Pages.TimeLinePages.MainTimeLine)); }
	private void OpenFavoritesMenu_Click(object sender, RoutedEventArgs e) { _ = (UrlBox.Text = "firebrowser://favorites") + TabContent.Navigate(typeof(FireBrowserWinUi3.Pages.TimeLinePages.MainTimeLine)); }
	private void MainUser_Click(object sender, RoutedEventArgs e)
	{
		UserFrame.Visibility = UserFrame?.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
		LoadUsernames();
	}
	private void MoreTool_Click(object sender, RoutedEventArgs e) { UserFrame.Visibility = Visibility.Collapsed; }
	private async void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			// Prompt the user for biometric or PIN authentication
			UserConsentVerificationResult authResult = await UserConsentVerifier.RequestVerificationAsync("Authenticate to open the 2fa data");

			if (authResult == UserConsentVerificationResult.Verified)
			{
				Fire.Authentication.ViewModels.TwoFactorsAuthentification.ShowFlyout(Secure);
			}
			else
			{
				Console.WriteLine("User authentication failed.");
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error authenticating: {ex.Message}");
		}
	}
	private void MenuFlyoutItem_Click_1(object sender, RoutedEventArgs e) { Fire.Core.Helpers.FlyoutLoad.ShowFlyout(Secure); }
	private async void SaveQrImage_Click(object sender, RoutedEventArgs e)
	{
		if (TabContent.Content is WebContent webContent)
		{
			QRCodeData qrCodeData = new QRCodeGenerator().CreateQrCode(webContent.WebViewElement.CoreWebView2.Source.ToString(), QRCodeGenerator.ECCLevel.M);
			byte[] qrCodeBmp = new BitmapByteQRCode(qrCodeData).GetGraphic(20);

			FileSavePicker savePicker = new()
			{
				SuggestedStartLocation = PickerLocationId.PicturesLibrary,
				DefaultFileExtension = ".png",
				SuggestedFileName = "QrImage"
			};

			WinRT.Interop.InitializeWithWindow.Initialize(savePicker, WinRT.Interop.WindowNative.GetWindowHandle((Application.Current as App)?.m_window as MainWindow));
			savePicker.FileTypeChoices.Add("PNG files", new List<string>() { ".png" });

			if (await savePicker.PickSaveFileAsync() is StorageFile file)
			{
				try
				{
					_ = await (await file.OpenAsync(FileAccessMode.ReadWrite)).WriteAsync(qrCodeBmp.AsBuffer());
				}
				catch (Exception ex)
				{
					ExceptionLogger.LogException(ex);
				}
			}
		}
	}
	private async void SwitchName_Click(object sender, RoutedEventArgs e)
	{
		if (!(sender is Button switchButton && switchButton.DataContext is string clickedUserName))
		{
			return;
		}

		OpenNewWindow(new Uri($"firebrowseruser://{clickedUserName}"));
		await new Shortcut().CreateShortcut(clickedUserName);
	}

	private void Profile_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
	{
		Microsoft.UI.Xaml.Controls.Primitives.FlyoutShowOptions options = new()
		{ Placement = Microsoft.UI.Xaml.Controls.Primitives.FlyoutPlacementMode.Bottom };
		Commander.ShowAt(Profile, options);
	}

	private void UrlBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
	{
		if (UrlBox.Text.Contains("youtube:"))
		{
			UrlBox.Text = "https://www.youtube.com/results?search_query=";
			_ = UrlBox.Focus(FocusState.Keyboard);
			_ = NotificationQueue.Show("Autofill Trigger Youtube Quick Search", 2500);
		}
		if (UrlBox.Text.Equals("192"))
		{
			UrlBox.Text = "http://192.";
			_ = UrlBox.Focus(FocusState.Keyboard);
			_ = NotificationQueue.Show("Autofill Local Site http://...", 2500);
		}
		if (UrlBox.Text.Equals("search:"))
		{
			UrlBox.Text = $"{SettingsService.CoreSettings.SearchUrl}";
			_ = UrlBox.Focus(FocusState.Keyboard);
			_ = NotificationQueue.Show($"Autofill Search Quick {SettingsService.CoreSettings.EngineFriendlyName}", 2500);
		}
	}

	private void Secure_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
	{
		Fire.Core.Helpers.FlyoutLoad.ShowFlyout(Secure);
	}

	private void UrlBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
	{
		UrlBox.Text = args.SelectedItem.ToString();
	}
}