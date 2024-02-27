﻿using System;
using System.Text.Json.Serialization;

namespace FireBrowserWinUi3MultiCore;
public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public bool IsFirstLaunch { get; set; }

    [JsonPropertyName("UserSettings")]
    public Settings UserSettings { get; set; }
}