using SimpleDogeWallet.Common;
using Lib.Dogecoin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace SimpleDogeWallet
{
	public class SimpleDogeWallet
	{
		public const decimal DEFAULT_DUST_LIMIT = 0.001M;
		public const decimal DEFAULT_FEE_COEFF = 1.618M;

		
		public const int MIN_PIN_LENGTH = 4;
		public const string ADDRESS_FILE = "address";
		public const string LOADED_MNEMONIC_FILE = "loadedmnemonic";
		public const string UTXO_FILE = "utxos";


		private LibDogecoinContext _ctx;
		public int _tpmFileNumber;

		private decimal _pendingAmount;
		private string _pendingHash;
		private DateTime _pendingTxSubmitTime;

		public SimpleDogeWallet(string address, IServiceProvider services)
		{
			Address = address;
			Services = services;

			_tpmFileNumber = services.GetService<ITerminalSettings>().GetInt("tpm-file-number");

			_pendingHash = services.GetService<ITerminalSettings>().GetString("pending-hash", string.Empty);
			_pendingAmount = services.GetService<ITerminalSettings>().GetDecimal("pending-amount", 0);
			_pendingTxSubmitTime = services.GetService<ITerminalSettings>().GetDateTime("pending-time", DateTime.MaxValue);

            _ctx = LibDogecoinContext.Instance;

			LoadUTXOs();

			_instance = this;
		}



		public string Address
		{
			get;
			private set;
		}


		public void Save()
		{
			SaveUTXOs();
		}

		private static SimpleDogeWallet _instance;
		public static SimpleDogeWallet Instance
		{
			get
			{
				return _instance;
			}
		}

		public decimal GetBalance()
		{
			if(UTXOs.Count == 0)
			{
				return 0;
			}
			return UTXOs.Sum(utxo => utxo.Amount);
		}

		public List<UTXO> UTXOs
		{
			get;
			private set;
		}

		private IServiceProvider Services
		{
			get;
			set;
		}


		public decimal PendingAmount
		{
			get
			{
				return _pendingAmount;
			}
			set
			{
				_pendingAmount = value;
				Services.GetService<ITerminalSettings>().Set("pending-amount", _pendingAmount);
			}
		}

		public string PendingTxHash
		{
			get
			{
				return _pendingHash;
			}
			set
			{
				_pendingHash = value;
				Services.GetService<ITerminalSettings>().Set("pending-hash", _pendingHash);
			}
		}

		public DateTime PendingTxTime
		{
			get
			{
				return _pendingTxSubmitTime;
            }
			set
			{
                _pendingTxSubmitTime = value;
				Services.GetService<ITerminalSettings>().Set("pending-time", _pendingTxSubmitTime);

            }
		}

		public bool UpdatePending()
		{
            if (!string.IsNullOrWhiteSpace(PendingTxHash) && PendingTxTime < DateTime.Now - TimeSpan.FromHours(24))
            {
                PendingTxTime = DateTime.MaxValue;
                PendingTxHash = string.Empty;
                PendingAmount = 0;
				return true;
            }
			return false;
        }



		public string GetMnemonic()
		{
			var key = Services.GetService<IPasswordService>().GetPassword();// _ctx.DecryptMnemonicWithTPM(_tpmFileNumber);

			return Crypto.Decrypt(File.ReadAllText(LOADED_MNEMONIC_FILE), key);
		}

		//public static void UpdatePin(string oldPin, string newPin)
		//{
		//	var address = Crypto.Decrypt(File.ReadAllText(ADDRESS_FILE), oldPin);

		//	string encryptedAddress = Crypto.Encrypt(address, newPin);
		//	string tempFilePath = Path.GetTempFileName();

		//	File.WriteAllText(tempFilePath, encryptedAddress);

		//	File.Move(tempFilePath, ADDRESS_FILE, true);
		//}

		public static void Init(IServiceProvider services)
		{

			if (Instance == null)
			{
				_instance = new SimpleDogeWallet(services.GetService<ITerminalSettings>().GetString("address"), services);
			}

		}

		public static bool TryOpen(string pin)
		{
			try
			{
				var address = Crypto.Decrypt(File.ReadAllText(ADDRESS_FILE), pin);

				if(!LibDogecoinContext.Instance.VerifyP2pkhAddress(address))
				{
					return false;
				}


				return true;
			}
			catch (Exception ex)
			{
				
			}
			return false;
		}



		//TODO: Remove duplicate code.

		private void LoadUTXOs()
		{
			UTXOs = new List<UTXO>();

			if (File.Exists(UTXO_FILE))
			{
				var utxoString = File.ReadAllText(UTXO_FILE);

				if (!string.IsNullOrEmpty(utxoString))
				{
					utxoString = utxoString.Replace("\r", string.Empty);

					var lines = utxoString.Split('\n');

					foreach (var line in lines)
					{
						if (string.IsNullOrEmpty(line))
						{
							continue;
						}

						var lineParts = line.Split('|');

						UTXOs.Add(new UTXO
						{
							TxId = lineParts[0],
							VOut = int.Parse(lineParts[1]),
							AmountKoinu = long.Parse(lineParts[2]),
							BlockHeight = uint.Parse(lineParts[3])
						});
					}
				}
			}


			
		}

		private void SaveUTXOs()
		{
			var utxoContent = new StringBuilder();

			foreach (var utxo in UTXOs)
			{
				utxoContent.Append(utxo.TxId + "|");
				utxoContent.Append(utxo.VOut + "|");
				utxoContent.Append(utxo.AmountKoinu?.ToString() + "|");
				utxoContent.AppendLine(utxo.BlockHeight.ToString());
			}

			string utxoContentString = utxoContent.ToString();
			string tempFilePath = Path.GetTempFileName();

			File.WriteAllText(tempFilePath, utxoContentString);

			File.Move(tempFilePath, UTXO_FILE, true);
		}


		public static void ClearWallet()
		{
			Instance.Address = null;
			_instance.Services.GetService<ITerminalSettings>().Set("address", string.Empty);
			_instance = null;
			
			File.Delete(ADDRESS_FILE);
			File.Delete(LOADED_MNEMONIC_FILE);
			File.Delete(UTXO_FILE);
		}



	}
}
