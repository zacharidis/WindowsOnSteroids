using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WindowsOnSteroids.Models;
using WindowsOnSteroids.Services;

namespace WindowsOnSteroids;

public partial class MainWindow : Window
{
    private readonly TweakManager _mgr = new();
    private List<RegistryTweak> _tweaks = new();
    private Dictionary<string, CheckBox> _cbMap = new();
    private bool _suppressToggle; // prevents bulk ops double-firing OnTweakToggled

    public MainWindow()
    {
        InitializeComponent();
        ApplyTheme();
        LoadTweaks();
    }

    // ── Theming ────────────────────────────────────────────────────────────────

    private void ApplyTheme()
    {
        bool dark = ThemeManager.IsSystemDarkMode();
        var r = Application.Current.Resources;
        if (dark)
        {
            r["BgBrush"] = Brush(32, 32, 32);
            r["HeaderBrush"] = Brush(22, 22, 22);
            r["CardBrush"] = Brush(44, 44, 44);
            r["FgBrush"] = Brush(255, 255, 255);
            r["SubtleBrush"] = Brush(170, 170, 170);
            r["ToggleOffBrush"] = Brush(90, 90, 90);
            r["BtnBrush"] = Brush(60, 60, 60);
            r["CategoryFg"] = Brush(100, 180, 255);
        }
        else
        {
            r["BgBrush"] = Brush(243, 243, 243);
            r["HeaderBrush"] = Brush(255, 255, 255);
            r["CardBrush"] = Brush(255, 255, 255);
            r["FgBrush"] = Brush(30, 30, 30);
            r["SubtleBrush"] = Brush(96, 96, 96);
            r["ToggleOffBrush"] = Brush(180, 180, 180);
            r["BtnBrush"] = Brush(225, 225, 225);
            r["CategoryFg"] = Brush(0, 84, 166);
        }

        static SolidColorBrush Brush(byte r, byte g, byte b)
            => new(Color.FromRgb(r, g, b));
    }

    // ── Load & Render ──────────────────────────────────────────────────────────

    private void LoadTweaks()
    {
        _tweaks = _mgr.GetAllTweaks();

        // Seed toggle state from live registry
        foreach (var t in _tweaks)
            t.IsEnabled = _mgr.ReadTweakState(t);

        // Build UI grouped by category
        foreach (var group in _tweaks.GroupBy(t => t.Category))
        {
            var header = new TextBlock
            {
                Text = group.Key,
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(2, 16, 0, 6)
            };
            header.SetResourceReference(TextBlock.ForegroundProperty, "CategoryFg");
            TweaksPanel.Children.Add(header);

            foreach (var tweak in group)
                TweaksPanel.Children.Add(BuildCard(tweak));
        }
    }

    private Border BuildCard(RegistryTweak tweak)
    {
        var cb = new CheckBox
        {
            Style = (Style)FindResource("ToggleSwitch"),
            IsChecked = tweak.IsEnabled,
            VerticalAlignment = VerticalAlignment.Center
        };
        cb.Checked += (_, _) => OnTweakToggled(tweak, true);
        cb.Unchecked += (_, _) => OnTweakToggled(tweak, false);
        _cbMap[tweak.Id] = cb;

        var name = new TextBlock
        {
            Text = tweak.Name,
            FontSize = 14,
            FontWeight = FontWeights.Medium,
            Margin = new Thickness(0, 0, 0, 3)
        };
        name.SetResourceReference(TextBlock.ForegroundProperty, "FgBrush");

        var desc = new TextBlock
        {
            Text = tweak.Description,
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap
        };
        desc.SetResourceReference(TextBlock.ForegroundProperty, "SubtleBrush");

        var text = new StackPanel { Margin = new Thickness(14, 0, 0, 0) };
        text.Children.Add(name);
        text.Children.Add(desc);

        var row = new Grid();
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        Grid.SetColumn(cb, 0);
        Grid.SetColumn(text, 1);
        row.Children.Add(cb);
        row.Children.Add(text);

        var card = new Border
        {
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(16, 12, 16, 12),
            Margin = new Thickness(0, 0, 0, 6),
            Child = row
        };
        card.SetResourceReference(Border.BackgroundProperty, "CardBrush");
        return card;
    }

    // ── Toggle Handler ─────────────────────────────────────────────────────────

    private void OnTweakToggled(RegistryTweak tweak, bool enable)
    {
        if (_suppressToggle) return;

        tweak.IsEnabled = enable;
        bool ok = _mgr.ApplyTweak(tweak, enable);

        StatusText.Text = ok
            ? $"{tweak.Name} {(enable ? "enabled" : "disabled")}."
            : $"⚠  Failed to apply '{tweak.Name}'. Check registry permissions.";
        LastApplied.Text = $"Last change: {DateTime.Now:HH:mm:ss}";
        _mgr.SaveState(_tweaks);
    }

    // ── Bulk Actions ───────────────────────────────────────────────────────────

    private void ApplyAll_Click(object sender, RoutedEventArgs e)
    {
        int ok = BulkApply(enable: true);
        StatusText.Text = $"✓  Applied all tweaks — {ok}/{_tweaks.Count} succeeded.";
        LastApplied.Text = $"Last change: {DateTime.Now:HH:mm:ss}";
    }

    private void ResetAll_Click(object sender, RoutedEventArgs e)
    {
        if (Confirm("Disable ALL tweaks and write Windows default values back to the registry?", "Reset All"))
        {
            int ok = BulkApply(enable: false);
            StatusText.Text = $"✕  All tweaks reset — {ok}/{_tweaks.Count} reverted.";
            LastApplied.Text = $"Last change: {DateTime.Now:HH:mm:ss}";
        }
    }

    private void RestoreDefaults_Click(object sender, RoutedEventArgs e)
    {
        if (Confirm("Restore ALL settings to Windows 11 defaults?", "Restore Defaults"))
        {
            int ok = BulkApply(enable: false);
            StatusText.Text = $"↺  Windows defaults restored — {ok}/{_tweaks.Count} reverted.";
            LastApplied.Text = $"Last change: {DateTime.Now:HH:mm:ss}";
        }
    }

    /// <summary>Applies or reverts all tweaks without firing individual toggle events.</summary>
    private int BulkApply(bool enable)
    {
        _suppressToggle = true;
        int ok = 0;
        foreach (var t in _tweaks)
        {
            t.IsEnabled = enable;
            if (_cbMap.TryGetValue(t.Id, out var cb)) cb.IsChecked = enable;
            if (_mgr.ApplyTweak(t, enable)) ok++;
        }
        _suppressToggle = false;
        _mgr.SaveState(_tweaks);
        return ok;
    }

    private static bool Confirm(string msg, string title) =>
        MessageBox.Show(msg, title, MessageBoxButton.YesNo, MessageBoxImage.Warning)
            == MessageBoxResult.Yes;
}