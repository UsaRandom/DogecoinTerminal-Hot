
using Lib.Dogecoin;
using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.CompilerServices;

namespace SimpleDogeWallet.WinForms
{
	internal class Program
	{
		private const string PipeName = "SimpleDogeWalletReOpenRequest"; // replace with a unique name

		private static SimpleDogeWalletWinFormGame _game;
		private static System.Threading.Timer _timer;

		[STAThread]
		private static void Main(string[] args)
		{
			Directory.SetCurrentDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"SimpleDogeWallet\"));

			//prevent two instances from running,
			//open previous instance's window and bring it to the top
			Process currentProcess = Process.GetCurrentProcess();
			Process[] processes = Process.GetProcessesByName(currentProcess.ProcessName);

			foreach (var otherProcess in processes)
			{
				if (otherProcess.Id != currentProcess.Id)
				{
					SendReOpenRequest();
					return;
				}
			}

			ListenForReOpenRequests();


			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			_game = new SimpleDogeWalletWinFormGame();

			if (args.Length > 0 && args[0] == "-h")
			{
                _timer = new System.Threading.Timer(HideForm, null, 500, Timeout.Infinite);
            }

            _game.Run();
		}


		private static void HideForm(object e)
		{
            var form = _game.Services.GetService<Form>();

            if (form != null && form.InvokeRequired)
            {
                form.Invoke((MethodInvoker)delegate
                {
					form.Hide();
                });
            }
        }


		private static void SendReOpenRequest()
		{
			using (var client = new NamedPipeClientStream(PipeName))
			{
				client.Connect(1000);
				using (var writer = new StreamWriter(client))
				{
					writer.WriteLine("Reopen");
				}
			}
		}


		private static void ListenForReOpenRequests()
		{
			Task.Run(() =>
			{
				while (true)
				{
					using (var server = new NamedPipeServerStream(PipeName))
					{
						server.WaitForConnection();
						using (var reader = new StreamReader(server))
						{
							string message = reader.ReadLine();
							if (message == "Reopen")
							{
								OnReOpenRequest();
							}
						}
					}
				}
			});
		}


		//called when someone trys to open the app and it's already running
		private static void OnReOpenRequest()
		{
			try
			{
				var form = _game.Services.GetService<Form>();

                if (form != null && form.InvokeRequired)
                {
					form.Invoke((MethodInvoker)delegate
					{
						form.WindowState = FormWindowState.Minimized;
						form.Show();
						form.WindowState = FormWindowState.Normal;
					});
                }
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex.StackTrace);
				Console.WriteLine(ex.ToString());
			}
		}

	}
}