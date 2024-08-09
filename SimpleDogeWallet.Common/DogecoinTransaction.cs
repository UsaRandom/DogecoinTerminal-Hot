using Lib.Dogecoin;
using System;
using System.Collections.Generic;
using SimpleDogeWallet.Common;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;


namespace SimpleDogeWallet
{
    internal class DogecoinTransaction
    {

        private int _workingTransactionId;
        private LibDogecoinContext _ctx;
        private IServiceProvider _services;
        private List<UTXO> _txUTXOs;

        public decimal Fee { get; private set; }

        public decimal Amount { get; private set; }

        public decimal Total
        {
            get
            {
                return Fee + Amount;
            }
        }

        internal decimal Remainder
        {
            get;
            private set;
        }

        public string To { get; private set; }

        public string From { get; private set; }

        public SimpleDogeWallet Wallet { get; private set; }

		public DogecoinTransaction(IServiceProvider services, SimpleDogeWallet wallet)
        {
            _services = services;
            Wallet = wallet;
            _txUTXOs = new List<UTXO>();
            _ctx = services.GetService<LibDogecoinContext>();
        }

		public bool Send(string recipient, decimal amount)
        {
            var settings = _services.GetService<ITerminalSettings>();

            var dustLimit = settings.GetDecimal("dust-limit");

            if (amount < dustLimit)
            {
                return false;
            }

            _ctx.RemoveAllTransactions();

            _workingTransactionId = _ctx.StartTransaction();


            //225 + 148 * (utxoInCount-1)


            var rate = _services.GetService<SimpleSPVNodeService>().EstimatedRate;
            var feeCoeff = _services.GetService<ITerminalSettings>().GetDecimal("fee-coeff");



			var ratePerByte = rate * feeCoeff;



            decimal fee = _services.GetService<FeeEstimator>().EstimatedFee;
            decimal feePerUtxo = ratePerByte * 148;
			decimal sum = 0M;
			int utxoCount = 0; //min 2 utxo (receipient + change)




            //detect if we are spending everything
			var maxSpend = SimpleDogeWallet.Instance.GetBalance();
            bool spendingAll = false;
			maxSpend -= ratePerByte * (225 + (SimpleDogeWallet.Instance.UTXOs.Count - 1) * 148);
			maxSpend = Math.Round(maxSpend, (int)Math.Ceiling(Math.Log10(1 / (double)dustLimit)), MidpointRounding.ToZero);


			if (Math.Abs(maxSpend - amount) < dustLimit)
            {
                spendingAll = true;
			}

			//UTXOs who's value is greater than the fee + dustlimit * 2  (so it makes sense to spend it)
			//Order by desc, so bigger UTXOs first.
			var spendableUTXOs = Wallet.UTXOs.Where(utxo => utxo.Amount > (fee + dustLimit * 2))
                                             .OrderByDescending(a => a.Amount);

            var utxoEnumerator = spendableUTXOs.GetEnumerator();



            do
            {
                if (!utxoEnumerator.MoveNext())
                {
                    break;
                }

                var utxo = utxoEnumerator.Current;

                sum += utxo.Amount;

                utxoCount++;

                _txUTXOs.Add(utxo);

                if (!_ctx.AddUTXO(_workingTransactionId, utxo.TxId, utxo.VOut))
                {
                    return false;
                }

            } while (sum < amount + (fee + feePerUtxo * (utxoCount-1)));

            if (sum < amount + (fee + feePerUtxo * (utxoCount - 1)))
            {
                return false;
            }

            var totalFee = ((feePerUtxo * (utxoCount - 1)) + fee);

            var remainder = sum - totalFee - amount;

            var amountStr = amount.ToString();
            var feeStr = totalFee.ToString();
            var sumStr = sum.ToString();
            var remainderStr = remainder.ToString();


            Remainder = remainder;
            Fee = totalFee;
            Amount = amount;

            To = recipient;
            From = Wallet.Address;


            if (Remainder > dustLimit && !spendingAll)
            {

                if (!_ctx.AddOutput(_workingTransactionId, From, remainderStr))
                {
                    return false;
                }
            }

            if (spendingAll)
            {
                Fee += Remainder;
                Remainder = 0;
            }

            if (!_ctx.AddOutput(_workingTransactionId, To, amountStr))
            {
                return false;
            }

            return true;
        }

        public bool Sign()
        {
            try
			{
                var mnemonic = Wallet.GetMnemonic();

                if(mnemonic == null)
                {
                    return false;
                }

				var masterKeys = _ctx.GenerateHDMasterPubKeypairFromMnemonic(mnemonic.Replace("-", " "));

				var pk = _ctx.GetHDNodePrivateKeyWIFByPath(masterKeys.privateKey, Crypto.HDPATH, true);
		
				for (var i = 0; i < _txUTXOs.Count; i++)
				{
					if (!_ctx.SignTransactionWithPrivateKey(_workingTransactionId, i, pk))
					{
						return false;
					}
				}
				return true;
			}
            finally
            {
                //force collection after signing
				GC.Collect(0, GCCollectionMode.Forced, true);
				GC.Collect(1, GCCollectionMode.Forced, true);
			}
        }


        public string GetRawTransaction()
        {
            return _ctx.GetRawTransaction(_workingTransactionId);
        }

		public async Task<bool> BroadcastAsync()
        { 

            
            var cancelTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(17));
            var cancelToken = cancelTokenSource.Token;

            //i was unable to redirect output from sendtx
            var processStartInfo = new ProcessStartInfo("cmd.exe")
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                WorkingDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"SimpleDogeWallet\")
            };

            //var processStartInfo = new ProcessStartInfo("sendtx.exe", "-m 24 -s 15")
            //{
            //	UseShellExecute = true,
            //	CreateNoWindow = false
            //};
            int maxNodes = 0;
            int connectedNodes = 0;
            int informedNodes = 0;
            int requestedNodes = 0;
            int seenNodes = 0;

            bool error = false;

            using var process = new Process();
			process.StartInfo = processStartInfo;
			process.EnableRaisingEvents = true;

			process.OutputDataReceived += (sender, e) =>
			{

                if (e.Data.Contains("Max nodes to connect to:"))
                {
                    maxNodes = int.Parse(e.Data.Split(':')[1].Trim());
                }
                else if (e.Data.Contains("Successfully connected to nodes:"))
                {
                    connectedNodes = int.Parse(e.Data.Split(':')[1].Trim());
                }
                else if (e.Data.Contains("Informed nodes:"))
                {
                    informedNodes = int.Parse(e.Data.Split(':')[1].Trim());
                }
                else if (e.Data.Contains("Requested from nodes:"))
                {
                    requestedNodes = int.Parse(e.Data.Split(':')[1].Trim());
                }
                else if (e.Data.Contains("Seen on other nodes:"))
                {
                    seenNodes = int.Parse(e.Data.Split(':')[1].Trim());
                }
                else if (e.Data.Contains("Error"))
                {
                    error = true;
                }
                
                Trace.WriteLine(e.Data);
                
            };
			process.ErrorDataReceived += (sender, e) => Trace.WriteLine(e.Data);

			process.Start();
			process.BeginErrorReadLine();
			process.BeginOutputReadLine();

			process.StandardInput.WriteLine("sendtx.exe -m 24 -s 15 " + GetRawTransaction());

			try
			{
				process.WaitForExit((int)TimeSpan.FromSeconds(30).TotalMilliseconds + 50);
			}
			catch (OperationCanceledException)
			{
				try
				{

					process.Kill();
				}
				catch
				{

				}
				Trace.WriteLine("Process timed out after 30 seconds");
			}

            return !error && connectedNodes > 0 && seenNodes > 0;
		}
	}
	
}

