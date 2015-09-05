using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQuant.API;

namespace Spider.OpenQuant.Strategies.Util
{
    public class StopPriceManager
    {
        private static readonly object LockObject = new object();
        private Dictionary<string, double> StopPrices = new Dictionary<string, double>(StringComparer.InvariantCultureIgnoreCase);
        private LoggingConfig logConfig = null;

        public StopPriceManager(LoggingConfig logConfig)
        {
            this.logConfig = logConfig;
        }

        internal void AddStopPrice(string symbol, double price)
        {
            lock (LockObject)
            {
                StopPrices[symbol] = price;
            }
        }

        internal bool IsExitStopPriceMet(double lastPrice, double stopPrice, PositionSide positionSide)
        {
            bool stopMet = false;
            if (positionSide == PositionSide.Short)
                stopMet = lastPrice >= stopPrice;
            else
                stopMet = lastPrice <= stopPrice;

            if (stopMet)
                LoggingUtility.WriteInfo(
                    logConfig,
                    string.Format(
                        "Stop price of {0:c} was met on {1} side by last close price of {2:c}",
                        stopPrice,
                        positionSide,
                        lastPrice));

            return stopMet;
        }

        internal bool IsEntryStopPriceMet(double lastPrice, double stopPrice, OrderSide orderSide)
        {
            bool stopMet = false;
            if (orderSide == OrderSide.Buy)
                stopMet = lastPrice >= stopPrice;
            else
                stopMet = lastPrice <= stopPrice;

            if (stopMet)
                LoggingUtility.WriteInfo(
                    logConfig,
                    string.Format(
                        "Stop price of {0:c} was met on {1} side by last close price of {2:c}",
                        stopPrice,
                        orderSide,
                        lastPrice));

            return stopMet;
        }


        internal double? GetStopPrice(string symbol)
        {
            if (StopPrices.ContainsKey(symbol))
                return StopPrices[symbol];
            return null;
        }
    }
}
