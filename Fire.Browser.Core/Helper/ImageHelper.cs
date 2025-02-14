﻿using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;

namespace Fire.Browser.Core.Helper;
public class ImageHelper : MarkupExtension
{
	private static readonly Dictionary<string, BitmapImage> ImageCache = new();

	public string ImageName { get; set; }

	protected override object ProvideValue() => LoadImage(ImageName);

	public BitmapImage LoadImage(string imageName) =>
		string.IsNullOrEmpty(imageName) ? null : ImageCache.TryGetValue(imageName, out var cachedImage)
			? cachedImage
			: ImageCache[imageName] = new BitmapImage(new Uri($"ms-appx:///Fire.Browser.Core//Assets/{imageName}"));
}