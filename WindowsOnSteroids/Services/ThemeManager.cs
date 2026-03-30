using Microsoft.Win32;

namespace WindowsOnSteroids.Services;

public static class ThemeManager
{
    /// <summary>Returns true if Windows Apps are in Dark Mode.</summary>
    public static bool IsSystemDarkMode()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            return key?.GetValue("AppsUseLightTheme") is int v && v == 0;
        }
        catch { return false; }
    }
}