using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using TextCopy;

namespace SimpleDogeWallet.Common.Pages
{
    [PageDef("Pages/Xml/PasswordPage.xml")]
    internal class PasswordPage : PromptPage
    {
        public PasswordPage(IPageOptions options) : base(options)
        {
            OnClick("BackButton", _ =>
            {
                Cancel();
            });

            OnClick("SubmitButton", _ =>
            {
                Submit(GetControl<TextInputControl>("PasswordInput").Text);
            });
        }
    }
}
