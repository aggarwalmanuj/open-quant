using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQuant.API;
using Spider.Trading.OpenQuant3.Calculations;
using Spider.Trading.OpenQuant3.Common;
using Spider.Trading.OpenQuant3.Diagnostics;
using Spider.Trading.OpenQuant3.Enums;
using Spider.Trading.OpenQuant3.Util;
using Spider.Trading.OpenQuant3.Validators;

namespace Spider.Trading.OpenQuant3
{
    public abstract partial class BaseStrategy
    {

        protected bool IsCurrentInstrumentIwm()
        {
            return SolutionUtility.IsInstrumentIwm(Instrument);
        }

        protected internal bool IsCurrentInstrumentIwmAndIgnorable()
        {
            return IsCurrentInstrumentIwm() && !IsIwmAlsoToBeIncludedInOrder;
        }


        public double GetPriceAsPercentageOfPreviousClosingPrice(double price)
        {
            if (EffectivePreviousClosePrice != 0)
                return (price / EffectivePreviousClosePrice);
            return 0;
        }

        public double GetPriceDiffAsPercentageOfPreviousClosingPrice(double price)
        {
            if (EffectivePreviousClosePrice != 0)
            {
                double diff = price - EffectivePreviousClosePrice;
                return (diff / EffectivePreviousClosePrice);
            }
            return 0;
        }


        public double GetSecondsLeftInSessionEnd(Bar bar)
        {
            double timeDifferenceInSessionClose = 0;

            if (bar.BeginTime.IsWithinRegularTradingHours(Instrument.Type)
                && bar.BeginTime >= EffectiveStartOfSessionTime)
            {
                timeDifferenceInSessionClose = Math.Abs((new DateTime(ValidityTriggerDate.Date.Year,
                                                                      ValidityTriggerDate.Date.Month,
                                                                      ValidityTriggerDate.Date.Day)
                                                            .AddSeconds(PstSessionTimeConstants.StockExchangeEndTimeSeconds)
                                                            .Subtract(bar.BeginTime)).TotalSeconds);
            }

            return timeDifferenceInSessionClose;
        }
        
        protected void PlaceTargetOrder(double qty, double price, string name)
        {
            price = new PriceCalculator(LoggingConfig).RoundPrice(price, Instrument);


            if (TargetOrderType == OrderType.Market)
                StrategyOrder = MarketOrder(Instrument, EffectiveOrderSide, qty, name);
            else if (TargetOrderType == OrderType.Limit)
                StrategyOrder = LimitOrder(Instrument, EffectiveOrderSide, qty, price, name);
            else if (TargetOrderType == OrderType.MarketOnClose)
            {
                StrategyOrder = MarketOrder(Instrument, EffectiveOrderSide, qty, name);
                StrategyOrder.Type = OrderType.MarketOnClose;
            }

            if (string.IsNullOrWhiteSpace(IbBrokerAccountNumber))
            {
                LoggingUtility.WriteError(LoggingConfig, "IB BROKER NUMBER IS NOT SET. NO ORDER SUBMITTED");
                return;
            }

            StrategyOrder.Account = IbBrokerAccountNumber;
            
            if (AutoSubmit)
                StrategyOrder.Send();

            //CurrentStrategyOrdersList.Add(newOrder);
        }



        protected string GetAutoPlacedOrderName(OrderType orderType, OrderSide orderSide, string info, string instrument, int retrials, string ibAccountNumber)
        {
            if (string.IsNullOrEmpty(info))
                return string.Format("ACCT: {4} -- {0}: {1} order for {2} [#{3}]", orderType, orderSide, instrument, retrials, ibAccountNumber);
            else
                return string.Format("ACCT: {5} -- {0}: {1} ({2}) order {3} [#{4}]", orderType, orderSide, info, instrument, retrials, ibAccountNumber);
        }


       

        protected bool IsStopPriceApplicableToThisInstrument()
        {
            if (OrderTriggerStrategy == OrderTriggerStrategy.StopPriceBasedTrigger)
                return true;

            if (OrderTriggerStrategy == OrderTriggerStrategy.IwmStopPriceBasedTrigger && IsCurrentInstrumentIwm())
                return true;

            return false;
        }
    }
}
