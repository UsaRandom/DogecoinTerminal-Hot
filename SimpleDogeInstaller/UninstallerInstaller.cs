using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleDogeInstaller
{
    internal class UninstallerInstaller : IInstaller
    {
        public void Install()
        {
            var currentExecutablePath = Process.GetCurrentProcess().MainModule.FileName;

            var installPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"SimpleDogeWallet\");
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"SimpleDogeWallet\");

            var destinationPath = Path.Combine(installPath, Path.GetFileName(currentExecutablePath));

            File.Copy(currentExecutablePath, destinationPath);



            // Create a registry key for the uninstaller
            RegistryKey uninstallKey = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\SimpleDogeWallet");

            uninstallKey.SetValue("DisplayName", "Simple Doge Wallet");
            uninstallKey.SetValue("UninstallString", Path.Combine(installPath, "SimpleDogeInstaller.exe") + " -u");
            uninstallKey.SetValue("DisplayIcon", Path.Combine(appDataPath, "Icon.ico"));
            uninstallKey.SetValue("DisplayVersion", "0.3.0");
            //uninstallKey.SetValue("Publisher", "Simple Doge");
            //uninstallKey.SetValue("URLInfoAbout", "https://www.simpledogewallet.com");
            //uninstallKey.SetValue("URLUpdateInfo", "https://www.simpledogewallet.com/updates");
            uninstallKey.SetValue("NoModify", 1);
            uninstallKey.SetValue("NoRepair", 1);

            uninstallKey.Close();

        }

        public void Uninstall()
        {
            RegistryKey uninstallKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\SimpleDogeWallet", true);
            try
            {
                uninstallKey.DeleteSubKeyTree("SimpleDogeWallet");
            }
            catch {  }



            //delete current executable
            var installPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"SimpleDogeWallet\");

            // Start a new process to delete the current executable after a delay
            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"cmd /c timeout /t 2 & rmdir /s /q \"{installPath}\"" + Path.DirectorySeparatorChar + "",
                UseShellExecute = false,
                CreateNoWindow = false
            });

            // Exit the current process
            Environment.Exit(0);
        }
    }
}
