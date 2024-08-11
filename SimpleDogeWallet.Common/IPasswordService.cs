using Lib.Dogecoin;
using SimpleDogeWallet.Common.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextCopy;

namespace SimpleDogeWallet.Common
{
    internal interface IPasswordService
    {
        string CreatePassword();
        string GetPassword();
    }

    class TpmPasswordService : IPasswordService
    {
        private ITerminalSettings _settings;

        public TpmPasswordService(ITerminalSettings settings)
        {
            _settings = settings;
        }

        public string CreatePassword()
        {
            return LibDogecoinTpmContext.Instance.GenerateMnemonicEncryptWithTPM(_settings.GetInt("tpm-file-number"), lang: "eng", space: "-");
        }

        public string GetPassword()
        {
            return LibDogecoinTpmContext.Instance.DecryptMnemonicWithTPM(_settings.GetInt("tpm-file-number"));
        }
    }

    class BasicPasswordService : IPasswordService
    {
        private IServiceProvider _services;

        public BasicPasswordService(IServiceProvider services)
        {
            _services = services;
        }

        public string CreatePassword()
        {
            var nav = _services.GetService<Navigation>();
            string result = string.Empty;
            Task.Run(async () =>
            {
                var res = await nav.PromptAsync<PasswordPage>();

                result = res.Value as string ?? string.Empty;
            }).Wait();

            return result;
        }

        public string GetPassword()
        {
            var nav = _services.GetService<Navigation>();
            string result = string.Empty;
            Task.Run(async () =>
            {
                var res = await nav.PromptAsync<PasswordPage>();

                result = res.Value as string ?? string.Empty;
            }).Wait();

            return result;
        }
    }
}
