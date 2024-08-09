using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
using Microsoft.Win32;
using IWshRuntimeLibrary;

namespace SimpleDogeInstaller
{
    internal class AppInstaller : IInstaller
    {
        public void Install()
        {
            Console.WriteLine("Installing Simple Doge Wallet...");
            
            Upgrade020();


            // Get the embedded resource
            Assembly assembly = Assembly.GetExecutingAssembly();

            string resourceName = "SimpleDogeInstaller.dist.sdw-release.zip";
            Stream resourceStream = assembly.GetManifestResourceStream(resourceName);

            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            // Assume 'zipStream' is your file stream
            using (var zip = new ZipArchive(resourceStream, ZipArchiveMode.Read))
            {
                foreach (var entry in zip.Entries)
                {
                    string filePath = string.Empty;

                    if(string.IsNullOrWhiteSpace(Path.GetDirectoryName(entry.FullName)) &&
                        entry.FullName != "Icon.ico")
                    {
                        filePath = Path.Combine(programFiles, @"SimpleDogeWallet", entry.FullName);
                    }
                    else
                    {
                        filePath = Path.Combine(localAppData, @"SimpleDogeWallet", entry.FullName);
                    }


                    string directoryPath = Path.GetDirectoryName(filePath);
                    
                    
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }
                    if (entry.Name == "")
                    {
                        Directory.CreateDirectory(filePath);
                    }
                    else
                    {
                        Console.WriteLine("Installing... " + Path.GetFileName(filePath));
                        entry.ExtractToFile(filePath, true);
                    }
                }
            }


            Console.WriteLine("Creating StartMenu and Startup links...");
            // Create the shortcut
            WshShell shell = new WshShell();
            string shortcutAddress = Path.Combine(localAppData, "SimpleDogeWallet\\Simple Ðoge Wallet.lnk");
            IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutAddress);
           
            // Set the shortcut properties
            shortcut.TargetPath = Path.Combine(programFiles, @"SimpleDogeWallet\SimpleDogeWallet.exe");
            shortcut.WorkingDirectory = Path.Combine(localAppData, @"SimpleDogeWallet\");
            shortcut.Description = "Simple Doge Wallet";
            shortcut.IconLocation = Path.Combine(localAppData, @"SimpleDogeWallet\Icon.ico");
            shortcut.Save();

            System.IO.File.Copy(shortcutAddress, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Simple Ðoge Wallet.lnk"), true);

            shortcut.Arguments = "-h";
            shortcut.Save();

            System.IO.File.Copy(shortcutAddress, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "Simple Ðoge Wallet.lnk"), true);

        }

        public void Uninstall()
        {
            Console.WriteLine("Removing Startup & StartMenu links...");
            
            FileHelper.TryDeleteFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Simple Ðoge Wallet.lnk"));
            FileHelper.TryDeleteFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "Simple Ðoge Wallet.lnk"));

            Console.WriteLine("Removing application files...");

            FileHelper.TryDeleteDirectoryContents(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"SimpleDogeWallet\"), "SimpleDogeInstaller.exe");
            FileHelper.TryDeleteDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"SimpleDogeWallet\"));

            Console.WriteLine("Done removing files...");
        }


        #region Upgrade Logic

        public void Upgrade020()
        {
            Console.WriteLine("Performing 0.2.0->future Directory Adjustments...");

            // 0.2.0 installed to localappdata, but we moved it to programfiles, so lets delete the localappdata.

            var localAppDataInstall = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"SimpleDogeWallet\");

            if (!Directory.Exists(localAppDataInstall))
            {
                return;
            }

            var files = Directory.GetFiles(localAppDataInstall);

            string[] filesToKeep = new[] { "utxos", "terminalsettings.json", "spvcheckpoint", "loadedmnemonic", "address" };

            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);

                // Check if the file is not in the list of files to keep
                if (!filesToKeep.Contains(fileName))
                {
                    FileHelper.TryDeleteFile(file);
                }
            }

            var directories = Directory.GetDirectories(localAppDataInstall);

            foreach (var directory in directories)
            {
                var directoryName = Path.GetFileName(directory);

                if (directoryName != "store")
                {
                    FileHelper.TryDeleteDirectory(directory);
                }
            }
        }

#endregion


}
}
