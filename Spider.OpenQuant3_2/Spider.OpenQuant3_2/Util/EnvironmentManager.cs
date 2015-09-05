using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider.OpenQuant3_2.Util
{
    public static class EnvironmentManager
    {
        public static string GetNormalizedMachineName()
        {
            return
                Environment.MachineName
                    .ToUpperInvariant()
                    .Replace("-", "")
                    .Replace("_", "");
        }
    }
}
