using System;
using System.Collections.Generic;
using Spider.Trading.OpenQuant3.Diagnostics;

namespace Spider.Trading.OpenQuant3.Util
{
    public class PositionSizeHolder
    {
        private Dictionary<string, double> PositionSizes = new Dictionary<string, double>(StringComparer.InvariantCultureIgnoreCase);
       
        internal PositionSizeHolder()
        {
        }

        internal void AddPositionSize(string symbol, double position)
        {
            lock (LockObjectManager.LockObject)
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