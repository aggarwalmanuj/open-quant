using System;
using System.Collections.Generic;

namespace Spider.OpenQuant.Strategies.Util
{
    public class PositionSizeManager
    {
        private static readonly object LockObject = new object();
        private Dictionary<string, double> PositionSizes = new Dictionary<string, double>(StringComparer.InvariantCultureIgnoreCase);
        private LoggingConfig logConfig = null;

        public PositionSizeManager(LoggingConfig logConfig)
        {
            this.logConfig = logConfig;
        }

        internal void AddPositionSize(string symbol, double position)
        {
            lock (LockObject)
            {
                PositionSizes[symbol] = position;
            }
        }

        internal double? GetPositionSize(string symbol)
        {
            if (PositionSizes.ContainsKey(symbol))
                return PositionSizes[symbol];
            return null;
        }
    }
}