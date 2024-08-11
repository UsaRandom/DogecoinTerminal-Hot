
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimpleDogeWallet.Common;
using Microsoft.Xna.Framework.Input;
using SimpleDogeWallet.Common.BackgroundScenes;
using System.Xml.Linq;
using SimpleDogeWallet.Common.Controls;
using System;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using SimpleDogeWallet.Common.Interop;
using MonoGame.Framework.Utilities;
using SimpleDogeWallet.Common.Pages;
using SimpleDogeWallet.Pages;
using Lib.Dogecoin;
using System.Diagnostics.Metrics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleDogeWallet
{
	public class SimpleDogeWalletGame : Game
	{
		protected GraphicsDeviceManager _graphics;
		protected SpriteBatch _spriteBatch;
		protected Navigation _nav;
		protected ButtonControl _devButton;
		protected ITerminalSettings _settings;
		protected SelectedControlVisitor _textInputSelector;

		protected IClipboardService _clipboardService;
		protected IUserInputService _userInputService;

		protected SimpleSPVNodeService _spvNodeService;

		protected IBackgroundScene _background;

		//dev tools
		protected ButtonState lastButtonState = ButtonState.Released;
		protected MoveHandlesControlVisitor _moveHandler;

		public FontSystem _fontSystem;
		public VirtualScreen _screen;

		public bool LockRequested
		{
			get;
			set;
		}



		public SimpleDogeWalletGame()
		{
			Trace.Listeners.Add(new ConsoleTraceListener());

			_graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			IsMouseVisible = true;

			_screen = new VirtualScreen();
			_fontSystem = new FontSystem();


			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

		}

		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			var exception = ((Exception)e.ExceptionObject);

			Debug.WriteLine($"An unhandled exception occurred: {exception.Message}");
			Debug.WriteLine("Stack trace:");
			Debug.WriteLine(exception.StackTrace);
		}

		protected virtual void OnResize(Object o, EventArgs evt)
		{

			if ((_graphics.PreferredBackBufferWidth != _graphics.GraphicsDevice.Viewport.Width) ||
					(_graphics.PreferredBackBufferHeight != _graphics.GraphicsDevice.Viewport.Height))
			{
				//		_graphics.PreferredBackBufferWidth = _graphics.GraphicsDevice.Viewport.Width;
				//		_graphics.PreferredBackBufferHeight = _graphics.GraphicsDevice.Viewport.Height;

				_background = new MoonBackgroundScene(Services, _graphics.GraphicsDevice.Viewport.Width, _graphics.GraphicsDevice.Viewport.Height);

				_screen.SetWindowDim(_graphics, false, _graphics.GraphicsDevice.Viewport.Width, _graphics.GraphicsDevice.Viewport.Height);
				//		_graphics.ApplyChanges();

			}
		}

		protected override void Initialize()
		{
			Window.AllowUserResizing = true;
			Window.ClientSizeChanged += OnResize;

			_settings = new TerminalSettings();

			_settings.Set("terminal-devmode", false);
			_settings.Set("terminal-background", true);

			if (!_settings.IsSet("dust-limit"))
			{
				_settings.Set("dust-limit", SimpleDogeWallet.DEFAULT_DUST_LIMIT);
			}

			if (!_settings.IsSet("fee-coeff"))
			{
				_settings.Set("fee-coeff", SimpleDogeWallet.DEFAULT_FEE_COEFF);
			}

			Strings.Current.SelectLanguage(
				Language.Languages[_settings.GetString("language", "eng")]);


			TerminalColor.Init(_graphics.GraphicsDevice);


			_screen.Init(_graphics, _settings.GetBool("terminal-fullscreen", false), _settings);

			_nav = new Navigation(Services);

			_userInputService = new UserInputService(this);
			_clipboardService = new TextCopyClipboardService();

			_spvNodeService = new SimpleSPVNodeService();

			Window.Title = Strings.Current.GetString("terminal-title");


			Services.AddService(Strings.Current);
			Services.AddService(_nav);
			Services.AddService(_screen);
			Services.AddService(new ContactService());
			Services.AddService(new Images(GraphicsDevice));
			Services.AddService(_settings);
			Services.AddService(GraphicsDevice);
			Services.AddService<Game>(this);
			Services.AddService<IServiceProvider>(Services);
			Services.AddService(_userInputService);


			if(Services.GetService<IPlatformControlTypeSelector>() == null)
			{
				Services.AddService<IPlatformControlTypeSelector>(new DefaultPlatformControlTypeSelector());
			}


			Services.AddService(_spvNodeService);

            Services.AddService(new NetworkMonitorService(_spvNodeService));

            Services.AddService(new FeeEstimator(Services));

			Services.AddService<IClipboardService>(_clipboardService);


			if(LibDogecoinTpmContext.Instance.IsSupportedPlatform)
			{
				Services.AddService<IPasswordService>(new TpmPasswordService(_settings));
			}
			else
			{
				Services.AddService<IPasswordService>(new BasicPasswordService(Services));
			}


			//text input selector
			_textInputSelector = new SelectedControlVisitor(_screen);


			//dev tools
			var devButtonEl = new XElement("button");
			devButtonEl.Add(new XAttribute("Name", "DevButton"));
			devButtonEl.Add(new XAttribute("StartPosition", "0,97"));
			devButtonEl.Add(new XAttribute("EndPosition", "4,99"));
			devButtonEl.Add(new XAttribute("BackgroundColor", "Blue"));
			devButtonEl.Add(new XAttribute("ForegroundColor", "White"));
			devButtonEl.Add(new XAttribute("TextSize", "2"));
			devButtonEl.Add(new XAttribute("Text", "copy"));
			_devButton = new ButtonControl(devButtonEl);
			_moveHandler = new MoveHandlesControlVisitor(_screen);


			_background = new MoonBackgroundScene(Services, _graphics.GraphicsDevice.Viewport.Width, _graphics.GraphicsDevice.Viewport.Height);


			this.Exiting += SimpleDogeWalletGame_Exiting;




			base.Initialize();
		}


		private void SimpleDogeWalletGame_Exiting(object sender, EventArgs e)
		{
			_spvNodeService.Stop();
			_nav.CurrentPage.Cleanup();
		}

		protected override void LoadContent()
		{

			_spriteBatch = new SpriteBatch(GraphicsDevice);

			_screen.Load(_spriteBatch);

			//Just for testing wallet creation.

			//		SimpleDogeWallet.ClearWallet();

			try
			{
				Services.AddService(LibDogecoinContext.Instance);

			}
			catch
			{
				var msg = $"Failed to load libdogecoin.\n\n"
						+ "This is likely caused by your device having missing or misconfigured security\nhardware, like TPM2.0"
						+ "\n"
						+ "\nCheck windows security settings and ensure you see one of the following messages:" +
								"\n - 'Your device meets the requirements for standard hardware security.'" +
								"\n - 'Your device meets the requirements for enhanced hardware security.'" +
								"\n - 'Your device has all Secured-core PC features enabled'\n\n"

						+ "The following message means this wallet is not supported on your device =[\n"
						+ " - 'Standard hardware security not supported'";

				_nav.PushAsync<ExceptionPage>(("title", "Not Supported"), ("error", msg));

				return;
			}
			var hasWallet = File.Exists(SimpleDogeWallet.ADDRESS_FILE);//detect

			if (!hasWallet)
			{
				_nav.PushAsync<LoadingPage>();

				//start on the language selection screen:
				Task.Run(async () =>
				{
				//	await _nav.PromptAsync<DisclaimerPage>();
					await _nav.PromptAsync<LanguageSelectionPage>();
					await _nav.TryInsertBeforeAsync<SetupWalletPage, LoadingPage>();
					_nav.Pop();
				});
			}
			else
			{
				SimpleDogeWallet.Init(Services);
				_spvNodeService.Start();

                _nav.PushAsync<LoadingPage>();
                _nav.TryInsertBeforeAsync<WalletPage, LoadingPage>(("wallet", SimpleDogeWallet.Instance));
                
                Task.Run(async () =>
				{
			//		await _nav.PromptAsync<DisclaimerPage>();
					_nav.Pop();
				});

			}
		}

		protected override void Update(GameTime gameTime)
		{
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
				Exit();

			//if(LockRequested)
			//{

			//	while (_nav.CurrentPage != null)
			//	{
			//		_nav.Pop();
			//	}

			//	_nav.PushAsync<UnlockTerminalPage>();

			//	LockRequested = false;
			//}

			_textInputSelector.UpdateMouse();
			foreach (var control in _nav.CurrentPage.Controls)
			{
				control.AcceptVisitor(_textInputSelector);
			}


			_nav.CurrentPage.Update(gameTime, Services);

			if (IsActive)
			{
				var mouseState = Mouse.GetState(Window);

				if (mouseState.LeftButton == ButtonState.Pressed &&
					lastButtonState == ButtonState.Released)
				{
					Messenger.Default.Send(
						new UserClickMessage(
							_screen.WindowCoordToVirtualCoord(
								new Point(mouseState.X, mouseState.Y))));

					if (_settings.GetBool("terminal-devmode", false))
					{
						if (_devButton.ContainsPoint(_screen.WindowCoordToVirtualCoord(
								new Point(mouseState.X, mouseState.Y))))
						{

							var xmlExporter = new XmlExporterControlVisitor();

							foreach (var control in _nav.CurrentPage.Controls)
							{
								control.AcceptVisitor(xmlExporter);
							}

							_clipboardService.SetClipboardContents(xmlExporter.PageElement.ToString());
						}
					}

				}

				lastButtonState = mouseState.LeftButton;



				if (_settings.GetBool("terminal-devmode", false))
				{
					_moveHandler.UpdateMouse();

					foreach (var control in _nav.CurrentPage.Controls)
					{
						control.AcceptVisitor(_moveHandler);
					}

				}
			}


			if (_settings.GetBool("terminal-background", false))
			{
				_background.Update(gameTime, Services);
			}


			base.Update(gameTime);
		}


		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(TerminalColor.Grey.Color);


			_spriteBatch.Begin();


			if (_settings.GetBool("terminal-background", false))
			{
				_background.Draw(gameTime, _spriteBatch, Services);
			}

			_nav.CurrentPage.Draw(gameTime, Services);

			if (_settings.GetBool("terminal-devmode", false))
			{
				var handles = new DrawHandlesControlVisitor(_spriteBatch, _screen);

				foreach (var control in _nav.CurrentPage.Controls)
				{
					control.AcceptVisitor(handles);
				}

				_devButton.Draw(gameTime, Services);
				_screen.DrawText(_screen.WindowCoordToVirtualCoord(Mouse.GetState().Position).ToString(), TerminalColor.White, 2, new Point(50, 98));
			}



			_spriteBatch.End();

			base.Draw(gameTime);
		}


	}
}