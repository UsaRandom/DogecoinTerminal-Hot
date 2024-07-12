﻿using SimpleDogeWallet.Common.Pages;
using SimpleDogeWallet.Common;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.Dogecoin;
using System.IO;
using System.Transactions;
using System.Xml.Linq;
using System.Runtime;

namespace SimpleDogeWallet.Pages
{

	[PageDef("Pages/Xml/SetupWalletPage.xml")]
	internal class SetupWalletPage : Page
	{

		private Game _game;

		private Navigation _navigation;
		private Strings _strings;
		private ITerminalSettings _settings;
		private LibDogecoinContext _ctx;
		private IServiceProvider _services;

		public SetupWalletPage(IPageOptions options, IServiceProvider services, Navigation navigation, Strings strings, Game game, ITerminalSettings settings, LibDogecoinContext ctx) : base(options)
		{
			_game = game;
			_services = services;
			_ctx = ctx;
			_strings = strings;
			_navigation = navigation;
			_settings = settings;

			OnClick("NewWalletButton", async _ =>
			{
				SetupWallet(true);
			});

			OnClick("LoadWalletButton", async _ =>
			{
				SetupWallet(false);
			});

		
		}


		private async void SetupWallet(bool isNew)
		{

			await _navigation.PushAsync<LoadingPage>();

			var newPin = string.Empty;
			var confirmPin = string.Empty;

			while (newPin != confirmPin || newPin.Length < SimpleDogeWallet.MIN_PIN_LENGTH)
			{
				var enterPin = await _navigation.PromptAsync<NumPadPage>(
					("title", _strings.GetString("terminal-setup-setpin")),
					("hint", _strings.GetString("terminal-setup-setpin-hint")),
					("regex", ".{" + SimpleDogeWallet.MIN_PIN_LENGTH + ",16}"));

				if (enterPin.Response == PromptResponse.YesConfirm)
				{
					newPin = (string)enterPin.Value;
				}
				else
				{
					_navigation.Pop();
					return;
				}

				var confirm = await _navigation.PromptAsync<NumPadPage>(
					("title", _strings.GetString("terminal-setup-confirmpin")),
					("hint", _strings.GetString("terminal-setup-confirmpin-hint")),
					("regex", ".{"+ SimpleDogeWallet.MIN_PIN_LENGTH + ",16}"));

				if (confirm.Response == PromptResponse.YesConfirm)
				{
					confirmPin = (string)confirm.Value;
				}
			}

			string address = string.Empty;
			bool createdWallet = false;

			string mnemonic = string.Empty;


			int tpmFileNumber = _settings.GetInt("tpm-file-number", -1);

			if (tpmFileNumber == -1)
			{
				//need to make sure we don't overwrite other keys,
				// so we need to find a good key number.
				var tpmKeys = _ctx.ListKeysInTPM();
				var takenNumbers = new List<int>();

				// key names look like: "dogecoin_mnemonic_069"
				foreach (var key in tpmKeys)
				{
					if (key.StartsWith("dogecoin_mnemonic_"))
					{
						int fileNumber = int.Parse(key.Split('_')[2]);
						takenNumbers.Add(fileNumber);
					}
				}

				for (var number = 0; number < 999; number++)
				{
					if (!takenNumbers.Contains(number))
					{
						tpmFileNumber = number;
						break;
					}
				}

				if (tpmFileNumber == -1)
				{
					await _navigation.PromptAsync<ShortMessagePage>(("message", _strings.GetString("terminal-setup-tpm-is-full")));
					_navigation.Pop();
					return;
				}

				_settings.Set("tpm-file-number", tpmFileNumber);
			}


			if (isNew)
			{

				//we have a valid mnemonic, encrypt it with key stored in tpm
				var mnemonicKey = _ctx.GenerateMnemonicEncryptWithTPM(tpmFileNumber, lang: "eng", space: "-");

				mnemonic = _ctx.GenerateMnemonic(_strings.Language.LanguageCode, LibDogecoinContext.ENTROPY_SIZE_128);

				File.WriteAllText(SimpleDogeWallet.LOADED_MNEMONIC_FILE, Crypto.Encrypt(mnemonic, mnemonicKey));

				await _navigation.PromptAsync<BackupCodePage>(("mnemonic", mnemonic), ("editmode", false));
			}
			else
			{

				while(true)
				{
					var getMnemonic = await _navigation.PromptAsync<BackupCodePage>(("title", _strings.GetString("terminal-backupcodes-load-title")),
																					("editmode", true),
																					("mnemonic", mnemonic));

					if (getMnemonic.Response == PromptResponse.YesConfirm)
					{
						//we need to confirm
						mnemonic = ((string)getMnemonic.Value).Trim('-');

						if (!IsValidMnemonic(_strings.Language.LanguageCode, mnemonic))
						{
							var retry = await _navigation.PromptAsync<YesNoPage>(("message", _strings.GetString("terminal-backupcodes-load-badmnemonic")));


							if(retry.Response == PromptResponse.YesConfirm)
							{
								continue;
							}
							else
							{
								_navigation.Pop();
								return;
							}
						}

						//we have a valid mnemonic, encrypt it with key stored in tpm
						var mnemonicKey = _ctx.GenerateMnemonicEncryptWithTPM(tpmFileNumber, lang: "eng", space: "-");

						File.WriteAllText(SimpleDogeWallet.LOADED_MNEMONIC_FILE, Crypto.Encrypt(mnemonic, mnemonicKey));

						break;
					}
					else
					{
						_navigation.Pop();
						return;
					}
				}

			}

			var masterKeys = _ctx.GenerateHDMasterPubKeypairFromMnemonic(mnemonic.Replace("-", " "));

			if (_ctx.VerifyHDMasterPubKeyPair(masterKeys.privateKey, masterKeys.publicKey))
			{
				address = _ctx.GetDerivedHDAddressByPath(masterKeys.privateKey, Crypto.HDPATH, false);

				File.WriteAllText("address", Crypto.Encrypt(address, newPin));

				_settings.Set("address", address);

				SimpleDogeWallet.Init(_services);

				createdWallet = true;
			}

			

			GC.Collect();

			if (createdWallet)
			{
				await _navigation.TryInsertBeforeAsync<WalletPage, LoadingPage>(("wallet", SimpleDogeWallet.Instance), ("is-new", isNew));
				await _navigation.PopToPage<WalletPage>();
			}
			else
			{
				await _navigation.PromptAsync<ShortMessagePage>(("message", "i have no idea how you did that"));
				_navigation.Pop();
			}
		}


		private bool IsValidMnemonic(string languageCode, string mnemonic)
		{
			//TODO: Verify more than just the number of words.
			var length = mnemonic.Split("-").Length;

			if(!(length == 12 || length == 24))
			{
				return false;
			}
			

			return true;
		}

		public override void Draw(GameTime gameTime, IServiceProvider services)
		{
			var screen = services.GetService<VirtualScreen>();

			screen.DrawRectangle(TerminalColor.DarkGrey, new Point(5, 30), new Point(48, 80));
			screen.DrawRectangle(TerminalColor.DarkGrey, new Point(52, 30), new Point(95, 80));

			screen.DrawRectangle(TerminalColor.Blue, new Point(5, 30), new Point(48, 40));
			screen.DrawRectangle(TerminalColor.Green, new Point(52, 30), new Point(95, 40));



			base.Draw(gameTime, services);
		}
	}
}
