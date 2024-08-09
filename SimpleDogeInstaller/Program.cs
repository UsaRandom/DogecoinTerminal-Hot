using SimpleDogeInstaller;
using System.Diagnostics;
using System.Runtime.CompilerServices;

internal class Program
{

    private static List<IInstaller> _installers = new List<IInstaller>();

    private static void Main(string[] args)
    {
        Process currentProcess = Process.GetCurrentProcess();
        Process[] processes = Process.GetProcessesByName(currentProcess.ProcessName);

        foreach (var otherProcess in processes)
        {
            if (otherProcess.Id != currentProcess.Id)
            {
                Console.WriteLine("Installer already open! Closing...");
                return;
            }
        }

        _installers.Add(new VcRedist2015Installer());
        _installers.Add(new AppInstaller());
        _installers.Add(new UninstallerInstaller());



        if (args.Length == 0 || (args.Length > 0 && args[0].ToLower().StartsWith("-i")))
        {
            Install();
        }
        else if (args.Length > 0 && args[0].ToLower().StartsWith("-u"))
        {
            Uninstall();
        }
        else
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("-u         Uninstall");
            Console.WriteLine("-i         Install (default)");
        }




    }

    private static void KillSimpleDogeWalletIfRunning()
    {
        var wallet = Process.GetProcessesByName("SimpleDogeWallet").FirstOrDefault();

        if(wallet != null)
        {
            wallet.Kill();
        }
    }

    public static void StartSimpleDogeWallet()
    {
        string appExeLoc = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"SimpleDogeWallet\SimpleDogeWallet.exe");
        ProcessStartInfo startInfo = new ProcessStartInfo(appExeLoc);

        startInfo.WorkingDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"SimpleDogeWallet");

        Process.Start(startInfo);
    }

    private static void Install()
    {
        Console.WriteLine(@"
Simple Doge Wallet
v0.3.0 - Beta

A decentralized Dogecoin wallet.

Press [Enter] to Install...
");
        Console.ReadLine();

        KillSimpleDogeWalletIfRunning();

        foreach(var installer in _installers)
        {
            installer.Install();
        }
        
        StartSimpleDogeWallet();
    }


    private static void Uninstall()
    {
        KillSimpleDogeWalletIfRunning();

        foreach (var installer in _installers)
        {
            installer.Uninstall();
        }
    }
}