using System.Diagnostics;
using System.Security.Principal;
using System.Windows;

namespace WindowsOnSteroids;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        if (!IsAdministrator())
        {
            var psi = new ProcessStartInfo
            {
                FileName = Process.GetCurrentProcess().MainModule!.FileName,
                UseShellExecute = true,
                Verb = "runas"
            };
            try { Process.Start(psi); }
            catch { /* User cancelled UAC */ }
            Shutdown();
            return;
        }
        base.OnStartup(e);
    }

    private static bool IsAdministrator()
    {
        var id = WindowsIdentity.GetCurrent();
        return new WindowsPrincipal(id).IsInRole(WindowsBuiltInRole.Administrator);
    }
}