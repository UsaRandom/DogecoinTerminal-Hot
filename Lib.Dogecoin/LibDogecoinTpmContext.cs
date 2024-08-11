using Lib.Dogecoin.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace Lib.Dogecoin
{
    public abstract class LibDogecoinTpmContext
    {
        private static LibDogecoinTpmContext _instance;



        static LibDogecoinTpmContext()
        {
            LibdogecoinDllSelector.Select();
        }

        public static LibDogecoinTpmContext Instance
        {
            get
            {
                return _instance ?? CreateContext();
            }
        }

        public static LibDogecoinTpmContext CreateContext()
        {
            if (_instance == null)
            {
                if (LibdogecoinDllSelector.IsTpmSupported)
                {
                    _instance = new LibDogecoinTpmContextSupported();
                }
                else
                {
                    _instance = new LibDogecoinTpmContextUnsupported();
                }


                return _instance;
            }

            throw new Exception("Already using a LibDogecoinTpmContext.");
        }

        public abstract bool IsSupportedPlatform { get; }

        public abstract string GenerateMnemonicEncryptWithTPM(int fileNumber, bool overwrite = true, string lang = "eng", string space = " ");
        public abstract string DecryptMnemonicWithTPM(int fileNumber);

        public abstract string[] ListKeysInTPM();


    }

    class LibDogecoinTpmContextUnsupported : LibDogecoinTpmContext
    {
        public override bool IsSupportedPlatform => false;

        public override string DecryptMnemonicWithTPM(int fileNumber)
        {
            return string.Empty;
        }

        public override string GenerateMnemonicEncryptWithTPM(int fileNumber, bool overwrite = true, string lang = "eng", string space = " ")
        {
            return string.Empty;
        }

        public override string[] ListKeysInTPM()
        {
            return new string[0];
        }
    }

    class LibDogecoinTpmContextSupported : LibDogecoinTpmContext
    {

        public override bool IsSupportedPlatform => true;


        public override string GenerateMnemonicEncryptWithTPM(int fileNumber, bool overwrite = true, string lang = "eng", string space = " ")
        {
            var mnemonic = new char[2048];

            LibDogecoinTpmInterop.dogecoin_generate_mnemonic_encrypt_with_tpm(mnemonic, fileNumber, overwrite, lang.NullTerminate(), space.NullTerminate(), null);


            return mnemonic.TerminateNull();
        }



        public override string DecryptMnemonicWithTPM(int fileNumber)
        {
            var mnemonic = new char[2048];

            LibDogecoinTpmInterop.dogecoin_decrypt_mnemonic_with_tpm(mnemonic, fileNumber);


            return mnemonic.TerminateNull();
        }


        public override string[] ListKeysInTPM()
        {
            //TODO: This can be simplified by just looking at the ./source/ folder where they are stored.
            var keyNames = new List<string>();
            int count;
            IntPtr countPtr;
            bool result;

            count = 1000;

            IntPtr[] keyNamePointers = new IntPtr[count];

            result = LibDogecoinTpmInterop.dogecoin_list_encryption_keys_in_tpm(Marshal.UnsafeAddrOfPinnedArrayElement(keyNamePointers, 0), out countPtr);

            // Check the result
            if (result)
            {
                // Retrieve the key names
                for (int i = 0; i < count; i++)
                {
                    if (keyNamePointers[i] == IntPtr.Zero)
                    {
                        break;
                    }
                    else
                    {
                        keyNames.Add(Marshal.PtrToStringUni(keyNamePointers[i]));

                        LibDogecoinInterop.dogecoin_free(keyNamePointers[i]);
                    }
                }

            }

            Marshal.FreeCoTaskMem(countPtr);

            return keyNames.ToArray();
        }

    }
}
