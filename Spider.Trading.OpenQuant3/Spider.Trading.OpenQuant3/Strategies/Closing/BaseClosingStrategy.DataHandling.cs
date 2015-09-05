using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQuant.API;
using Spider.Trading.OpenQuant3.Calculations;
using Spider.Trading.OpenQuant3.Diagnostics;
using Spider.Trading.OpenQuant3.Entities;
using Spider.Trading.OpenQuant3.Exceptions;
using Spider.Trading.OpenQuant3.Util;

namespace Spider.Trading.OpenQuant3.Strategies.Closing
{
    public abstract partial class BaseClosingStrategy : BaseStrategy
    {


        protected double? longPositionQuantity = null;
        protected double? shortPositionQuantity = null;

        protected override OrderSide GetEffectiveOrderSide()
        {
            if (IsCurrentInstrumentIwmAndIgnorable())
                return OrderSideForIwmStopTriggerForClosing;

            PositionSide pos = GetOpenPositionSide();
            if (pos == PositionSide.Long)
                return OrderSide.Sell;
            else
                return OrderSide.Buy;
        }

        protected internal PositionSide GetOpenPositionSide()
        {
            if (longPositionQuantity.HasValue && longPositionQuantity.Value > 0)
            {
                return PositionSide.Long;
            }

            if (shortPositionQuantity.HasValue && shortPositionQuantity.Value > 0)
            {
                return PositionSide.Short;
            }

            return PositionSide.Long;
        }

        protected internal double GetAbsoluteOpenQuantity()
        {
            if (longPositionQuantity.HasValue && longPositionQuantity.Value > 0)
            {
                return longPositionQuantity.Value;
            }

            if (shortPositionQuantity.HasValue && shortPositionQuantity.Value > 0)
            {
                return shortPositionQuantity.Value;
            }

            return 0;
        }

        private double GetTargetQuantity()
        {
            double targetQuantity;
            ClosingQuantityCalculatorInput input = new ClosingQuantityCalculatorInput()
                                                       {
                                                           PositionSizePercentage = PositionSizePercentage,
                                                           RoundLots = RoundLots,
                                                           MinimumPosition = MinimumOrderSize
                                                       };

            QuantityCalculator qtyCalc = new QuantityCalculator(LoggingConfig);

            targetQuantity = qtyCalc.CalculateClosingQuantity(GetAbsoluteOpenQuantity(), input);
            return targetQuantity;
        }


        protected void QueueUpClosingOrder()
        {
            RefreshOpenQuantity(false);
        }

        private void RefreshOpenQuantity(bool onlyGetFromBroker)
        {

            if (string.IsNullOrWhiteSpace(IbBrokerAccountNumber))
            {
                LoggingUtility.WriteError(LoggingConfig,
                    "IB BROKER ACCOUNT NUMBER NOT SET!!!");
            }

            string symbol = Instrument.Symbol;
            BrokerAccount IBaccount = DataManager.GetBrokerInfo("IB").Accounts[IbBrokerAccountNumber];

            foreach (BrokerPosition currentBrokerPosition in IBaccount.Positions)
            {
                string brokerPositionSymbol = SolutionUtility.GetBrokerPositionSymbol(currentBrokerPosition);
                bool isSymbolSame = string.Compare(symbol, brokerPositionSymbol, StringComparison.InvariantCultureIgnoreCase) == 0;


                if (isSymbolSame && currentBrokerPosition.InstrumentType == InstrumentType.Stock)
                {
                    double? specifiedQty = SolutionUtility.PositionSizeHolder.GetPositionSize(symbol);

                    if (onlyGetFromBroker)
                        specifiedQty = null;

                    if (specifiedQty.HasValue)
                    {
                        if (specifiedQty.Value > 0)
                        {
                            longPositionQuantity = Math.Abs(specifiedQty.Value);
                            shortPositionQuantity = 0;
                        }
                        else
                        {
                            shortPositionQuantity = Math.Abs(specifiedQty.Value);
                            longPositionQuantity = 0;
                        }
                    }
                    else
                    {
                        longPositionQuantity = currentBrokerPosition.LongQty;
                        shortPositionQuantity = currentBrokerPosition.ShortQty;
                    }

                    break;
                }

            }

            if (longPositionQuantity.HasValue && longPositionQuantity.Value > 0)
                LoggingUtility.WriteInfo(LoggingConfig,
                                         string.Format("Found an open LONG position of {0}", longPositionQuantity.Value));

            if (shortPositionQuantity.HasValue && shortPositionQuantity.Value > 0)
                LoggingUtility.WriteInfo(LoggingConfig,
                                         string.Format("Found an open SHORT position of {0}", shortPositionQuantity.Value));


        }



    }
}
