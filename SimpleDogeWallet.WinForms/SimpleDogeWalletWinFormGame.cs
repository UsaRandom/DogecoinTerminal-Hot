﻿
using Microsoft.Xna.Framework;
using SimpleDogeWallet.Common.BackgroundScenes;
using System;
using SimpleDogeWallet;
using System.Windows.Forms;



namespace SimpleDogeWallet.WinForms
{
	public class SimpleDogeWalletWinFormGame : SimpleDogeWalletGame
	{

		private System.Windows.Forms.NotifyIcon _notifyIcon;

		private System.Windows.Forms.Form _form;

		protected override void OnResize(Object o, EventArgs evt)
		{

			//if ((_graphics.PreferredBackBufferWidth != _graphics.GraphicsDevice.Viewport.Width) ||
			//		(_graphics.PreferredBackBufferHeight != _graphics.GraphicsDevice.Viewport.Height))
			//{
			//	_graphics.PreferredBackBufferWidth = _graphics.GraphicsDevice.Viewport.Width;
			//	_graphics.PreferredBackBufferHeight = _graphics.GraphicsDevice.Viewport.Height;

			_background = new MoonBackgroundScene(Services, _graphics.GraphicsDevice.Viewport.Width, _graphics.GraphicsDevice.Viewport.Height);

			_screen.SetWindowDim(_graphics, false, _graphics.GraphicsDevice.Viewport.Width, _graphics.GraphicsDevice.Viewport.Height);
			//	_graphics.ApplyChanges();

			//}
		}
		private void notifyIcon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (e.Button == System.Windows.Forms.MouseButtons.Left)
			{
				_form.Show();
				_form.WindowState = System.Windows.Forms.FormWindowState.Normal;
			}
		}

		protected override void Initialize()
		{

			base.Initialize();

			_notifyIcon = new System.Windows.Forms.NotifyIcon();
			_notifyIcon.Icon = new System.Drawing.Icon("Icon.ico");
			_notifyIcon.Text = "Simple Doge Wallet";
			_notifyIcon.Visible = true;
			_notifyIcon.MouseClick += notifyIcon_MouseClick;

			// Create a context menu
			ContextMenuStrip contextMenu = new ContextMenuStrip();

			// Create an "Exit" button
			ToolStripMenuItem exitButton = new ToolStripMenuItem("Exit");
			exitButton.Click += (sender, e) => Application.Exit();

			// Create a "Copy Address" button
			ToolStripMenuItem copyAddressButton = new ToolStripMenuItem("Copy Address");
			copyAddressButton.Click += (sender, e) => {
				Clipboard.SetText(_settings.GetString("address"));
			};

			// Create a label to display some text
			ToolStripLabel label = new ToolStripLabel("Simple Doge Wallet");
			contextMenu.Items.Add(copyAddressButton);
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
				IntPtr handle = Window.Handle; // Your window handle
				System.Windows.Forms.Control control = System.Windows.Forms.Control.FromHandle(handle);
				System.Windows.Forms.Form form = control as System.Windows.Forms.Form;
				if (form != null)
				{
					e.Cancel = true;
					form.WindowState = System.Windows.Forms.FormWindowState.Minimized;
					form.Hide();
					_notifyIcon.Visible = true;
				}


			}
		}

		protected override void Update(GameTime gameTime)
		{
			if (_form == null)
			{
				IntPtr handle = Window.Handle; // Your window handle
				System.Windows.Forms.Control fControl = System.Windows.Forms.Control.FromHandle(handle);
				System.Windows.Forms.Form form = fControl as System.Windows.Forms.Form;
				if (form != null)
				{
					_form = form;

					_form.FormClosing += Form1_FormClosing;
				}

			}


			base.Update(gameTime);
		}



	}
}