using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Reflection;

namespace SimpleDogeInstaller
{
    public class VcRedist2015Installer : IInstaller
    {
        public void Install()
        {

            Console.WriteLine("Installing VC++ 2015 Redist...");

            // Get the embedded resource
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = "SimpleDogeInstaller.dist.vc_redist.x64.exe";
            Stream resourceStream = assembly.GetManifestResourceStream(resourceName);

            // Extract the resource to a temporary file
            string tempFilePath = Path.GetTempFileName();
            using (FileStream fileStream = File.Create(tempFilePath))
            {
                resourceStream.CopyTo(fileStream);
            }

            

            string installerArguments = "/install /passive /norestart";

            Process process = new Process();
            process.StartInfo.FileName = tempFilePath;
            process.StartInfo.Arguments = installerArguments;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;

            // Start the process
            process.Start();

            // Wait for the process to complete
            process.WaitForExit();

            // Check the exit code
            if (process.ExitCode == 0)
            {
                Console.WriteLine("VC++ 2015 Redistributable installed successfully.");
            }
            else
            {
                Console.WriteLine("Error installing VC++ 2015 Redistributable: " + process.ExitCode);
            }
        }

        public void Uninstall()
        {

        }
    
    }
}
