using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lib.Dogecoin.Interop
{
    internal class LibDogecoinTpmInterop
    {
        private const string DLL_NAME = "dogecoin";

        /* Generate a BIP39 mnemonic and encrypt it with the TPM */

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int dogecoin_generate_mnemonic_encrypt_with_tpm(
            [Out, MarshalAs(UnmanagedType.LPArray)] char[] mnemonic,
            int file_num,
            [MarshalAs(UnmanagedType.I1)] bool overwrite,
            [MarshalAs(UnmanagedType.LPArray)] char[] lang,
            [MarshalAs(UnmanagedType.LPArray)] char[] space,
            [MarshalAs(UnmanagedType.LPArray)] char[] words);


        /* Decrypt a BIP39 mnemonic with the TPM */
        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int dogecoin_decrypt_mnemonic_with_tpm(
            [Out, MarshalAs(UnmanagedType.LPArray)] char[] mnemonic,
            int file_num);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool dogecoin_list_encryption_keys_in_tpm(IntPtr names, out IntPtr count);


    }
}
