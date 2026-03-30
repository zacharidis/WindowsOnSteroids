using Microsoft.Win32;

namespace WindowsOnSteroids.Models;

public enum TweakValueType { DWord, String }

public class RegistryTweak
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Category { get; set; } = "";

    public RegistryHive Hive { get; set; }
    public string KeyPath { get; set; } = "";
    public string ValueName { get; set; } = "";
    public TweakValueType ValueType { get; set; }

    // Value written when tweak is ON
    public object EnabledValue { get; set; } = 0;
    // Value written when tweak is OFF (Windows default)
    public object DefaultValue { get; set; } = 1;

    public bool IsEnabled { get; set; }
}