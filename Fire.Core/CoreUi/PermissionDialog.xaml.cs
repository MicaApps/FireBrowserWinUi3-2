using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;


namespace Fire.Core.CoreUi;

public sealed partial class PermissionDialog : ContentDialog
{
	public string DialogTitle { get; set; }
	public string ManageText { get; set; }

	public PermissionDialog(string title, string manageText)
	{
		this.InitializeComponent();
		DialogTitle = title;
		ManageText = manageText;
	}
}
