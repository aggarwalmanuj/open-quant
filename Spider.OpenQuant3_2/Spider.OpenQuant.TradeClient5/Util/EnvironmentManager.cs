using System;

namespace Spider.OpenQuant.TradeClient5.Util
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
