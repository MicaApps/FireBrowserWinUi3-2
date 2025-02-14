﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Fire.Core.Exceptions;
using Fire.Core.Helpers;
using Fire.Core.Models;
using Fire.Data.Favorites;
using FireBrowserDatabase;
using FireBrowserWinUi3.Pages;
using FireBrowserWinUi3.Services;
using FireBrowserWinUi3.Services.Models;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using static FireBrowserWinUi3.Pages.NewTab;


namespace FireBrowserWinUi3.ViewModels;
public partial class HomeViewModel : ObservableRecipient
{
	private Settings.NewTabBackground _backgroundType;
	private string _imageTitle;
	private string _imageCopyright;
	private string _imageCopyrightLink;
	[ObservableProperty]
	private Visibility _ntpCoreVisibility;
	[ObservableProperty]
	private bool _isNtpTimeVisible;
	[ObservableProperty]
	private string _ntpTimeText;
	[ObservableProperty]
	private string _ntpDateText;
	[ObservableProperty]
	private bool _ntpTimeEnabled;
	[ObservableProperty]
	private bool _isFavoriteCardEnabled;
	[ObservableProperty]
	private bool _isFavoriteExpanded;
	[ObservableProperty]
	private bool _isHistoryCardEnabled;
	[ObservableProperty]
	private bool _isHistoryExpanded;
	[ObservableProperty]
	private bool _isSearchBoxEnabled;
	[ObservableProperty]
	private bool _istrendingEnabled;
	[ObservableProperty]
	private Visibility _isFavoritesVisible;
	[ObservableProperty]
	private Visibility _isHistoryVisible;
	[ObservableProperty]
	private Visibility _IsSearchVisible;
	[ObservableProperty]
	private Visibility _IsTrendingVisible;
	[ObservableProperty]
	private bool islogoEnabled;
	[ObservableProperty]
	private Visibility _IsLogoVisible;
	[ObservableProperty]
	private Brush _brushNtp = new SolidColorBrush(Colors.Ivory);
	[ObservableProperty]
	private UIElement _TeachingPoint;
	[ObservableProperty]
	private TrendingItem _trendingItem;

	[ObservableProperty]
	private SearchProviders _SearchProvider;
	public SettingsService SettingsService { get; set; }
	private DispatcherTimer timer { get; set; }
	public ObservableCollection<HistoryItem> HistoryItems { get; set; }
	public ObservableCollection<FavItem> FavoriteItems { get; set; }
	private void LoadUISettings()
	{
		NtpCoreVisibility = SettingsService.CoreSettings.NtpCoreVisibility ? Visibility.Visible : Visibility.Collapsed;
		IsNtpTimeVisible = SettingsService.CoreSettings.NtpDateTime;
		NtpTimeEnabled = SettingsService.CoreSettings.NtpDateTime;

		IsFavoritesVisible = SettingsService.CoreSettings.IsFavoritesVisible ? Visibility.Visible : Visibility.Collapsed;
		IsFavoriteCardEnabled = SettingsService.CoreSettings.IsFavoritesVisible;
		IsHistoryVisible = SettingsService.CoreSettings.IsHistoryVisible ? Visibility.Visible : Visibility.Collapsed;
		IsHistoryCardEnabled = SettingsService.CoreSettings.IsHistoryVisible;
		IsSearchVisible = SettingsService.CoreSettings.IsSearchVisible ? Visibility.Visible : Visibility.Collapsed;
		IsSearchBoxEnabled = SettingsService.CoreSettings.IsSearchVisible;
		IsTrendingVisible = SettingsService.CoreSettings.IsTrendingVisible ? Visibility.Visible : Visibility.Collapsed;
		IslogoEnabled = SettingsService.CoreSettings.IsLogoVisible;
		IsLogoVisible = SettingsService.CoreSettings.IsLogoVisible ? Visibility.Visible : Visibility.Collapsed;
		SearchProvider = SearchProviders.ProvidersList.FirstOrDefault(t => t.ProviderName == SettingsService.CoreSettings.EngineFriendlyName);

		IsFavoriteExpanded = SettingsService.CoreSettings.IsFavoritesToggled;
		IsHistoryExpanded = SettingsService.CoreSettings.IsHistoryToggled;
		if (SettingsService.CoreSettings.NtpTextColor != null)
		{
			BrushNtp = new SolidColorBrush((Windows.UI.Color)XamlBindingHelper.ConvertValue(typeof(Windows.UI.Color), SettingsService.CoreSettings.NtpTextColor));
		}
		OnPropertyChanged(nameof(SearchProvider));
		OnPropertyChanged(nameof(NtpCoreVisibility));
		OnPropertyChanged(nameof(IsNtpTimeVisible));
		OnPropertyChanged(nameof(NtpTimeEnabled));

		OnPropertyChanged(nameof(IsFavoritesVisible));
		OnPropertyChanged(nameof(IsHistoryVisible));
		OnPropertyChanged(nameof(IsSearchVisible));
		OnPropertyChanged(nameof(IsTrendingVisible));
		OnPropertyChanged(nameof(IstrendingEnabled));

		OnPropertyChanged(nameof(IslogoEnabled));
		OnPropertyChanged(nameof(IsLogoVisible));

		OnPropertyChanged(nameof(IsSearchBoxEnabled));
		OnPropertyChanged(nameof(IsFavoriteCardEnabled));
		OnPropertyChanged(nameof(IsHistoryCardEnabled));

		OnPropertyChanged(nameof(IsFavoriteExpanded));
		OnPropertyChanged(nameof(IsHistoryExpanded));
	}
	private async void UpdateUIControls()
	{
		NtpCoreVisibility = SettingsService.CoreSettings.NtpDateTime ? Visibility.Visible : Visibility.Collapsed;
		NtpTimeEnabled = SettingsService.CoreSettings.NtpDateTime;

		IsFavoriteCardEnabled = SettingsService.CoreSettings.IsFavoritesVisible;
		IsHistoryCardEnabled = SettingsService.CoreSettings.IsHistoryVisible;
		IsSearchBoxEnabled = SettingsService.CoreSettings.IsSearchVisible;
		IstrendingEnabled = SettingsService.CoreSettings.IsTrendingVisible;

		IsFavoritesVisible = IsFavoriteCardEnabled ? Visibility.Visible : Visibility.Collapsed;
		IsHistoryVisible = IsHistoryCardEnabled ? Visibility.Visible : Visibility.Collapsed;
		IsSearchVisible = IsSearchBoxEnabled ? Visibility.Visible : Visibility.Collapsed;
		IsTrendingVisible = IstrendingEnabled ? Visibility.Visible : Visibility.Collapsed;
		IslogoEnabled = SettingsService.CoreSettings.IsLogoVisible;
		IsLogoVisible = IslogoEnabled ? Visibility.Visible : Visibility.Collapsed;

		IsFavoriteExpanded = SettingsService.CoreSettings.IsFavoritesToggled;
		IsHistoryExpanded = SettingsService.CoreSettings.IsHistoryToggled;

		// - Use CoreSettings to save file access -> to Settings.json every 4 seconds handle in one place usings delegate...
		await SettingsService?.SaveChangesToSettings(Fire.Browser.Core.AuthService.CurrentUser, SettingsService.CoreSettings);
	}
	public void RaisePropertyChanges([CallerMemberName] string? propertyName = null)
	{
		OnPropertyChanged(propertyName);
	}

	public ObservableCollection<FavItem> LoadFavorites()
	{
		_ = new ObservableCollection<FavItem>();

		try
		{
			FavManager fs = new();
			ObservableCollection<FavItem> favorites = fs.LoadFav().ToObservableCollection();

			return favorites.ToObservableCollection();
		}
		catch (Exception ex)
		{
			ExceptionLogger.LogException(ex);
			return new ObservableCollection<FavItem>();
		}
	}

	public HomeViewModel(IMessenger messenger)
		: base(messenger)
	{

		FavoriteItems = new ObservableCollection<FavItem>();
		FavoriteItems.CollectionChanged += (s, e) => OnPropertyChanged(nameof(FavoriteItems));

		HistoryItems = new ObservableCollection<HistoryItem>();
		HistoryItems.CollectionChanged += (s, e) => OnPropertyChanged(nameof(HistoryItems));

		// LOAD settings service 
		SettingsService = App.GetService<SettingsService>();
		SettingsService.Initialize();
		// load ui settings from CoreSettings. 
		LoadUISettings();
	}

	[RelayCommand]
	private void ProtocolHandler(object sender)
	{

		if (Application.Current is App app && app.m_window is MainWindow window)
		{
			_ = (window.DispatcherQueue?.TryEnqueue(() =>
			{

				if (sender is Button btn)
				{

					switch (btn.Tag.ToString())
					{
						case "Settings":
							window.Tabs.TabItems.Add(window.CreateNewTab(typeof(SettingsPage)));
							window.SelectNewTab();
							break;
						case "Downloads":
							window.UrlBox.Text = "firebrowser://downloads";
							_ = window.TabContent.Navigate(typeof(FireBrowserWinUi3.Pages.TimeLinePages.MainTimeLine));
							break;
						case "History":
							window.UrlBox.Text = "firebrowser://history";
							_ = window.TabContent.Navigate(typeof(FireBrowserWinUi3.Pages.TimeLinePages.MainTimeLine));
							break;
						case "Favorites":
							window.UrlBox.Text = "firebrowser://favorites";
							_ = window.TabContent.Navigate(typeof(FireBrowserWinUi3.Pages.TimeLinePages.MainTimeLine));
							break;
					}
				}
			}));
		}

	}
	public Task Intialize()
	{
		UpdateUIControls();
		if (NtpTimeEnabled)
		{
			InitClock();
		}
		return Task.CompletedTask;
	}
	private void UpdateClock()
	{
		(NtpTimeText, NtpDateText) = (DateTime.Now.ToString("H:mm"), $"{DateTime.Today.DayOfWeek}, {DateTime.Today:MMMM d}");
		OnPropertyChanged(nameof(NtpTimeText));
		OnPropertyChanged(nameof(NtpDateText));
	}
	private void InitClock()
	{
		// intial time => use timer to update then after that.
		UpdateClock();
		timer = new DispatcherTimer();
		// let refresh every four seconds and allow ui to work.
		timer.Interval = new System.TimeSpan(0, 0, 4);
		timer.Tick += (_, _) =>
		{
			UpdateClock();
		};
		timer.Start();
	}

	public Settings.NewTabBackground BackgroundType
	{
		get => _backgroundType;
		set => SetProperty(ref _backgroundType, value);
	}

	public string ImageTitle
	{
		get => _imageTitle;
		set => SetProperty(ref _imageTitle, value);
	}

	public string ImageCopyright
	{
		get => _imageCopyright;
		set => SetProperty(ref _imageCopyright, value);
	}

	public string ImageCopyrightLink
	{
		get => _imageCopyrightLink;
		set => SetProperty(ref _imageCopyrightLink, value);
	}
}