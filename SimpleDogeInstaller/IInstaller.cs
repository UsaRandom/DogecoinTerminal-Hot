using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleDogeInstaller
{
    internal interface IInstaller
    {
        void Install();
        void Uninstall();
    }
}
