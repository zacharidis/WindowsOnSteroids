using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using WindowsOnSteroids.Models;
using WindowsOnSteroids.Services;

namespace WindowsOnSteroids.WinUI
{
    public sealed partial class MainWindow : Window
    {
        private readonly TweakManager _mgr = new();
        private List<RegistryTweak> _tweaks = new();
        private Dictionary<string, ToggleSwitch> _toggleMap = new();
        private bool _suppressToggle;

        public MainWindow()
        {
            this.InitializeComponent();
            ApplyTheme();
            LoadTweaks();
        }

        private void ApplyTheme()
        {
            // WinUI 3 automatically follows system theme – Mica is already set in XAML
        }

        private void LoadTweaks()
        {
            _tweaks = _mgr.GetAllTweaks();

            // Seed state from registry
            foreach (var t in _tweaks)
                t.IsEnabled = _mgr.ReadTweakState(t);

            // Build UI grouped by category
            foreach (var group in _tweaks.GroupBy(t => t.Category))
            {
                // Category header
                var header = new TextBlock
                {
                    Text = group.Key,
                    FontSize = 18,
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                    Margin = new Thickness(0, 8, 0, 8)
                };
                TweaksPanel.Children.Add(header);

                foreach (var tweak in group)
                {
                    TweaksPanel.Children.Add(BuildCard(tweak));
                }
            }
        }

        private UIElement BuildCard(RegistryTweak tweak)
        {
            var ts = new ToggleSwitch
            {
                IsOn = tweak.IsEnabled,
                OnContent = "ON",
                OffContent = "OFF",
                Margin = new Thickness(0, 0, 12, 0)
            };

            ts.Toggled += (s, e) => OnTweakToggled(tweak, ts.IsOn);
            _toggleMap[tweak.Id] = ts;

            var name = new TextBlock { Text = tweak.Name, FontSize = 15, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold };
            var desc = new TextBlock { Text = tweak.Description, FontSize = 13, Opacity = 0.8, TextWrapping = TextWrapping.Wrap };

            var contentGrid = new Grid { ColumnSpacing = 12 };
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            Grid.SetColumn(ts, 0);
            Grid.SetColumn(name, 1);
            Grid.SetColumn(desc, 1);

            contentGrid.Children.Add(ts);
            contentGrid.Children.Add(name);
            contentGrid.Children.Add(desc);

            var card = new Card
            {
                Padding = new Thickness(16),
                Margin = new Thickness(0, 0, 0, 8),
                Child = contentGrid
            };

            return card;
        }

        private void OnTweakToggled(RegistryTweak tweak, bool enable)
        {
            if (_suppressToggle) return;

            tweak.IsEnabled = enable;
            bool ok = _mgr.ApplyTweak(tweak, enable);

            StatusText.Text = ok
                ? $"✅ {tweak.Name} {(enable ? "enabled" : "disabled")}"
                : $"❌ Failed to apply '{tweak.Name}'";

            LastApplied.Text = $"Last change: {DateTime.Now:HH:mm:ss}";
            _mgr.SaveState(_tweaks);
        }

        // Bulk actions (same as original)
        private void ApplyAll_Click(object sender, RoutedEventArgs e) => BulkApply(true);
        private void ResetAll_Click(object sender, RoutedEventArgs e) => BulkApply(false);
        private void RestoreDefaults_Click(object sender, RoutedEventArgs e)
        {
            if (ContentDialogHelper.ShowConfirm(this, "Restore ALL settings to Windows 11 defaults?", "Restore Defaults"))
                BulkApply(false);
        }

        private void BulkApply(bool enable)
        {
            _suppressToggle = true;
            int ok = 0;
            foreach (var t in _tweaks)
            {
                t.IsEnabled = enable;
                if (_toggleMap.TryGetValue(t.Id, out var ts))
                    ts.IsOn = enable;
                if (_mgr.ApplyTweak(t, enable)) ok++;
            }
            _suppressToggle = false;
            _mgr.SaveState(_tweaks);

            StatusText.Text = enable
                ? $"🚀 Applied all tweaks – {ok}/{_tweaks.Count} succeeded"
                : $"❌ All tweaks reset – {ok}/{_tweaks.Count} reverted";
            LastApplied.Text = $"Last change: {DateTime.Now:HH:mm:ss}";
        }
    }

    // Helper for confirm dialog (WinUI style)
    public static class ContentDialogHelper
    {
        public static bool ShowConfirm(Window window, string message, string title)
        {
            // Simple MessageBox equivalent for WinUI 3
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                PrimaryButtonText = "Yes",
                SecondaryButtonText = "No",
                XamlRoot = window.Content.XamlRoot
            };
            var result = dialog.ShowAsync().GetAwaiter().GetResult();
            return result == ContentDialogResult.Primary;
        }
    }
}