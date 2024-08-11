using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lib.Dogecoin.Interop
{
    /*
	 * Note,
	 * 
	 * This is not a complete set of bindings for libdogecoin, but a starting point
	 * for those who want to build ontop of libdogecoin. 
	 * 
	 * Please see the dogecoin foundation's repo for infomration on how to build
	 * and use libdogecoin. 
	 */
    internal static class LibDogecoinInterop
    {
        private const string DLL_NAME = "dogecoin";




        //!init static ecc context
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void dogecoin_ecc_start();

        //!destroys the static ecc context
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void dogecoin_ecc_stop();

        /* generates a private and public keypair (a wallet import format private key and a p2pkh ready-to-use corresponding dogecoin address)*/
        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int generatePrivPubKeypair(
            [Out, MarshalAs(UnmanagedType.LPArray)] char[] wif_privkey,
            [Out, MarshalAs(UnmanagedType.LPArray)] char[] p2pkh_pubkey,
            [MarshalAs(UnmanagedType.I1)] bool is_testnet
        );

        /* generates a hybrid deterministic WIF master key and p2pkh ready-to-use corresponding dogecoin address. */
        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int generateHDMasterPubKeypair(
            [Out, MarshalAs(UnmanagedType.LPArray)] char[] wif_privkey_master,
            [Out, MarshalAs(UnmanagedType.LPArray)] char[] p2pkh_pubkey_master,
            [MarshalAs(UnmanagedType.I1)] bool is_testnet
        );

        /* generates a new dogecoin address from a HD master key */
        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int generateDerivedHDPubkey(
            [MarshalAs(UnmanagedType.LPArray)] char[] wif_privkey_master,
            [Out, MarshalAs(UnmanagedType.LPArray)] char[] p2pkh_pubkey
        );

        /* get derived hd address by custom path */
        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int getDerivedHDAddressByPath(
            [MarshalAs(UnmanagedType.LPArray)] char[] masterkey,
            [MarshalAs(UnmanagedType.LPArray)] char[] derived_path,
            [Out, MarshalAs(UnmanagedType.LPArray)] char[] outaddress,
            [MarshalAs(UnmanagedType.I1)] bool outprivkey
        );



        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr getHDNodePrivateKeyWIFByPath(
            [MarshalAs(UnmanagedType.LPArray)] char[] masterkey,
            [MarshalAs(UnmanagedType.LPArray)] char[] derived_path,
            [Out, MarshalAs(UnmanagedType.LPArray)] char[] outaddress,
            [MarshalAs(UnmanagedType.I1)] bool outprivkey
        );

        /* verify that a private key and dogecoin address match */
        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int verifyPrivPubKeypair(
            [MarshalAs(UnmanagedType.LPArray)] char[] wif_privkey,
            [MarshalAs(UnmanagedType.LPArray)] char[] p2pkh_pubkey,
            [MarshalAs(UnmanagedType.I1)] bool is_testnet
        );

        /* verify that a HD Master key and a dogecoin address matches */
        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int verifyHDMasterPubKeypair(
            [MarshalAs(UnmanagedType.LPArray)] char[] wif_privkey_master,
            [MarshalAs(UnmanagedType.LPArray)] char[] p2pkh_pubkey_master,
            [MarshalAs(UnmanagedType.I1)] bool is_testnet
        );

        /* verify that a HD Master key and a dogecoin address matches */
        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int verifyP2pkhAddress(
            [MarshalAs(UnmanagedType.LPArray)] char[] p2pkh_pubkey,
            uint len
        );

        /* Generates an English mnemonic phrase from given hex entropy */
        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int generateEnglishMnemonic(
            [MarshalAs(UnmanagedType.LPArray)] char[] entropy,
            [MarshalAs(UnmanagedType.LPArray)] char[] size,
            [Out, MarshalAs(UnmanagedType.LPArray)] char[] mnemonic
        );

        /* Generates a random (e.g. "128" or "256") English mnemonic phrase */
        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int generateRandomEnglishMnemonic(
            [MarshalAs(UnmanagedType.LPArray)] char[] size,
            [Out, MarshalAs(UnmanagedType.LPArray)] char[] mnemonic
        );


        /* Generates a HD master key and p2pkh ready-to-use corresponding dogecoin address from a mnemonic */
        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int getDerivedHDAddressFromMnemonic(
            uint account,
            uint index,
            [MarshalAs(UnmanagedType.LPArray)] char[] change_level,
            [MarshalAs(UnmanagedType.LPArray)] char[] mnemonic,
            [MarshalAs(UnmanagedType.LPArray)] char[] pass,
            [Out, MarshalAs(UnmanagedType.LPArray)] char[] p2pkh_pubkey,
            [MarshalAs(UnmanagedType.I1)] bool is_testnet
        );


        /* sign a message with a private key */
        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr sign_message(
            [MarshalAs(UnmanagedType.LPArray)] char[] privkey,
            [MarshalAs(UnmanagedType.LPArray)] char[] msg
        );


        /* verify a message with a address */
        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool verify_message(
            [MarshalAs(UnmanagedType.LPArray)] char[] sig,
            [MarshalAs(UnmanagedType.LPArray)] char[] msg,
            [MarshalAs(UnmanagedType.LPArray)] char[] address
        );


        /* create a QR text formatted string (with line breaks) from an incoming p2pkh*/
        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int qrgen_p2pkh_to_qr_string(
            [MarshalAs(UnmanagedType.LPArray)] char[] in_p2pkh,
            [Out, MarshalAs(UnmanagedType.LPArray)] char[] outString
        );


        /* Creates a .png file with the filename outFilename, from string inString, w. size factor of SizeMultiplier.*/

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int qrgen_string_to_qr_pngfile(
            [MarshalAs(UnmanagedType.LPArray)] char[] outFilename,
            [MarshalAs(UnmanagedType.LPArray)] char[] inString,
            byte sizeMultiplier
        );

        /* Creates a .jpg file with the filename outFilename, from string inString, w. size factor of SizeMultiplier.*/
        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int qrgen_string_to_qr_jpgfile(
            [MarshalAs(UnmanagedType.LPArray)] char[] outFilename,
            [MarshalAs(UnmanagedType.LPArray)] char[] inString,
            byte sizeMultiplier
        );


        /* create a new dogecoin transaction: Returns the (txindex) in memory of the transaction being worked on. */
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int start_transaction();


        /* add a utxo to the transaction being worked on at (txindex), specifying the utxo's txid and vout. returns 1 if successful.*/
        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int add_utxo(
            int txindex,
            [MarshalAs(UnmanagedType.LPArray)] char[] hex_utxo_txid,
            int vout
        );




        /* add an output to the transaction being worked on at (txindex) of amount (amount) in dogecoins, returns 1 if successful. */
        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int add_output(
            int txindex,
            [MarshalAs(UnmanagedType.LPArray)] char[] destinationaddress,
            [MarshalAs(UnmanagedType.LPArray)] char[] amount
        );


        /* finalize the transaction being worked on at (txindex), with the (destinationaddress) paying a fee of (subtractedfee), */
        /* re-specify the amount in dogecoin for verification, and change address for change. If not specified, change will go to the first utxo's address. */
        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr finalize_transaction(
            int txindex,
            [MarshalAs(UnmanagedType.LPArray)] char[] destinationaddress,
            [MarshalAs(UnmanagedType.LPArray)] char[] subtractedfee,
            [Out, MarshalAs(UnmanagedType.LPArray)] char[] out_dogeamount_for_verification,
            [MarshalAs(UnmanagedType.LPArray)] char[] changeaddress
        );


        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sign_raw_transaction(int inputindex,

            IntPtr incomingrawtx,
            [MarshalAs(UnmanagedType.LPArray)] char[] scripthex,
            int sighashtype,
            [MarshalAs(UnmanagedType.LPArray)] char[] privkey);




        /**
		 * @brief This function generates a HD master key and p2pkh ready-to-use corresponding dogecoin address from a mnemonic.
		 *
		 * @param hd_privkey_master The generated master private key.
		 * @param p2pkh_pubkey_master The generated master public key.
		 * @param mnemonic The mnemonic code words.
		 * @param passphrase The passphrase (optional).
		 * @param is_testnet The flag denoting which network, 0 for mainnet and 1 for testnet.
		 *
		 * return: 0 (success), -1 (fail)
		 */
        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int generateHDMasterPubKeypairFromMnemonic
            ([Out, MarshalAs(UnmanagedType.LPArray)] char[] hd_privkey_master,
            [Out, MarshalAs(UnmanagedType.LPArray)] char[] p2pkh_pubkey_master,
            [MarshalAs(UnmanagedType.LPArray)] char[] mnemonic,
            [MarshalAs(UnmanagedType.LPArray)] char[] pass,
            [MarshalAs(UnmanagedType.I1)] bool is_testnet);



        /* retrieve the raw transaction at (txindex) as a hex string (char*) */
        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr get_raw_transaction(
            int txindex
        );



        /* sign a raw transaction in memory at (txindex), sign (inputindex) with (scripthex) of (sighashtype), with (privkey) */
        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sign_transaction(
            int txindex,
            [MarshalAs(UnmanagedType.LPArray)] char[] script_pubkey,
            [MarshalAs(UnmanagedType.LPArray)] char[] privkey
        );

        /* Sign a formed transaction with working transaction index (txindex), prevout.n index (vout_index) and private key (privkey) */
        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sign_transaction_w_privkey(
            int txindex,
            int vout_index,
            [MarshalAs(UnmanagedType.LPArray)] char[] privkey
        );


        /**
		 * It takes a p2pkh address and converts it to a compressed public key in
		 * hexadecimal format. It then strips the network prefix and checksum
		 */
        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte dogecoin_p2pkh_address_to_pubkey_hash(
            [MarshalAs(UnmanagedType.LPArray)] char[] p2pkh,
            [Out, MarshalAs(UnmanagedType.LPArray)] char[] pubkeyHash
        );

        /* clear all internal working transactions */
        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void remove_all();

        /* clear the transaction at (txindex) in memory */
        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void clear_transaction(int txindex);





        /* Koinu functions */

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int koinu_to_coins_str(
            ulong koinu,
            [Out, MarshalAs(UnmanagedType.LPArray)] char[] str
        );



        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong coins_to_koinu_str(
            [MarshalAs(UnmanagedType.LPArray)] char[] coins
        );


        /* Memory functions */

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr dogecoin_char_vla(uint size);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void dogecoin_free(IntPtr ptr);




		[DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		public static extern int dogecoin_generate_mnemonic(
			[MarshalAs(UnmanagedType.LPArray)] char[] entropy_size,
			[MarshalAs(UnmanagedType.LPArray)] char[] language,
			[MarshalAs(UnmanagedType.LPArray)] char[] space,
			[MarshalAs(UnmanagedType.LPArray)] char[] entropy,
			[MarshalAs(UnmanagedType.LPArray)] char[] filepath,
			[Out, MarshalAs(UnmanagedType.LPArray)] char[] entropy_out,
            [Out] out int size,
            [Out, MarshalAs(UnmanagedType.LPArray)] char[] words);



        #region SPV


        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr dogecoin_spv_client_new(
            IntPtr chainparams,
            [MarshalAs(UnmanagedType.U1)] bool debug,
            [MarshalAs(UnmanagedType.U1)] bool headers_memonly,
            [MarshalAs(UnmanagedType.U1)] bool use_checkpoints,
            [MarshalAs(UnmanagedType.U1)] bool full_sync,
            int maxNodes,
             IntPtr httpServer);
            




        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr chain_from_b58_prefix(
            [MarshalAs(UnmanagedType.LPArray)] char[] address
            );

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern dogecoin_tx_out_type dogecoin_script_classify(
             cstring* script,
             vector* data_out
        );

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void dogecoin_spv_client_runloop(
            IntPtr client
        );

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void dogecoin_spv_client_discover_peers(IntPtr client, [In, MarshalAs(UnmanagedType.LPStr)] string ips);


        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern vector* vector_new(ulong res, IntPtr free_f);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void vector_remove_idx(vector vec, int idx);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool dogecoin_p2pkh_addr_from_hash160(
            byte[] hashin,
            IntPtr chain,
            [Out, MarshalAs(UnmanagedType.LPArray)] char[] addrout,
            uint len);


        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern void vector_free(vector* vec, bool free_array);


        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern dogecoin_tx_out_type dogecoin_script_classify_ops(vector* ops);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void dogecoin_tx_hash(IntPtr tx, [Out] byte[] hashout);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void dogecoin_node_group_shutdown(IntPtr group);

		[DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		public static extern void dogecoin_node_send(IntPtr node, IntPtr data);

		[DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe cstring* dogecoin_p2p_message_new(char[] netmagic, char[] command, IntPtr data, uint data_len);

		[DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		public static extern void dogecoin_spv_client_free(IntPtr spvClient);

		[DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe cstring* cstr_new(char[] str);

		[DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void cstr_free(cstring* s, int free_buf);

		[DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool dogecoin_spv_client_load(IntPtr spvClient, char[] file);

		[DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool broadcast_raw_tx(IntPtr chain, char[] raw_hex_tx);



		[DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		public static extern void dogecoin_node_disconnect(IntPtr node);
		[DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		public static extern void dogecoin_node_free(IntPtr node);


		[DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		public static extern void dogecoin_node_group_free(IntPtr group);


		[DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		public static extern void dogecoin_node_group_event_loop(IntPtr group);


        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void dogecoin_node_group_event_loopbreak(IntPtr group);



        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		public static extern void dogecoin_node_group_connect_next_nodes(IntPtr group);


		[DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		public static unsafe extern void dogecoin_tx_serialize(cstring* s, dogecoin_tx* tx);

		[DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		public static unsafe extern cstring* cstr_new_sz(int sz);


        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr utils_hex_to_uint8(char[] chars);

		#endregion SPV

	}
}
