using System;
using System.Linq;
using System.Text;
using OpenQuant.API;
using Spider.Trading.OpenQuant3.Common;
using Spider.Trading.OpenQuant3.Diagnostics;

namespace Spider.Trading.OpenQuant3.Util
{
    public static class SolutionUtility
    {
        private static readonly LoggingConfig dummyLoggingConfig = new LoggingConfig()
                                                                       {
                                                                           IsDebugEnabled = true,
                                                                           IsInfoEnabled = true,
                                                                           IsWarnEnabled = true
                                                                       };
    

        public static double GetOpenQuantityForSymbol(string symbol, string ibAccountNumber)
        {
            double returnValue = 0;
            double? longPositionQuantity = null;
            double? shortPositionQuantity = null;

           
            BrokerAccount IBaccount = DataManager.GetBrokerInfo("IB").Accounts[ibAccountNumber];

            foreach (BrokerPosition currentBrokerPosition in IBaccount.Positions)
            {
                string brokerPositionSymbol = GetBrokerPositionSymbol(currentBrokerPosition);
                bool isSymbolSame = string.Compare(symbol, brokerPositionSymbol, StringComparison.InvariantCultureIgnoreCase) == 0;

                

                if (isSymbolSame && currentBrokerPosition.InstrumentType == InstrumentType.Stock)
                {
                    longPositionQuantity = currentBrokerPosition.LongQty;
                    shortPositionQuantity = currentBrokerPosition.ShortQty;

                    break;
                }

            }

            if (longPositionQuantity.HasValue && longPositionQuantity.Value > 0)
                returnValue = Math.Abs(longPositionQuantity.Value);
              
            if (shortPositionQuantity.HasValue && shortPositionQuantity.Value > 0)
                returnValue = Math.Abs(shortPositionQuantity.Value)*-1;

            return returnValue;
        }

        public static string GetBrokerPositionSymbol(BrokerPosition position)
        {
            return position.Symbol.Trim().Replace(" ", "");
        }

        public static PositionSide GetPositionSide(string symbol, string ibAccountNumber)
        {
            PositionSide returnValue = PositionSide.Long;
            double currentQuantity = GetOpenQuantityForSymbol(symbol, ibAccountNumber);

            if (currentQuantity == 0)
                throw new ArgumentOutOfRangeException("Could not find any open quantity for " + symbol);

            if (currentQuantity > 0)
                returnValue = PositionSide.Long;
            else
                returnValue = PositionSide.Short;

            return returnValue;
        }

        public static OrderSide GetOrderSideToCloseCurrentPosition(string symbol, string ibAccountNumber)
        {
            OrderSide returnValue = OrderSide.Buy;

            if (GetPositionSide(symbol, ibAccountNumber) == PositionSide.Long)
                returnValue = OrderSide.Sell;
            else
                returnValue = OrderSide.Buy;

            return returnValue;
        }

        public static bool IsInstrumentIwm(Instrument instrument)
        {
            return
                string.Compare(instrument.Symbol, IwmStopTriggerConstants.IwmSymbol,
                               StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        public static void AddPositionSize(string symbol, PositionSide positionSide, double positionSize)
        {
            double qty = (positionSide == PositionSide.Long) ? Math.Abs(positionSize) : Math.Abs(positionSize)*-1;
            PositionSizeHolder.AddPositionSize(symbol, qty);
        }

        public static void AddStopPrice(string symbol, double price)
        {
            StopPriceHolder.AddStopPrice(symbol, price);
        }

        public static void ClearStopPrices()
        {
            StopPriceHolder.ClearStops();
        }

        internal static PositionSizeHolder PositionSizeHolder
        {
            get
            {
                if (!Strategy.Global.ContainsKey(GlobalStorageKeys.GLOBAL_POS_SIZE_HOLDER))
                {
                    lock (LockObjectManager.LockObject)
                    {
                        if (!Strategy.Global.ContainsKey(GlobalStorageKeys.GLOBAL_POS_SIZE_HOLDER))
                            Strategy.Global[GlobalStorageKeys.GLOBAL_POS_SIZE_HOLDER] = new PositionSizeHolder();
                    }
                }
                return Strategy.Global[GlobalStorageKeys.GLOBAL_POS_SIZE_HOLDER] as PositionSizeHolder ;
            }
        }

        internal static StopPriceHolder StopPriceHolder
        {
            get
            {
                if (!Strategy.Global.ContainsKey(GlobalStorageKeys.GLOBAL_STOP_HOLDER))
                {
                    lock (LockObjectManager.LockObject)
                    {
                        if (!Strategy.Global.ContainsKey(GlobalStorageKeys.GLOBAL_STOP_HOLDER))
                            Strategy.Global[GlobalStorageKeys.GLOBAL_STOP_HOLDER] = new StopPriceHolder();
                    }
                }
                return Strategy.Global[GlobalStorageKeys.GLOBAL_STOP_HOLDER] as StopPriceHolder;
            }
        }


    }
}
