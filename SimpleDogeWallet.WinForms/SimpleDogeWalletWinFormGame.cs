
using Microsoft.Xna.Framework;
using SimpleDogeWallet.Common.BackgroundScenes;
using System;
using SimpleDogeWallet;
using System.Windows.Forms;
using SimpleDogeWallet.Common;
using Lib.Dogecoin;
using System.Threading.Tasks;
using SimpleDogeWallet.Pages;
using SimpleDogeWallet.Common.Pages;
using System.Net;



namespace SimpleDogeWallet.WinForms
{
	public class SimpleDogeWalletWinFormGame : SimpleDogeWalletGame, IReceiver<SPVNodeBlockInfo>
	{

		private ToolStripLabel _blockLabel;
		private ToolStripLabel _currentFees;

		private ToolStripMenuItem _quickTipMenu;

		private System.Windows.Forms.NotifyIcon _notifyIcon;
		private System.Windows.Forms.Form _form;

		protected override void OnResize(Object o, EventArgs evt)
		{


			_background = new MoonBackgroundScene(Services, _graphics.GraphicsDevice.Viewport.Width, _graphics.GraphicsDevice.Viewport.Height);

			_screen.SetWindowDim(_graphics, false, _graphics.GraphicsDevice.Viewport.Width, _graphics.GraphicsDevice.Viewport.Height);
		}


		public void Receive(SPVNodeBlockInfo message)
		{
			if(_blockLabel != null)
			{
				_blockLabel.Text = "Block: " + message.BlockHeight.ToString("N0");
			}

			if(_currentFees != null)
			{
				var estimatedFee = Services.GetService<FeeEstimator>().EstimatedFee;

				_currentFees.Text = "Fees: " + estimatedFee.ToString("N5");
			}
		}

		private void notifyIcon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (e.Button == System.Windows.Forms.MouseButtons.Left)
			{
				if(!_form.IsDisposed)
				{
					_form.Show();
					_form.WindowState = System.Windows.Forms.FormWindowState.Normal;
				}
			}
		}

		protected override void Initialize()
		{

			Messenger.Default.Register(this);

			Services.AddService<IPlatformControlTypeSelector>(new WinFormsPlatformControlTypeSelector());


			IntPtr handle = Window.Handle; // Your window handle
			System.Windows.Forms.Control control = System.Windows.Forms.Control.FromHandle(handle);
			_form = control as System.Windows.Forms.Form;
			_form.FormClosing += Form1_FormClosing;
            _form.Icon = new System.Drawing.Icon("Icon.ico");

            Services.AddService(_form);

			base.Initialize();

			_notifyIcon = new System.Windows.Forms.NotifyIcon();
			_notifyIcon.Icon = new System.Drawing.Icon("Icon.ico");
			_notifyIcon.Text = "Simple Doge Wallet";
			_notifyIcon.Visible = true;

			_notifyIcon.MouseClick += notifyIcon_MouseClick;

			// Create a context menu
			ContextMenuStrip contextMenu = new ContextMenuStrip();

			contextMenu.ShowImageMargin = false;

			// Create an "Exit" button
			ToolStripMenuItem exitButton = new ToolStripMenuItem("Exit");
			exitButton.Click += (sender, e) =>
			{
				_spvNodeService.Stop();
				Application.Exit();
			};


			// Create a "Copy Address" button
			ToolStripMenuItem copyAddressButton = new ToolStripMenuItem("Copy Your Address");
			copyAddressButton.Click += (sender, e) => {
				Clipboard.SetText(_settings.GetString("address"));
			};

			// Create a label to display some text
			var openButton = new ToolStripMenuItem(Strings.Current.GetString("terminal-title"));

			openButton.Click += (sender, e) =>
			{
				_form.Show();
				_form.WindowState = System.Windows.Forms.FormWindowState.Normal;
			};

			_blockLabel = new ToolStripLabel("Block:");
			_currentFees = new ToolStripLabel("Fees:");




			contextMenu.Items.Add(openButton);
			contextMenu.Items.Add(new ToolStripSeparator());
			contextMenu.Items.Add(_blockLabel);
			contextMenu.Items.Add(_currentFees);
			contextMenu.Items.Add(new ToolStripSeparator());
			contextMenu.Items.Add(copyAddressButton);
			contextMenu.Items.Add(new ToolStripSeparator());
			contextMenu.Items.Add(exitButton);

			// Add the context menu to the notify icon
			_notifyIcon.ContextMenuStrip = contextMenu;

			Exiting += SimpleDogeWalletGame_Exiting;


		}

		private void SimpleDogeWalletGame_Exiting(object sender, EventArgs e)
		{
			_spvNodeService.Stop();
		}

		private void Form1_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
		{
			if (e.CloseReason == System.Windows.Forms.CloseReason.UserClosing)
			{

				if(SimpleDogeWallet.Instance == null || string.IsNullOrWhiteSpace(SimpleDogeWallet.Instance.Address))
				{
					//wallet is in setup mode, don't prevent close
					return;
				}

				e.Cancel = true;
				_form.WindowState = System.Windows.Forms.FormWindowState.Minimized;
				_form.Hide();
				_notifyIcon.Visible = true;


			}
		}

		protected override void Update(GameTime gameTime)
		{

			base.Update(gameTime);
		}



	}
}