using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider.Trading.OpenQuant3.Util
{
    public static class LockObjectManager
    {
        public static readonly object LockObject = new object();
    }
}
