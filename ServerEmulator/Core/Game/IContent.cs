using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ServerEmulator.Core.Game
{
    [Obfuscation(Exclude = true)]
    interface IContent
    {
        void Load();
    }

}
