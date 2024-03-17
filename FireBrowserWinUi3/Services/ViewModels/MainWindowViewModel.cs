﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI.Behaviors;
using FireBrowserWinUi3.Services.Messages;
using FireBrowserWinUi3MultiCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace FireBrowserWinUi3.Services.ViewModels;

public partial class MainWindowViewModel : ObservableRecipient
{
    internal MainWindow MainView { get; set;  }
    public MainWindowViewModel(IMessenger messenger) : base(messenger)
    {
        Messenger.Register<Message_Settings_Actions>(this, ReceivedStatus);

    }

    private void ReceivedStatus(object recipient, Message_Settings_Actions message)
    {
        switch (message.Status)
        {
            case EnumMessageStatus.Added:
                break;
            case EnumMessageStatus.Login:
                var note = new Notification
                {
                    Title = "FireBroswerWinUi3 \n",
                    Message = $"Welcomes, {AuthService.CurrentUser.Username.ToUpperInvariant()} !",
                    Severity = InfoBarSeverity.Informational,
                    IsIconVisible = true,
                    Duration = TimeSpan.FromSeconds(3),

                };
                MainView.NotificationQueue.Show(note);
                break;
            case EnumMessageStatus.Logout:
                break;
            case EnumMessageStatus.Removed:
                break;
            case EnumMessageStatus.Settings:
                MainView.LoadUserSettings();    
                break;
            case EnumMessageStatus.Updated:
                break;
            default:
                break;
        }
        
        


    }
}
