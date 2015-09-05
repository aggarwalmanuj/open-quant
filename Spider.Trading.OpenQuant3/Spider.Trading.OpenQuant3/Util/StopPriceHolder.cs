using System;
using System.Collections.Generic;
using Spider.Trading.OpenQuant3.Diagnostics;

namespace Spider.Trading.OpenQuant3.Util
{
    public class StopPriceHolder
    {
        private Dictionary<string, double> StopPrices = new Dictionary<string, double>(StringComparer.InvariantCultureIgnoreCase);
        
        
        internal void AddStopPrice(string symbol, double price)
        {
            lock (LockObjectManager.LockObject)
            {
                StopPrices[symbol] = price;
            }
        }

        internal double? GetStopPrice(string symbol)
        {
            if (StopPrices.ContainsKey(symbol))
                return StopPrices[symbol];
            return null;
        }

        public void ClearStops()
        {
            lock (LockObjectManager.LockObject)
            {
                StopPrices.Clear();
            }
        }
    }
}
