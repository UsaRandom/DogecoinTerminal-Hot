﻿using DogecoinTerminal.Common;
using DogecoinTerminal.Common.Pages;
using DogecoinTerminal.old;
using Lib.Dogecoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DogecoinTerminal.Pages
{
    [PageDef("Pages/Xml/WalletPage.xml")]
    internal class WalletPage : Page
    {
        public WalletPage(IPageOptions options, Navigation navigation, Strings strings, IMnemonicProvider mnemonicProvider) : base(options)
        {
            var slot = options.GetOption<IWalletSlot>("slot");

            var addressTextControl = GetControl<TextControl>("AddressText");
            addressTextControl.Text = slot.Address;


            var balanceTextControl = GetControl<TextControl>("BalanceText");
            balanceTextControl.Text = "Đ" + slot.CalculateBalance();


            OnClick("BackButton", async _ =>
            {
                navigation.Pop();
            });

            OnClick("ReceiveButton", async _ =>
            {
				await navigation.PromptAsync<DisplayQRPage>(("message", slot.Address), ("qr", slot.Address));

			});


            OnClick("RemoveButton", async _ =>
            {
                //ok, we want to remove this wallet.
                //first put in a loading page for flicker
                await navigation.PushAsync<BlankPage>();

                var numPadResult = await navigation.PromptAsync<NumPadPage>();

                //We need to get the users pin confirmed on delete.
                if (numPadResult.Response == PromptResponse.YesConfirm
                    && slot.SlotPin == (string)numPadResult.Value)
                {
                    //we've confirmed pin
                    //now lets delete the slot.
                    slot.ClearSlot();
					//remove loading screen.
					navigation.Pop();
                    //remove wallet page
                    navigation.Pop();
				}
                else
                {
                    //just remove loading screen
					navigation.Pop();
				}


			});

            OnClick("ShowSeedButton", async _ =>
            {
                await navigation.PushAsync<BlankPage>();

                var acknowledge = await navigation.PromptAsync<ShortMessagePage>(("message", "Don't share seed phrase, have pen & paper ready!"));

                if(acknowledge.Response == PromptResponse.YesConfirm)
                {
                    string mnemonic = string.Empty;

                    using(var ctx = LibDogecoinContext.CreateContext())
                    {
                        mnemonic = mnemonicProvider.GetMnemonic(ctx, slot.SlotNumber);
					}

                    if(!string.IsNullOrEmpty(mnemonic))
					{
						await navigation.TryInsertBeforeAsync<BackupCodePage, BlankPage>(("mnemonic", mnemonic));
					}
                }

                navigation.Pop();

            });



   //         OnClick("UpdatePinButton", async _ =>
   //         {
   //             //User wants to update pin

   //             await navigation.PushAsync<BlankPage>();

   //             var numPadResult = await navigation.PromptAsync<NumPadPage>(("title", strings["terminal-wallet-updatepin-newpin"]));

			//	var enteredPin = (string)numPadResult.Value;

			//	if (numPadResult.Response != PromptResponse.YesConfirm
   //                 || string.IsNullOrEmpty(enteredPin))
			//	{
			//		//just remove loading screen
			//		navigation.Pop();
   //                 return;
			//	}

			//	numPadResult = await navigation.PromptAsync<NumPadPage>(("title", strings["terminal-wallet-updatepin-confirmpin"]));

			//	if (numPadResult.Response != PromptResponse.YesConfirm
   //                 && enteredPin != (string)numPadResult.Value)
			//	{

   //                 //the new pin was not updated, so lets notify user!

   //                 await navigation.PromptAsync<ShortMessagePage>(("message", "ERROR: Pin was NOT updated!"));

			//		navigation.Pop();
			//	}
   //             else
   //             {
   //                 //update the pin

   //                 slot.UpdateSlotPin(enteredPin);

   //                 await navigation.PromptAsync<ShortMessagePage>(("message", "SUCCESS: Pin was updated!"));

			//		navigation.Pop();

			//	}
			//});

		}
    }
}
