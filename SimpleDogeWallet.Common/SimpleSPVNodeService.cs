using SimpleDogeWallet.Common;
using Lib.Dogecoin;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Transactions;
using ZXing;
using System.IO;

namespace SimpleDogeWallet
{
	//TODO: Remove duplicate code
	public class SimpleSPVNodeService
	{
		public const string SPV_CHECKPOINT_FILE = "spvcheckpoint";

		public SPVNodeBlockInfo NEW_WALLET_START_BLOCK = new SPVNodeBlockInfo()
		{
			Hash = "dfabf09db6a20947965f8159bc3ec78252a3893124f53a32d3b13f12fdebd0e6",
			BlockHeight = 5330000,
			Timestamp = DateTimeOffset.FromUnixTimeSeconds(1723271918)
		};

		private SPVNode _spvNode;

		private uint _staleTimerLastBlock;
		private uint _staleTimerCounter = 0;

		private const int SPV_CHECKPOINT_BLOCKS_BEHIND = 30;



		long currentBlock = 0;
		long currentMinFee = long.MaxValue;
		long sumOfFees = 0;
		int txsWithFee = 0;

		long blockSize = 0;

		private LimitedQueue<decimal> blockSizes = new LimitedQueue<decimal>(30);
		private LimitedQueue<long> blockFees = new LimitedQueue<long>(30);
		private UTXOSampleIndex utxos = new UTXOSampleIndex(75000);


		

		public SimpleSPVNodeService()
		{
			TxCount = 0;
			CurrentBlock = NEW_WALLET_START_BLOCK;
			blockFees.Enqueue(1000);
		}


		public decimal EstimatedRate
		{
			get; set;
		}


		public uint EstimatedHeight
		{
			get
			{
				var diff = DateTimeOffset.UtcNow - CurrentBlock.Timestamp;
				return (uint)diff.TotalMinutes + CurrentBlock.BlockHeight;
			}
		}

		public SPVNodeBlockInfo CurrentBlock
		{
			get;
			private set;
		}

		public ulong TxCount
		{
			get; private set;
		}

		public ulong SpentUTXOCount
		{
			get; private set;
		}

		public ulong NewUTXOCount
		{
			get; private set;
		}

		public bool IsRunning
		{
			get
			{
				return _spvNode != null && _spvNode.IsRunning;
			}
		}


		private int _peerCount = 0;
		private long _timeBetweenRefresh = 500;
		private long _lastRefreshTime = 0;

		public int PeerCount
		{
			get
			{
                if (!IsRunning)
				{
					return 0;
				}

				var currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

				if(currentTime - _lastRefreshTime > _timeBetweenRefresh)
				{
					//peercount is an expensive call, so we only refresh so often
					_peerCount = _spvNode.GetPeerCount();
					_lastRefreshTime = currentTime;
				}

				return _peerCount;
			}
		}

		public decimal LastBlockSize
		{
			get
			{
				lock (blockSizes)
				{
					if (blockSizes.Count > 0)
					{
						return blockSizes.Last();
					}
				}

				return 0;
			}
		}

		public decimal AverageBlockSize
		{
			get
			{
				lock (blockSizes)
				{
					if (blockSizes.Count > 0)
					{
						return blockSizes.Average();
					}
				}

				return 0;
			}
		}


		public bool SyncCompleted
		{
			get
			{
				return _spvNode.SyncComplete;
			}
		}

		public void PrintDebug()
		{
			_spvNode?.PrintDebug();
		}

		public void Start(bool isNew = false)
		{
			if (_spvNode != null && (_spvNode.IsRunning || IsPaused))
			{
				return;
			}

			uint startingBlock = 0;


			if (isNew)
			{
				startingBlock = NEW_WALLET_START_BLOCK.BlockHeight;

				_spvNode = new SPVNodeBuilder()
					.StartAt(NEW_WALLET_START_BLOCK.Hash, NEW_WALLET_START_BLOCK.BlockHeight)
					.UseCheckpointFile(SPV_CHECKPOINT_FILE, SPV_CHECKPOINT_BLOCKS_BEHIND)
					.UseMainNet()
					.FullSync()
					.OnSyncCompleted(OnSyncComplete)
					.OnNextBlock(HandleOnBlock)
					.EnableDebug()
					.OnTransaction(HandleOnTransaction)
					.Build();
			}
			else
			{
				if(File.Exists(SPV_CHECKPOINT_FILE))
				{
					startingBlock = uint.Parse(File.ReadAllText(SPV_CHECKPOINT_FILE).Split(":")[1]);
				}


				_spvNode = new SPVNodeBuilder()
					.UseCheckpointFile(SPV_CHECKPOINT_FILE, SPV_CHECKPOINT_BLOCKS_BEHIND)
					.UseMainNet()
					.OnSyncCompleted(OnSyncComplete)
					.OnNextBlock(HandleOnBlock)
					.FullSync()
					.EnableDebug()
					.OnTransaction(HandleOnTransaction)
					.Build();
			}



			ClearUTXOsAfterCheckpoint(startingBlock);
			_spvNode.Start();

		}

		public void Stop()
		{
			_spvNode?.Stop();
			TxCount = 0;
			SpentUTXOCount = 0;
			NewUTXOCount = 0;
		}


		private SPVNodeBlockInfo _pauseBlock = null;
		public bool IsPaused { get; set; } = false;

		public void Pause()
		{
			if (_spvNode == null || !_spvNode.IsRunning)
			{
				return;
			}
			_pauseBlock = _spvNode.CurrentBlockInfo;
			_spvNode?.Stop();
			IsPaused = true;

		}


		public void Resume()
		{

			if (_spvNode == null || _spvNode.IsRunning)
			{
				return;
			}

			if(_pauseBlock == null || !IsPaused)
			{
				Start();
				return;
			}
			
			
			IsPaused = false;


			_spvNode = new SPVNodeBuilder()
					.StartAt(_pauseBlock)
				.UseCheckpointTracker(_spvNode.CheckpointTracker)
				.UseMainNet()
				.FullSync()
				.OnSyncCompleted(OnSyncComplete)
				.OnNextBlock(HandleOnBlock)
				.OnTransaction(HandleOnTransaction)
					.EnableDebug()
				.Build();

			ClearUTXOsAfterCheckpoint((uint)_pauseBlock.BlockHeight);

			_spvNode.Start();
		}




		public void Rescan(SPVNodeBlockInfo startPoint)
		{
			_spvNode?.Stop();

			utxos.Clear();
			blockFees.Clear();
			blockSizes.Clear();

			currentBlock = startPoint.BlockHeight;

			blockSize = 0;
			EstimatedRate = 1000;
			//	wallet.UTXOs.Clear();

			//wallet.Save();

			_spvNode = new SPVNodeBuilder()
					.StartAt(startPoint)
				.UseCheckpointFile(SPV_CHECKPOINT_FILE, SPV_CHECKPOINT_BLOCKS_BEHIND)
				.UseMainNet()
				.FullSync()
				.OnSyncCompleted(OnSyncComplete)
				.OnNextBlock(HandleOnBlock)
				.OnTransaction(HandleOnTransaction)
					.EnableDebug()
				.Build();

			ClearUTXOsAfterCheckpoint((uint)currentBlock);

			_spvNode.Start();

		}


		private void ClearUTXOsAfterCheckpoint(uint blockHeight)
		{
			SimpleDogeWallet.Instance.UTXOs.RemoveAll(u => u.BlockHeight > blockHeight);
		}

		private void OnSyncComplete()
		{
			Messenger.Default.Send(new UpdateSendButtonMessage());
		}

		private void HandleOnBlock(SPVNodeBlockInfo previous,  SPVNodeBlockInfo next)
		{
			if (!_spvNode.IsRunning)
			{
				return;
			}

			if (CurrentBlock.BlockHeight + 1 != next.BlockHeight)
			{
				Debug.WriteLine("Out of order? Ok on first block");
			}

			CurrentBlock = next;

			Messenger.Default.Send(next);
		}


		private unsafe void HandleOnTransaction(SPVNodeTransaction tx)
		{
			if(!_spvNode.IsRunning)
			{
				return;
			}

			bool walletChanged = false;

			TxCount++;

			if(tx.TxId.ToUpper() == SimpleDogeWallet.Instance.PendingTxHash?.ToUpper())
			{
				SimpleDogeWallet.Instance.PendingTxHash = string.Empty;
				SimpleDogeWallet.Instance.PendingAmount = 0;
			}

			SpentUTXOCount += (ulong)tx.In.Length;
			NewUTXOCount += (ulong)tx.Out.Length;

			foreach (var spentUtxo in tx.In)
			{
				UTXO targetUtxoToRemove = default;

				foreach(var utx in SimpleDogeWallet.Instance.UTXOs)
				{
					if(spentUtxo.TxId == utx.TxId && spentUtxo.VOut == utx.VOut)
					{
						walletChanged = true;
						targetUtxoToRemove = spentUtxo;
						break;
					}
				}
				if(targetUtxoToRemove != default)
				{
					SimpleDogeWallet.Instance.UTXOs.Remove(targetUtxoToRemove);
				}
			}

			foreach (var newUtxo in tx.Out)
			{
				var utxoAddress = LibDogecoinContext.Instance.UnsafeGetP2PKHAddress(newUtxo.ScriptPubKey);

				if (!string.IsNullOrEmpty(utxoAddress) &&
					utxoAddress == SimpleDogeWallet.Instance.Address &&
					!SimpleDogeWallet.Instance.UTXOs.Contains(newUtxo))
				{
						SimpleDogeWallet.Instance.UTXOs.Add(newUtxo);
						walletChanged = true;
					
				}
			}

			if (walletChanged)
			{
				SimpleDogeWallet.Instance.Save();

				Messenger.Default.Send(new UpdateSendButtonMessage());
			}




			//Fee Calculation
			
			if (tx.BlockHeight > currentBlock)
			{
				if (currentBlock != 0)
				{
					if (currentMinFee != long.MaxValue)
					{
						blockFees.Enqueue(currentMinFee);
					}


					EstimatedRate = ((decimal)(blockFees.Min()) / (decimal)100000000);
					
					currentMinFee = long.MaxValue;
				}

				currentBlock = tx.BlockHeight;
				
				blockSizes.Enqueue((decimal)blockSize / 1000000M);

				blockSize = 0;
			}


			bool allInputsPresent = true;
			long inputVal = 0;

			foreach (var input in tx.In)
			{
				var i = utxos.GetUTXOOrDefault(input.TxId, input.VOut);
				if (i == default(UTXO))
				{
					allInputsPresent = false;
					break;
				}
				inputVal += i.AmountKoinu.Value;
			}

			if (allInputsPresent)
			{
				var outputVal = tx.Out.Sum(o => o.AmountKoinu);
				var fee = (inputVal - outputVal) / tx.SizeBytes;

				if (fee <= currentMinFee)
				{
					currentMinFee = (long)fee;
				}
			}

			blockSize += tx.SizeBytes;

			foreach (var o in tx.Out)
			{
				utxos.Enqueue(o);
			}

			

		}

	}


	public class SPVUpdatedWalletMessage
	{

	}

	public class UpdateSendButtonMessage
	{

	}

	public class UpdateSPVTextMessage
	{

	}


	class UTXOSampleIndex : Queue<UTXO>
	{
		private readonly int _limit;
		private Dictionary<string, UTXO> _index;

		public UTXOSampleIndex(int limit) : base(limit)
		{
			_limit = limit;
			_index = new Dictionary<string, UTXO>(limit);
		}

		public new void Enqueue(UTXO item)
		{
			lock (this)
			{
				if (Count >= _limit)
				{
					var utxoToRemvoe = Dequeue(); // Kick out the oldest utxo
					_index.Remove(utxoToRemvoe.TxId + utxoToRemvoe.VOut);
				}
				if(_index.TryAdd(item.TxId + item.VOut, item))
				{
					base.Enqueue(item);
				}
			}
		}

		public UTXO GetUTXOOrDefault(string txId, int vout)
		{
			lock (this)
			{
				return _index.GetValueOrDefault(txId + vout);
			}
		}
	}

	class LimitedQueue<T> : Queue<T>
	{
		private readonly int _limit;

		public LimitedQueue(int limit) : base(limit)
		{
			_limit = limit;
		}

		public new void Enqueue(T item)
		{
			lock (this)
			{
				if (Count >= _limit)
				{
					Dequeue(); // Kick out the oldest item
				}
				base.Enqueue(item);

			}
		}
		
	}
}
