using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using WindowsOnSteroids.Models;

namespace WindowsOnSteroids.Services;

public class TweakManager
{
    private static readonly string SavePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "WindowsOnSteroids", "tweaks.json");

    // ── Tweak Definitions ────────────────────────────────────────────────────

    public List<RegistryTweak> GetAllTweaks() => new()
    {
        // ── Privacy ──────────────────────────────────────────────────────────
        new() {
            Id = "telemetry", Category = "🔒  Privacy",
            Name = "Disable Telemetry",
            Description = "Prevents Windows from sending usage and diagnostic data to Microsoft servers.",
            Hive = RegistryHive.LocalMachine,
            KeyPath = @"SOFTWARE\Policies\Microsoft\Windows\DataCollection",
            ValueName = "AllowTelemetry",
            ValueType = TweakValueType.DWord,
            EnabledValue = 0, DefaultValue = 1
        },
        new() {
            Id = "advertising_id", Category = "🔒  Privacy",
            Name = "Disable Advertising ID",
            Description = "Stops apps from using a unique advertising ID to show you personalised ads.",
            Hive = RegistryHive.CurrentUser,
            KeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\AdvertisingInfo",
            ValueName = "Enabled",
            ValueType = TweakValueType.DWord,
            EnabledValue = 0, DefaultValue = 1
        },
        new() {
            Id = "activity_history", Category = "🔒  Privacy",
            Name = "Disable Activity History",
            Description = "Prevents Windows from tracking apps, files and websites you've accessed.",
            Hive = RegistryHive.LocalMachine,
            KeyPath = @"SOFTWARE\Policies\Microsoft\Windows\System",
            ValueName = "EnableActivityFeed",
            ValueType = TweakValueType.DWord,
            EnabledValue = 0, DefaultValue = 1
        },
        new() {
            Id = "location", Category = "🔒  Privacy",
            Name = "Disable Location Tracking",
            Description = "Denies all apps access to your physical location at the system level.",
            Hive = RegistryHive.LocalMachine,
            KeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\location",
            ValueName = "Value",
            ValueType = TweakValueType.String,
            EnabledValue = "Deny", DefaultValue = "Allow"
        },

        // ── Performance ───────────────────────────────────────────────────────
        new() {
            Id = "animations", Category = "⚡  Performance",
            Name = "Disable Window Animations",
            Description = "Turns off minimize/maximize animations so windows open and close instantly.",
            Hive = RegistryHive.CurrentUser,
            KeyPath = @"Control Panel\Desktop\WindowMetrics",
            ValueName = "MinAnimate",
            ValueType = TweakValueType.String,
            EnabledValue = "0", DefaultValue = "1"
        },
        new() {
            Id = "transparency", Category = "⚡  Performance",
            Name = "Disable Transparency Effects",
            Description = "Removes frosted-glass blur from the taskbar and windows, reducing GPU load.",
            Hive = RegistryHive.CurrentUser,
            KeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize",
            ValueName = "EnableTransparency",
            ValueType = TweakValueType.DWord,
            EnabledValue = 0, DefaultValue = 1
        },
        new() {
            Id = "visual_effects", Category = "⚡  Performance",
            Name = "Best Performance Visual Effects",
            Description = "Tells Windows to prioritise speed over eye-candy (shadows, smooth fonts, etc.).",
            Hive = RegistryHive.CurrentUser,
            KeyPath = @"Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects",
            ValueName = "VisualFXSetting",
            ValueType = TweakValueType.DWord,
            EnabledValue = 2, DefaultValue = 0
        },
        new() {
            Id = "startup_delay", Category = "⚡  Performance",
            Name = "Remove Startup Delay",
            Description = "Eliminates the artificial delay Windows adds before loading startup programs.",
            Hive = RegistryHive.CurrentUser,
            KeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Serialize",
            ValueName = "StartupDelayInMSec",
            ValueType = TweakValueType.DWord,
            EnabledValue = 0, DefaultValue = 1
        },

        // ── UI / Taskbar ──────────────────────────────────────────────────────
        new() {
            Id = "widgets", Category = "🎨  UI / Taskbar",
            Name = "Remove Widgets Button",
            Description = "Hides the Widgets (news & weather) button from the taskbar.",
            Hive = RegistryHive.CurrentUser,
            KeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced",
            ValueName = "TaskbarDa",
            ValueType = TweakValueType.DWord,
            EnabledValue = 0, DefaultValue = 1
        },
        new() {
            Id = "chat_button", Category = "🎨  UI / Taskbar",
            Name = "Remove Chat / Teams Button",
            Description = "Removes the Microsoft Teams Chat shortcut pinned to the taskbar.",
            Hive = RegistryHive.CurrentUser,
            KeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced",
            ValueName = "TaskbarMn",
            ValueType = TweakValueType.DWord,
            EnabledValue = 0, DefaultValue = 1
        },
        new() {
            Id = "task_view", Category = "🎨  UI / Taskbar",
            Name = "Remove Task View Button",
            Description = "Hides the Task View (virtual desktops) button from the taskbar.",
            Hive = RegistryHive.CurrentUser,
            KeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced",
            ValueName = "ShowTaskViewButton",
            ValueType = TweakValueType.DWord,
            EnabledValue = 0, DefaultValue = 1
        },
        new() {
            Id = "search_box", Category = "🎨  UI / Taskbar",
            Name = "Hide Taskbar Search Box",
            Description = "Collapses the search box so it doesn't occupy taskbar space.",
            Hive = RegistryHive.CurrentUser,
            KeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Search",
            ValueName = "SearchboxTaskbarMode",
            ValueType = TweakValueType.DWord,
            EnabledValue = 0, DefaultValue = 1
        },

        // ── Miscellaneous ─────────────────────────────────────────────────────
        new() {
            Id = "windows_tips", Category = "💡  Miscellaneous",
            Name = "Disable Windows Tips & Suggestions",
            Description = "Stops Windows from showing tips, tricks, and welcome messages on the desktop.",
            Hive = RegistryHive.CurrentUser,
            KeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager",
            ValueName = "SoftLandingEnabled",
            ValueType = TweakValueType.DWord,
            EnabledValue = 0, DefaultValue = 1
        },
        new() {
            Id = "suggested_content", Category = "💡  Miscellaneous",
            Name = "Disable Suggested Content in Settings",
            Description = "Prevents Windows from showing sponsored apps and promotions inside the Settings app.",
            Hive = RegistryHive.CurrentUser,
            KeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager",
            ValueName = "SubscribedContent-338393Enabled",
            ValueType = TweakValueType.DWord,
            EnabledValue = 0, DefaultValue = 1
        },
        new() {
            Id = "copilot", Category = "💡  Miscellaneous",
            Name = "Disable Copilot Button",
            Description = "Removes the Copilot AI button from the taskbar.",
            Hive = RegistryHive.CurrentUser,
            KeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced",
            ValueName = "ShowCopilotButton",
            ValueType = TweakValueType.DWord,
            EnabledValue = 0, DefaultValue = 1
        },
    };

    // ── Registry Read / Write ─────────────────────────────────────────────────

    public bool ReadTweakState(RegistryTweak t)
    {
        try
        {
            using var root = RegistryKey.OpenBaseKey(t.Hive, RegistryView.Registry64);
            using var key = root.OpenSubKey(t.KeyPath);
            if (key == null) return false;
            var val = key.GetValue(t.ValueName);
            return val != null && val.ToString() == t.EnabledValue.ToString();
        }
        catch { return false; }
    }

    public bool ApplyTweak(RegistryTweak t, bool enable)
    {
        try
        {
            using var root = RegistryKey.OpenBaseKey(t.Hive, RegistryView.Registry64);
            using var key = root.CreateSubKey(t.KeyPath, writable: true);
            var value = enable ? t.EnabledValue : t.DefaultValue;
            if (t.ValueType == TweakValueType.DWord)
                key.SetValue(t.ValueName, (int)value, RegistryValueKind.DWord);
            else
                key.SetValue(t.ValueName, value.ToString()!, RegistryValueKind.String);
            return true;
        }
        catch { return false; }
    }

    // ── JSON Persistence ──────────────────────────────────────────────────────

    public void SaveState(List<RegistryTweak> tweaks)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(SavePath)!);
        var state = new Dictionary<string, bool>();
        foreach (var t in tweaks) state[t.Id] = t.IsEnabled;
        File.WriteAllText(SavePath, JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true }));
    }

    public Dictionary<string, bool> LoadState()
    {
        if (!File.Exists(SavePath)) return new();
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, bool>>(File.ReadAllText(SavePath)) ?? new();
        }
        catch { return new(); }
    }
}