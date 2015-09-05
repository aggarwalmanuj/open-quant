using System;
using System.Collections.Generic;
using System.Linq;

using OpenQuant.API;

using Spider.OpenQuant.TradeClient5.Common;
using Spider.OpenQuant.TradeClient5.Extensions;
using Spider.OpenQuant.TradeClient5.Util;

namespace Spider.OpenQuant.TradeClient5.Base
{
    public abstract partial class BaseStrategy
    {
        public DateTime? LastOrderCompletedAt { get; set; }

        protected double MaximumOrderSize { get; set; }

        protected Order CurrentOrder { get; set; }

        public double CurrentLastOrderPrice = 0;

        protected DateTime OrderQueuedTime { get; set; }

        protected double RandomDelayBetweenOrdersInSeconds { get; set; }

        protected int RandomRetryCounterBetweenSlippageAdjustment { get; set; }

        protected int RandomDelayInSecondsBetweenSlippageAdjustment { get; set; }

        protected int OrderRetrialCheckCounter { get; set; }

        protected int OrderRetrialCounter { get; set; }


        protected readonly object LockObject = new object();


        /// <summary>
        /// 
        /// </summary>
        private void AttempOrder()
        {

            if (!AreConditionsOkForOrderProcessing())
            {
                if (GetCurrentDateTime() >= CurrentValidityDateTime)
                {
                    LoggingUtility.WriteTrace(this, "AttempOrder:AreConditionsOkForOrderProcessing are not OK");
                }

                return;
            }

            if (CurrentAtrPrice <= 0)
            {
                LoggingUtility.WriteTrace(this, "ATR is 0");
                // need the ATR to process further
                return;
            }

            if (CurrentAskPrice <= 0)
            {
                LoggingUtility.WriteTrace(this, "Ask Price is 0");
                // need the Ask to process further
                return;
            }

            if (CurrentBidPrice <= 0)
            {
                LoggingUtility.WriteTrace(this, "Bid Price is 0");
                // need the Bid to process further
                return;
            }

            if (CurrentDailyAvgPrice <= 0)
            {
                LoggingUtility.WriteTrace(this, "CurrentDailyAvgPrice is 0");
                // need to process further
                return;
            }

            if (CurrentDailyAvgVolume <= 0)
            {
                LoggingUtility.WriteTrace(this, "CurrentDailyAvgVolume is 0");
                // need to process further
                return;
            }

            if (MaximumOrderSize <= 0)
            {
                //MaximumOrderSize = 1000;
                throw new ArgumentException("Maximum order size must be a positive number", "MaximumOrderSize");
            }


            if (GetCurrentDateTime() < CurrentValidityDateTime)
            {
                return;
            }

            try
            {
                LoggingUtility.WriteTrace(this, "Starting to attempt order");

                lock (LockObject)
                {

                    // 1. If the order is not triggered yet - then check when is the right time to trigger the order
                    if (!IsCurrentOrderActive())
                    {
                        if (GetHasOrderBeenTriggered())
                        {
                            QueueNewOrder();
                        }
                    }
                    else
                    {

                        // 4. Partially filled? then queue up the remaining order
                        // We need to retry by adjusting the prices 
                        if (GetHasOrderBeenReTriggered())
                        {
                            ReQueueOrder();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                LoggingUtility.WriteInfoFormat(this, "Error occured while attempting to place order. {0}", ex.Message);
            }
            finally
            {
                LoggingUtility.WriteTrace(this, "Ending attempt order");
            }

        }

        private void ReQueueOrder()
        {


            if (!AreConditionsOkForOrderProcessing())
            {
                return;
            }

           
            double quantity = 0;
            double price = 0;

            if (!IsCurrentOrderActive())
            {
                throw new InvalidOperationException("Cannot retrigger order when there is no prior order in place.");
            }


            OrderSide ordAction = CurrentTradeLeg.OrderSide;

            if (ordAction == OrderSide.Buy)
            {
                price = GetNewBuyOrderPrice();
            }
            else
            {
                price = GetNewSellOrderPrice();
            }

            quantity = CurrentTradeLeg.GetQuantityForExitingOrder(price);

            price = RoundPrice(price, this.Instrument);
            bool wasAnOrderPlacedOrReplaced = false;


            decimal decPrice = Convert.ToDecimal(price);
            decimal decQuantity = Convert.ToDecimal(quantity);


            bool isDifferent = (Convert.ToDecimal(CurrentOrder.Price) != decPrice) ||
                               (Convert.ToDecimal(CurrentOrder.Qty) != decQuantity);
            if (isDifferent)
            {
                // this is modified order
                CurrentOrder.Price = price;
                CurrentOrder.Qty = quantity;
                CurrentOrder.Replace();
                wasAnOrderPlacedOrReplaced = true;

                OrderRetrialCounter++;

                if (OrderRetrialCheckCounter%RandomRetryCounterBetweenSlippageAdjustment == 0)
                {
                    LoggingUtility.WriteInfoFormat(this, "Order retrial counter: {0}, adjusting spread stepping -1", OrderRetrialCheckCounter);
                    AdjustSpreadStepping(-1);
                }
            }


            if (wasAnOrderPlacedOrReplaced)
            {
                CurrentLastOrderPrice = price;
                OrderQueuedTime = GetCurrentDateTime();

                LoggingUtility.WriteHorizontalBreak(this);
                LoggingUtility.WriteInfoFormat(this, "ORDER QUEUED: {0} {1} shares of {5} @ {2:c}. Bid={3:c}, Ask={4:c}",
                    ordAction, quantity, price, CurrentBidPrice, CurrentAskPrice, Instrument.Symbol);
                LoggingUtility.WriteHorizontalBreak(this);
            }
        }

        private bool IsCurrentOrderActive()
        {
            return null != CurrentOrder && !CurrentOrder.IsDone;
        }


        private void QueueNewOrder()
        {
            if (!AreConditionsOkForOrderProcessing())
            {
                return;
            }

            ResetMaxAllowedSpread();

            if (!IsSpreadTolerable())
            {
                return;
            }

            double quantity = 0;
            double price = 0;


            OrderSide ordAction = CurrentTradeLeg.OrderSide;

            if (ordAction == OrderSide.Buy)
            {
                price = GetNewBuyOrderPrice();
            }
            else
            {
                price = GetNewSellOrderPrice();
            }

            quantity = CurrentTradeLeg.GetQuantityForNewOrder(price);
            if (quantity > MaximumOrderSize)
            {
                LoggingUtility.WriteDebugFormat(this, "Original quantity {0:N2}. Max quantity {1:N2}. Using MAX quantity", quantity, MaximumOrderSize);
                quantity = MaximumOrderSize;
            }
            else
            {
                LoggingUtility.WriteDebugFormat(this, "Original quantity {0:N2}. Max quantity {1:N2}. Using ORIGINAL quantity", quantity, MaximumOrderSize);
            }

            quantity = Convert.ToInt32(Math.Abs(quantity));
            price = RoundPrice(price, this.Instrument);
            bool wasAnOrderPlacedOrReplaced = false;

            if (!IsCurrentOrderActive())
            {
                // this is brand new order
                if (ordAction == OrderSide.Buy)
                {
                    CurrentOrder = BuyLimitOrder(quantity, price, CurrentTradeLeg.GetEntrySignalName(this));
                }
                else
                {
                    CurrentOrder = SellLimitOrder(quantity, price, CurrentTradeLeg.GetEntrySignalName(this));
                }

                CurrentTradeLeg.TrackNewOrder(CurrentOrder);
                CurrentOrder.Account = IbAccountNumber.ToUpper();
                CurrentOrder.Send();
                wasAnOrderPlacedOrReplaced = true;

                LastOrderCompletedAt = null;

                OrderRetrialCounter = 0;
            }


            if (wasAnOrderPlacedOrReplaced)
            {
                CurrentLastOrderPrice = price;
                OrderQueuedTime = GetCurrentDateTime();

                LoggingUtility.WriteHorizontalBreak(this);
                LoggingUtility.WriteInfoFormat(this, "ORDER QUEUED: {0} {1} shares of {5} @ {2:c}. Bid={3:c}, Ask={4:c}",
                    ordAction, quantity, price, CurrentBidPrice, CurrentAskPrice, Instrument.Symbol);
                LoggingUtility.WriteHorizontalBreak(this);
            }
        }


        protected double GetNewSellOrderPrice()
        {
            double newOrderPrice;
          
            // We are selling - check for bearish signs to cancel order
            bool isOrderCancellationTriggeredBasedOnIndicators = ArePriceActionIndicatorsInFavorableMode(OrderSide.Buy);
            if (isOrderCancellationTriggeredBasedOnIndicators)
            {
                LoggingUtility.WriteInfo(this, "Applying getting out of market pricing");
                newOrderPrice = GetGetOutOfMarketSellPrice();
            }
            else
            {
                if (GetMinutesElapsedSinceBeginningOfSession() <= EarlyMorningTradingGradePeriodMinutes)
                {
                    WriteInfrequentDebugMessage("Applying early morning pricing");
                    newOrderPrice = GetEarlyMorningSellPrice();
                }
                else
                {
                    LoggingUtility.WriteTrace(this, "Applying regular pricing");
                    newOrderPrice = GetSellPrice();
                }
            }
            return newOrderPrice;
        }

        private double GetEarlyMorningSellPrice()
        {
            List<double> priceCandidates = new List<double>();

            if (CurrentLastOrderPrice > 0 && IsCurrentOrderActive())
                priceCandidates.Add(CurrentLastOrderPrice);
            if (CurrentDayOperningPrice > 0)
                priceCandidates.Add(CurrentDayOperningPrice);
            if (CurrentAskPrice > 0)
                priceCandidates.Add(CurrentAskPrice);
            priceCandidates.Add(GetSellPrice());

            return priceCandidates.Max();
        }

        protected double GetNewBuyOrderPrice()
        {
            double newOrderPrice;
          

            // We are buying - check for bearish signs to cancel order
            bool isOrderCancellationTriggeredBasedOnIndicators = ArePriceActionIndicatorsInFavorableMode(OrderSide.Sell);
            if (isOrderCancellationTriggeredBasedOnIndicators)
            {
                LoggingUtility.WriteInfo(this, "Applying getting out of market pricing");
                newOrderPrice = GetGetOutOfMarketBuyPrice();
            }
            else
            {
                if (GetMinutesElapsedSinceBeginningOfSession() <= EarlyMorningTradingGradePeriodMinutes)
                {
                    LoggingUtility.WriteDebug(this, "Applying early morning pricing");
                    newOrderPrice = GetEarlyMorningBuyPrice();    
                }
                else
                {
                    LoggingUtility.WriteTrace(this, "Applying regular pricing");
                    newOrderPrice = GetBuyPrice();    
                }
            }

            return newOrderPrice;
        }

        private double GetEarlyMorningBuyPrice()
        {
            List<double> priceCandidates = new List<double>();

            if (CurrentLastOrderPrice > 0 && IsCurrentOrderActive())
                priceCandidates.Add(CurrentLastOrderPrice);
            if (CurrentDayOperningPrice > 0)
                priceCandidates.Add(CurrentDayOperningPrice);
            if (CurrentBidPrice > 0)
                priceCandidates.Add(CurrentBidPrice);
            priceCandidates.Add(GetBuyPrice());

            return priceCandidates.Min();
        }

        protected double GetBuyPrice()
        {
            double ask = GetCurrentAsk();
            double referenecAmount = ask;
            double slippage = GetAllowededSlippageAmount();
            double spread = Math.Abs(GetAllowededSpreadAmount());
            double adjusted = (referenecAmount + slippage) - spread;
            if (CurrentBidPrice > adjusted)
                adjusted = CurrentBidPrice;
            double diff = Math.Abs(adjusted - referenecAmount);

            string logMessage = string.Format("BUY PRICE: Bid={0:c}, Ask={1:c}, Ref={2:c}, Slippage={3:c5}, Spread={4:c5} Adjusted={5:c5}, Diff={6:p}",
                CurrentBidPrice,
                CurrentAskPrice,
                referenecAmount,
                slippage,
                spread,
                adjusted,
                diff/referenecAmount);


            WriteInfrequentDebugMessage(logMessage);

            double[] all = new double[] {CurrentBidPrice, adjusted};
            return all.Min();
        }


        protected double GetSellPrice()
        {
            double bid = GetCurrentBid();
            double referenecAmount = bid;
            double slippage = GetAllowededSlippageAmount();
            double spread = Math.Abs(GetAllowededSpreadAmount());
            double adjusted = (referenecAmount - slippage) + spread;
            if (CurrentAskPrice < adjusted)
                adjusted = CurrentAskPrice;
            double diff = Math.Abs(adjusted - referenecAmount);
            string logMessage = string.Format("SELL PRICE: Bid={0:c},  Ask={1:c}, Ref={2:c}, Slippage={3:c5}, Spread={4:c5} Adjusted={5:c5}, Diff={6:p}",
                CurrentBidPrice,
                CurrentAskPrice,
                referenecAmount,
                slippage,
                spread,
                adjusted,
                diff / referenecAmount);
           
            WriteInfrequentDebugMessage(logMessage);

            double[] all = new double[] {CurrentAskPrice, adjusted};
            return all.Max();
        }

        public double RoundPrice(double input, Instrument instrument)
        {
            double step = GetPriceStep(input, instrument);
            return Math.Round(input/step)*step;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private double GetPriceStep(double input, Instrument instrument)
        {
            double retValue = 0.01;
            if (instrument.Type == InstrumentType.Futures)
            {
                if (System.String.Compare(instrument.Symbol, "ES", System.StringComparison.OrdinalIgnoreCase) == 0)
                {
                    retValue = 0.25;
                }
            }
            else if (instrument.Type == InstrumentType.Stock)
            {
                if (input < 1)
                    retValue = 0.001;
            }

            return retValue;
        }


        private double GetAllowedDurationBetweenRetries()
        {
            double periodAllowedBetweenRetries = MaximumIntervalInMinutesBetweenOrderRetries*
                                                 GetRemainderSessionTimeFraction();

            double[] periods = new double[] {1.0d, periodAllowedBetweenRetries};
            double effectivePeriod = periods.Max();
            return effectivePeriod;
        }

        protected double GetGetOutOfMarketBuyPrice()
        {
            double atrFactor = 1;
            if (GetMinutesElapsedSinceBeginningOfSession() <= EarlyMorningTradingGradePeriodMinutes)
            {
                atrFactor = 0.03;
            }
            
            List<double> candidatePrics = new List<double>();

            candidatePrics.Add(GetCurrentAsk() - (CurrentAtrPrice * atrFactor));
            candidatePrics.Add(GetCurrentBid() - (CurrentAtrPrice * atrFactor));
            candidatePrics.Add(CurrentLastPrice - (CurrentAtrPrice * atrFactor));

            return candidatePrics.Where(a => a > 0).Min();
        }

        protected double GetGetOutOfMarketSellPrice()
        {
            double atrFactor = 1;
            if (GetMinutesElapsedSinceBeginningOfSession() <= EarlyMorningTradingGradePeriodMinutes)
            {
                atrFactor = 0.03;
            }

            List<double> candidatePrics = new List<double>();

            candidatePrics.Add(GetCurrentAsk() + (CurrentAtrPrice * atrFactor));
            candidatePrics.Add(GetCurrentBid() + (CurrentAtrPrice * atrFactor));
            candidatePrics.Add(CurrentLastPrice + (CurrentAtrPrice * atrFactor));

            return candidatePrics.Where(a => a > 0).Max();
        }

        private double GetCurrentAsk()
        {
            return CurrentAskPrice;
        }

        private double GetCurrentBid()
        {
            return CurrentBidPrice;
        }


        private bool GetHasOrderBeenReTriggered()
        {
            OrderRetrialCheckCounter++;

            if (!AreConditionsOkForOrderProcessing())
            {
                return false;
            }


            if (IsCurrentOrderActive() && CurrentOrder.CumQty <= 0 && IsSpreadTolerable())
            {
                return HasOrderBeenReTriggeredForFreshUnfilledOrder();
            }
            else if (IsCurrentOrderActive())
            {
                return HasOrderBeenReTriggeredForPartiallyFilledOrder();
            }

            return false;

        }

        private bool HasOrderBeenReTriggeredForPartiallyFilledOrder()
        {
            OrderSide orderAction = CurrentTradeLeg.OrderSide;
            double minDurationInMinutesBeforeNextRetry = GetAllowedDurationBetweenRetries();
            double minutesPastSinceLastOrderTry = Math.Abs(GetCurrentDateTime().Subtract(OrderQueuedTime).TotalMinutes);
            bool isOrderTriggeredBasedOnTime = minutesPastSinceLastOrderTry >= minDurationInMinutesBeforeNextRetry;
            double timeRemainingForNextTry = minDurationInMinutesBeforeNextRetry - minutesPastSinceLastOrderTry;
            DateTime timeOfNextRetrial = GetCurrentDateTime().AddMinutes(timeRemainingForNextTry);


            double adversePriceMovementAllowed = (CurrentAtrPrice*AdverseMovementInPriceAtrThreshold);
            double adversePriceThresholdAmount = 0;
            bool isOrderTriggeredBasedOnAdversePriceMovement = false;

            if (adversePriceMovementAllowed > 0 &&
                CurrentLastOrderPrice > 0)
            {
                if (orderAction == OrderSide.Buy)
                {
                    adversePriceThresholdAmount = CurrentLastOrderPrice + adversePriceMovementAllowed;
                    isOrderTriggeredBasedOnAdversePriceMovement = CurrentClosePrice >= adversePriceThresholdAmount;
                }
                else
                {
                    adversePriceThresholdAmount = CurrentLastOrderPrice - adversePriceMovementAllowed;
                    isOrderTriggeredBasedOnAdversePriceMovement = CurrentClosePrice <= adversePriceThresholdAmount;
                }
            }


            LogRetrialOrderParameters(isOrderTriggeredBasedOnTime, timeOfNextRetrial, timeRemainingForNextTry,
                adversePriceMovementAllowed, adversePriceThresholdAmount,
                isOrderTriggeredBasedOnAdversePriceMovement);


            return isOrderTriggeredBasedOnAdversePriceMovement || isOrderTriggeredBasedOnTime;
        }

        private bool HasOrderBeenReTriggeredForFreshUnfilledOrder()
        {

            OrderSide orderAction = CurrentTradeLeg.OrderSide;
            // Order is still new - if so - we can check if the big/ask or last price changed
            // and update our order
            double totalSecondsPast = GetCurrentDateTime().Subtract(OrderQueuedTime).TotalSeconds;
            double oldPrice = CurrentLastOrderPrice;

            double newOrderPrice = 0;
            if (orderAction == OrderSide.Buy)
            {
                newOrderPrice = GetNewBuyOrderPrice();
            }
            else
            {
                newOrderPrice = GetNewSellOrderPrice();
            }

            newOrderPrice = RoundPrice(newOrderPrice, this.Instrument);

            if (oldPrice > 0 && newOrderPrice > 0 && totalSecondsPast >= 1)
            {

                if (oldPrice != newOrderPrice)
                {
                    LoggingUtility.WriteInfoFormat(this,
                        "Last order price={0:c}, New order price={1:c}. ORDER HAS BEEN RETRIGGERED",
                        CurrentLastOrderPrice, newOrderPrice);

                    return true;
                }
            }
            return false;
        }


        private void LogRetrialOrderParameters(bool isOrderTriggeredBasedOnTime,
            DateTime timeOfNextRetrial,
            double timeRemainingForNextTry,
            double adversePriceMovementAllowed,
            double adversePriceThresholdAmount,
            bool isOrderTriggeredBasedOnAdversePriceMovement)
        {
            if (OrderRetrialCheckCounter%10 == 0)
            {
                string timePart = string.Format("TIME={0} (Next Try={1} ({2:F4}m)",
                    isOrderTriggeredBasedOnTime,
                    timeOfNextRetrial,
                    timeRemainingForNextTry);

                double diffBetweenOrderAndClosePrice = CurrentLastOrderPrice - CurrentClosePrice;

                string lastOrderRelMessage =
                    string.Format(
                        "Order Price={0:c}, Tolerable Diff={1:c} ({2:p}) of Order. Trigger Amt={3:c}. Last={4:c} Diff={5:c} ({6:p})",
                        CurrentLastOrderPrice,
                        adversePriceMovementAllowed,
                        adversePriceMovementAllowed/CurrentLastOrderPrice,
                        adversePriceThresholdAmount,
                        CurrentClosePrice,
                        diffBetweenOrderAndClosePrice,
                        diffBetweenOrderAndClosePrice/CurrentLastOrderPrice);

                string priceParth = string.Format(
                    "PRICE={0} (Tol Diff={1:c} ({2:F4} x ATR {3:c})). {4}",
                    isOrderTriggeredBasedOnAdversePriceMovement,
                    adversePriceMovementAllowed,
                    AdverseMovementInPriceAtrThreshold,
                    CurrentAtrPrice,
                    lastOrderRelMessage);

                LoggingUtility.WriteInfoFormat(this, "Re-Order TIME triggers. {0}",
                    timePart);

                LoggingUtility.WriteInfoFormat(this, "Re-Order PRICE triggers. {0}",
                    priceParth);

                LogCurrentIndicatorValues();
            }
        }


        private void LogCurrentIndicatorValues()
        {
            string emaPart = string.Format("Ema({0}x{1})=({2:F4}x{3:F4})",
                FastMaPeriod,
                SlowMaPeriod,
                FastEmaIndicator.Last,
                SlowEmaIndicator.Last);

            string stochPart = string.Format("Stochastics({0},{1},{2})=(K={3:F4},D={4:F4})",
                StochasticsKPeriod,
                StochasticsDPeriod,
                StochasticsSmoothPeriod,
                KSlowIndicator.Last,
                DSlowIndicator.Last);

            LoggingUtility.WriteInfoFormat(this, "CURRENT INDICATORS=({0}, {1})",
                emaPart,
                stochPart);
        }


        private bool GetHasOrderBeenTriggered()
        {

            if (!AreConditionsOkForOrderProcessing())
            {
                return false;
            }



            bool isOrderTriggeredBasedOnTime = HasTimeTriggerFired();


            if (LastOrderCompletedAt.HasValue && !isOrderTriggeredBasedOnTime)
            {
                double actualDelay = Math.Abs(GetCurrentDateTime().Subtract(LastOrderCompletedAt.Value).TotalSeconds);
                if (actualDelay < RandomDelayBetweenOrdersInSeconds)
                {
                    return false;
                }
            }

            OrderSide orderAction = CurrentTradeLeg.OrderSide;
            bool isOrderTriggeredBasedOnIndicators = ArePriceActionIndicatorsInFavorableMode(orderAction);

            string logMessage = string.Format("Order triggers. TIME={0}, INDICATORS={1}",
                isOrderTriggeredBasedOnTime,
                isOrderTriggeredBasedOnIndicators);


            bool returnValue = isOrderTriggeredBasedOnIndicators || isOrderTriggeredBasedOnTime;

            if (returnValue)
            {
                LoggingUtility.WriteInfo(this, logMessage);
            }
            else
            {
                WriteInfrequentDebugMessage(logMessage);
            }

            return returnValue;

        }

        protected bool HasTimeTriggerFired()
        {
            // If there ar elast 30-40 minutes left in the session - then fire the 
            // time based trigger
            return GetRemainderSessionTimeFraction() <= 0.08d;
        }

    }
}
