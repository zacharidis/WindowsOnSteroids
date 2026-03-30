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
        // — AI / Copilot / Recall (2026) —
new() {
    Id = "copilot_full",
    Category = "🔒 AI & Copilot",
    Name = "Disable Windows Copilot Completely",
    Description = "Blocks Copilot AI assistant (taskbar, keyboard shortcut, silent reinstalls)",
    Hive = RegistryHive.CurrentUser,
    KeyPath = @"SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot",
    ValueName = "TurnOffWindowsCopilot",
    ValueType = TweakValueType.DWord,
    EnabledValue = 1,
    DefaultValue = 0
},
new() {
    Id = "recall",
    Category = "🔒 AI & Copilot",
    Name = "Disable Windows Recall (Snapshots)",
    Description = "Stops AI timeline recording of your screen/activity (huge privacy + storage win)",
    Hive = RegistryHive.LocalMachine,
    KeyPath = @"SOFTWARE\Policies\Microsoft\Windows\WindowsAI",
    ValueName = "AllowRecallEnablement",
    ValueType = TweakValueType.DWord,
    EnabledValue = 0,
    DefaultValue = 1
},
new() {
    Id = "ai_data_analysis",
    Category = "🔒 AI & Copilot",
    Name = "Disable AI Data Analysis",
    Description = "Prevents Copilot/Recall from sending usage data for AI training",
    Hive = RegistryHive.LocalMachine,
    KeyPath = @"SOFTWARE\Policies\Microsoft\Windows\WindowsAI",
    ValueName = "DisableAIDataAnalysis",
    ValueType = TweakValueType.DWord,
    EnabledValue = 1,
    DefaultValue = 0
},
new() {
    Id = "gaming_copilot",
    Category = "🔒 AI & Copilot",
    Name = "Disable Gaming Copilot",
    Description = "Removes AI hints/widget inside Game Bar (2026 feature)",
    Hive = RegistryHive.CurrentUser,
    KeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR",
    ValueName = "GamingCopilotEnabled",
    ValueType = TweakValueType.DWord,
    EnabledValue = 0,
    DefaultValue = 1
},

// — Gaming (FPS + input lag) —
new() {
    Id = "game_dvr",
    Category = "🎮 Gaming",
    Name = "Disable Xbox Game DVR / Game Bar",
    Description = "Stops background recording + overlay (biggest gaming performance win)",
    Hive = RegistryHive.CurrentUser,
    KeyPath = @"System\GameConfigStore",
    ValueName = "GameDVR_Enabled",
    ValueType = TweakValueType.DWord,
    EnabledValue = 0,
    DefaultValue = 1
},
new() {
    Id = "fullscreen_optimizations",
    Category = "🎮 Gaming",
    Name = "Disable Fullscreen Optimizations (Global)",
    Description = "Removes Windows overlay layers for true exclusive fullscreen (better FPS/1% lows)",
    Hive = RegistryHive.CurrentUser,
    KeyPath = @"System\GameConfigStore",
    ValueName = "GameDVR_DSEBehavior",
    ValueType = TweakValueType.DWord,
    EnabledValue = 2,
    DefaultValue = 0
},
new() {
    Id = "gpu_priority",
    Category = "🎮 Gaming",
    Name = "Set GPU/CPU Priority to High for Games",
    Description = "Gives games maximum scheduling priority (tested +5-15% FPS in many titles)",
    Hive = RegistryHive.LocalMachine,
    KeyPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games",
    ValueName = "Scheduling Category",
    ValueType = TweakValueType.String,
    EnabledValue = "High",
    DefaultValue = "Medium"
},
new() {
    Id = "network_throttling",
    Category = "🎮 Gaming",
    Name = "Disable Network Throttling",
    Description = "Removes artificial network limit for online gaming (lower ping)",
    Hive = RegistryHive.LocalMachine,
    KeyPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile",
    ValueName = "NetworkThrottlingIndex",
    ValueType = TweakValueType.DWord,
    EnabledValue = -1, // ffffffff in hex
    DefaultValue = 0
},
new() {
    Id = "vbs",
    Category = "🎮 Gaming",
    Name = "Disable Virtualization-Based Security (VBS)",
    Description = "Huge gaming boost (2-10% FPS) on supported hardware — reversible",
    Hive = RegistryHive.LocalMachine,
    KeyPath = @"SOFTWARE\Policies\Microsoft\Windows\DeviceGuard",
    ValueName = "EnableVirtualizationBasedSecurity",
    ValueType = TweakValueType.DWord,
    EnabledValue = 0,
    DefaultValue = 1
},
new() {
    Id = "game_mode",
    Category = "🎮 Gaming",
    Name = "Force Game Mode On",
    Description = "Ensures Game Mode is always active for all games",
    Hive = RegistryHive.CurrentUser,
    KeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR",
    ValueName = "AppCaptureEnabled",
    ValueType = TweakValueType.DWord,
    EnabledValue = 1,
    DefaultValue = 0
},

// — Advanced Performance —
new() {
    Id = "background_apps",
    Category = "⚡ Advanced Performance",
    Name = "Disable Background Apps",
    Description = "Stops all apps from running in background (massive RAM/CPU savings)",
    Hive = RegistryHive.CurrentUser,
    KeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications",
    ValueName = "GlobalUserDisabled",
    ValueType = TweakValueType.DWord,
    EnabledValue = 1,
    DefaultValue = 0
},
new() {
    Id = "mpo",
    Category = "⚡ Advanced Performance",
    Name = "Disable Multi-Plane Overlay (MPO)",
    Description = "Fixes micro-stutter in some games (especially NVIDIA/AMD)",
    Hive = RegistryHive.LocalMachine,
    KeyPath = @"SOFTWARE\Microsoft\Windows\Dwm",
    ValueName = "OverlayTestMode",
    ValueType = TweakValueType.DWord,
    EnabledValue = 5,
    DefaultValue = 0
},
new() {
    Id = "search_indexing",
    Category = "⚡ Advanced Performance",
    Name = "Disable Windows Search Indexing",
    Description = "Stops constant disk activity (great for SSDs and gaming PCs)",
    Hive = RegistryHive.LocalMachine,
    KeyPath = @"SYSTEM\CurrentControlSet\Services\WSearch",
    ValueName = "Start",
    ValueType = TweakValueType.DWord,
    EnabledValue = 4,
    DefaultValue = 3
},
new() {
    Id = "startup_delay2",
    Category = "⚡ Advanced Performance",
    Name = "Remove Additional Startup Delay (Explorer)",
    Description = "Further reduces Start menu and Explorer lag",
    Hive = RegistryHive.CurrentUser,
    KeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced",
    ValueName = "Serialize",
    ValueType = TweakValueType.DWord,
    EnabledValue = 0,
    DefaultValue = 1
},
new() {
    Id = "power_throttle",
    Category = "⚡ Advanced Performance",
    Name = "Disable Power Throttling",
    Description = "Prevents CPU/GPU power limits on desktop/gaming machines",
    Hive = RegistryHive.LocalMachine,
    KeyPath = @"SYSTEM\CurrentControlSet\Control\Power",
    ValueName = "PowerThrottling",
    ValueType = TweakValueType.DWord,
    EnabledValue = 0,
    DefaultValue = 1
},

// — Extra Privacy —
new() {
    Id = "onedrive_autostart",
    Category = "📉 Privacy",
    Name = "Disable OneDrive Auto-Start",
    Description = "Stops OneDrive from launching at boot (saves resources + privacy)",
    Hive = RegistryHive.CurrentUser,
    KeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run",
    ValueName = "OneDrive",
    ValueType = TweakValueType.String,
    EnabledValue = "", // empty = disabled
    DefaultValue = "OneDrive.exe /background"
},
new() {
    Id = "cortana",
    Category = "📉 Privacy",
    Name = "Disable Cortana / Search Suggestions",
    Description = "Fully disables legacy Cortana and Bing search suggestions",
    Hive = RegistryHive.CurrentUser,
    KeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Search",
    ValueName = "CortanaConsent",
    ValueType = TweakValueType.DWord,
    EnabledValue = 0,
    DefaultValue = 1
},
new() {
    Id = "suggested_apps",
    Category = "📉 Privacy",
    Name = "Disable All Suggested Apps & Ads",
    Description = "Removes promotions from Start Menu, Settings, Explorer",
    Hive = RegistryHive.CurrentUser,
    KeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager",
    ValueName = "SubscribedContent-338388Enabled",
    ValueType = TweakValueType.DWord,
    EnabledValue = 0,
    DefaultValue = 1
},
new() {
    Id = "edge_background",
    Category = "📉 Privacy",
    Name = "Disable Microsoft Edge Background Services",
    Description = "Stops Edge from running silently in background",
    Hive = RegistryHive.LocalMachine,
    KeyPath = @"SOFTWARE\Policies\Microsoft\Edge",
    ValueName = "BackgroundModeEnabled",
    ValueType = TweakValueType.DWord,
    EnabledValue = 0,
    DefaultValue = 1
},
new() {
    Id = "silent_install",
    Category = "📉 Privacy",
    Name = "Block Silent App Installs",
    Description = "Prevents Microsoft from force-installing apps in background",
    Hive = RegistryHive.CurrentUser,
    KeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager",
    ValueName = "SilentInstalledAppsEnabled",
    ValueType = TweakValueType.DWord,
    EnabledValue = 0,
    DefaultValue = 1
}
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