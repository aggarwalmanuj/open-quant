using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenQuant.API;

using Spider.OpenQuant.TradeClient5.Entities;
using Spider.OpenQuant.TradeClient5.Util;

namespace Spider.OpenQuant.TradeClient5.Base
{
    public abstract class BaseQuantityFocusedStrategy : BaseStrategy
    {
        protected SpiderPosition GetOpenPositionFromBroker()
        {
            string symbol = Instrument.Symbol;

            SpiderPosition currentPosition = null;

            BrokerAccount ibAccount = DataManager.GetBrokerInfo("IB").Accounts[IbAccountNumber];

            foreach (BrokerPosition currentBrokerPosition in ibAccount.Positions)
            {
                string posSymbol = currentBrokerPosition.Symbol.Trim().Replace(" ", "");
                bool isSymbolSame = string.Compare(symbol, posSymbol, StringComparison.InvariantCultureIgnoreCase) == 0;

                if (isSymbolSame && currentBrokerPosition.InstrumentType == InstrumentType.Stock)
                {
                    double? longPositionQuantity = currentBrokerPosition.LongQty;
                    double? shortPositionQuantity = currentBrokerPosition.ShortQty;

                    double? openQuantity = null;
                    PositionSide? openPositionSide = null;

                    if (longPositionQuantity.HasValue && longPositionQuantity.Value > 0)
                    {
                        openPositionSide = PositionSide.Long;
                        openQuantity = Math.Abs(longPositionQuantity.Value);
                    }
                    else if (shortPositionQuantity.HasValue && shortPositionQuantity.Value > 0)
                    {
                        openPositionSide = PositionSide.Short;
                        openQuantity = Math.Abs(shortPositionQuantity.Value);
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            string.Format("Could not retrieve an open position for {0} in account {1}",
                                this.Instrument.Symbol,
                                this.IbAccountNumber));
                    }

                    LoggingUtility.WriteInfo(this,
                        string.Format("***  Found an OPEN {0} position of {1} shares for {2} in account {3} ***",
                            openPositionSide.Value.ToString().ToUpper(),
                            openQuantity,
                            this.Instrument.Symbol.ToUpper(),
                            this.IbAccountNumber));

                    if (!openPositionSide.HasValue)
                    {
                        throw new InvalidOperationException(
                            string.Format("Could not retrieve an open position for {0} in account {1}",
                                this.Instrument.Symbol,
                                this.IbAccountNumber));
                    }

                    if (!openQuantity.HasValue)
                    {
                        throw new InvalidOperationException(
                            string.Format("Could not retrieve an open position for {0} in account {1}",
                                this.Instrument.Symbol,
                                this.IbAccountNumber));
                    }

                    currentPosition = new SpiderPosition()
                    {
                        Instrument = this.Instrument,
                        PositionSide = openPositionSide.Value,
                        Quantity = openQuantity.Value
                    };


                    break;
                }
            }

            return currentPosition;
        }

    }
}
