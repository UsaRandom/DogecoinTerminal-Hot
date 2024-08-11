using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Lib.Dogecoin
{
    internal class LibdogecoinDllSelector
    {
        private static bool _isTpmSupported = false;

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool SetDllDirectory(string lpPathName);

        static LibdogecoinDllSelector()
        {
            CheckTpmSupported();
        }

        public static void Select()
        {
            if (!IsTpmSupported)
            {
                var exeloc = Path.GetDirectoryName(Environment.ProcessPath);
                var path = Path.Combine(exeloc, "libdogecoin", "no-tpm\\");
                SetDllDirectory(path);
            }
            else
            {
                var path = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath), "libdogecoin", "no-tpm\\");
                SetDllDirectory(path);
            }
        }

        public static bool IsTpmSupported
        {
            get
            {
                return _isTpmSupported;
            }
        }
        private static void CheckTpmSupported()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo("tpmtool", "getdeviceinformation");
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;

            Process process = new Process();
            process.StartInfo = startInfo;
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            string[] lines = output.Split('\n');
            bool isLockedOut = false;
            bool readyForStorage = false;
            bool tpmPresent = false;

            foreach (string line in lines)
            {
                string[] parts = line.Split(':');
                if (parts.Length > 1)
                {
                    string key = parts[0].Trim();
                    string value = parts[1].Trim();

                    if (key.Equals("-Is Locked Out", StringComparison.OrdinalIgnoreCase))
                    {
                        isLockedOut = value.Equals("True", StringComparison.OrdinalIgnoreCase);
                    }
                    else if (key.Equals("-Ready For Storage", StringComparison.OrdinalIgnoreCase))
                    {
                        readyForStorage = value.Equals("True", StringComparison.OrdinalIgnoreCase);
                    }
                    else if (key.Equals("-TPM Present", StringComparison.OrdinalIgnoreCase))
                    {
                        tpmPresent = value.Equals("True", StringComparison.OrdinalIgnoreCase);
                    }
                }
            }

            _isTpmSupported = !isLockedOut && tpmPresent && readyForStorage;
        }
    }
}
