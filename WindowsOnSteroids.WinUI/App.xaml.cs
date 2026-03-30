using Microsoft.UI.Xaml;
using System;
using System.Diagnostics;
using System.Security.Principal;

namespace WindowsOnSteroids.WinUI
{
    public partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            if (!IsAdministrator())
            {
                var psi = new ProcessStartInfo
                {
                    FileName = Process.GetCurrentProcess().MainModule!.FileName,
                    UseShellExecute = true,
                    Verb = "runas"
                };
                try
                {
                    Process.Start(psi);
                    Environment.Exit(0);
                }
                catch { Environment.Exit(0); }
            }

            m_window = new MainWindow();
            m_window.Activate();
        }

        private static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private Window m_window;
    }
}