using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using OpenQuant.API;
using OpenQuant.API.Indicators;
using Spider.OpenQuant3_2.Common;
using Spider.OpenQuant3_2.Extensions;
using Spider.OpenQuant3_2.QuoteServer;
using Spider.OpenQuant3_2.Util;

namespace Spider.OpenQuant3_2.Base
{

    public abstract partial class BaseStrategy : Strategy
    {
        #region Properties

        #region Private/Protected Properties

        public DateTime CurrentValidityDateTime { get; set; }

        public DateTime CurrentStartOfSessionTime = DateTime.MaxValue;

        public DateTime CurrentEndOfSessionTime = DateTime.MaxValue;

        public DateTime CurrentLastSessionDate = DateTime.MaxValue;

        public double CurrentLastOrderPrice = 0;

        public int CurrentExecutionTimePeriodInSeconds = 120;

        protected const double MinSpreadInCents = 0.01;

        protected bool firstCrossOverOfIndicatorHappened = false;

        protected Bar CurrentBarRef { get; set; }

        protected Quote CurrentQuoteRef { get; set; }

        protected double CurrentAtrPrice { get; set; }

        protected double CurrentClosePrice { get; set; }

        protected double CurrentAskPrice { get; set; }

        protected double CurrentBidPrice { get; set; }

        protected double CurrentDailyAvgPrice { get; set; }

        protected double CurrentDailyAvgVolume { get; set; }


        protected double LastDayClosingPrice { get; set; }

        protected double CurrentDayOperningPrice { get; set; }

        protected bool IsStrategyOrderPartiallyFilled { get; set; }

        protected double OriginalMaxSpreadFractionToAllocateForPricing { get; set; }

        protected bool IsStrategyOrderFilled { get; set; }

        protected DateTime OrderQueuedTime { get; set; }

        protected DateTime StrategyBeginTime { get; set; }

        protected double TotalNumberOfSlicesAvailableInSession { get; set; }

        protected bool IsStochCrossUp { get; set; }

        protected bool IsStochCrossDown { get; set; }

        protected bool IsEmaCrossUp { get; set; }

        protected bool IsEmaCrossDown { get; set; }

        protected Order CurrentOrder { get; set; }

        protected int OrderRetrialCheckCounter { get; set; }

        protected static ConcurrentDictionary<string, bool> ReversalOpeningOrderStateDictionary = new ConcurrentDictionary<string, bool>(StringComparer.InvariantCultureIgnoreCase);
        
        
        protected readonly object LockObject = new object();

        #region Indicators

        protected SMA DailyVolumeSmaIndicator { get; set; }

        protected SMA DailyPriceSmaIndicator { get; set; }


        protected EMA SlowEmaIndicator { get; set; }

        protected EMA FastEmaIndicator { get; set; }

        protected K_Slow KSlowIndicator { get; set; }

        protected D_Slow DSlowIndicator { get; set; }

        protected ATR DailyAtrIndicator { get; set; }

        protected BarSeries CurrentDailyBarSeries { get; set; }

        protected BarSeries CurrentExecutionBarSeries { get; set; }


        #endregion

        #endregion


        #endregion


        public override void OnStrategyStart()
        {
            OriginalMaxSpreadFractionToAllocateForPricing = MaxSpreadFractionToAllocateForPricing;
            OrderRetrialCheckCounter = 0;
            StrategyBeginTime = DateTime.Now;

            // First setup the main objects
            SetAndValidateValues();

            SetupIndicators();

            SetupData();

            SetupQuoteClient();

            ReversalOpeningOrderStateDictionary.AddOrUpdate(this.Instrument.Symbol, IsStrategyOrderFilled, (s, b) => IsStrategyOrderFilled);

            LoggingUtility.WriteHorizontalBreak(this);
            LoggingUtility.WriteInfoFormat(this, "*** INITIALIZATION DONE ***");
            LoggingUtility.WriteHorizontalBreak(this);
            Console.WriteLine();
            Console.WriteLine();
        }

        public override void OnStrategyStop()
        {
            TearDownQuoteClient();

            base.OnStrategyStop();
        }

        public override void OnBar(Bar bar)
        {

            // If everything is filled - then exit:
            if (IsStrategyOrderFilled)
                return;


            if (!bar.IsWithinRegularTradingHours(Instrument.Type))
            {
                return;
            }
            else
            {
                SaveData(bar);
            }

            try
            {
                CurrentBarRef = bar;

                if (bar.Size == PeriodConstants.PERIOD_DAILY)
                {
                    if (CurrentDailyBarSeries.Count <= AtrPeriod)
                    {
                        return;
                    }

                    if (CurrentDailyBarSeries.Count > 20)
                    {
                        CurrentDailyAvgPrice = DailyPriceSmaIndicator.Last;
                        CurrentDailyAvgVolume = DailyVolumeSmaIndicator.Last;
                    }
                }


                if (bar.Size == CurrentExecutionTimePeriodInSeconds)
                {
                    int[] periods = new[]
                    {
                        SlowMaPeriod,
                        FastMaPeriod,
                        StochasticsDPeriod,
                        StochasticsKPeriod,
                        StochasticsSmoothPeriod
                    };

                    int maxPeriod = periods.Max();

                    if (CurrentExecutionBarSeries.Count <= maxPeriod)
                    {
                        return;
                    }
                }


                CurrentClosePrice = bar.Close;

                if (bar.Size == PeriodConstants.PERIOD_DAILY)
                {
                    CurrentAtrPrice = DailyAtrIndicator.Last;
                    if (CurrentClosePrice > 0 && IsBarCloseEnoughForLogging(bar))
                    {
                        LoggingUtility.WriteInfoFormat(this,
                            "Setting ATR to {0:c} which is {1:p} of the last close price {2:c}",
                            CurrentAtrPrice,
                            CurrentAtrPrice/CurrentClosePrice,
                            CurrentClosePrice);
                    }
                }

                if (bar.Size == PeriodConstants.PERIOD_DAILY)
                {
                    // We do not need to worry about any other type of bar
                    return;
                }

                if (bar.Size == CurrentExecutionTimePeriodInSeconds)
                {
                    // Ensure that intraday indicators are evaluated
                    GetIsStochInBullishMode(bar);
                    GetIsStochInBearishMode(bar);
                    GetIsEmaInBearishMode(bar);
                    GetIsEmaInBullishMode(bar);
                }

                if (CurrentAtrPrice <= 0)
                {
                    // need the ATR to process further
                    return;
                }

                if (CurrentAskPrice <= 0)
                {
                    // need the Ask to process further
                    return;
                }

                if (CurrentBidPrice <= 0)
                {
                    // need the Bid to process further
                    return;
                }

                if (GetCurrentDateTime() < CurrentValidityDateTime)
                {
                    return;
                }


                AttempOrderIfApplicable();
            }
            finally
            {
                CurrentBarRef = null;
            }
        }

        private void AttempOrderIfApplicable()
        {
            if (!AreConditionsOkForOrderProcessing())
            {
                return;
            }


            try
            {

                lock (LockObject)
                {
                    // 1. If the order is not triggered yet - then check when is the right time to trigger the order
                    if (null == CurrentOrder)
                    {
                        if (GetHasOrderBeenTriggered())
                        {
                            QueueOrder();
                        }
                    }
                    else
                    {
                        // 4. Partially filled? then queue up the remaining order
                        // We need to retry by adjusting the prices 
                        if (GetHasOrderBeenReTriggered())
                        {
                            AdjustSpreadStepping(-1);

                            QueueOrder();
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

            }
        }


        public override void OnBarOpen(Bar bar)
        {
            if (!AreConditionsOkForOrderProcessing())
            {
                return;
            }

            try
            {
                if (!bar.IsWithinRegularTradingHours(Instrument.Type))
                {
                    return;
                }


                if (bar.Size == PeriodConstants.PERIOD_DAILY)
                {
                    // Since we will like to capture any gaps in the current day
                    // open and corresponding effects of ATR calculations
                    OnBar(bar);
                }
                else if (bar.BeginTime.Date >= CurrentValidityDateTime.Date)
                {
                    //EvaluateIndicatorsAtBeginningOfSession();

                    OnBar(bar);

                }

                base.OnBarOpen(bar);
            }
            finally
            {
                
            }
        }



        public override void OnOrderStatusChanged(Order order)
        {
            if (order.Status == OrderStatus.Filled)
            {
                IsStrategyOrderFilled = true;
            }
            else if (order.Status == OrderStatus.PartiallyFilled)
            {
                HandlePartiallyFilledQuantity(order);

                IsStrategyOrderPartiallyFilled = true;
            }
            else if (order.Status == OrderStatus.Rejected || order.Status == OrderStatus.Cancelled)
            {
                LoggingUtility.WriteInfo(this, "Order in rejected/cancelled state. " + order.Text);
                
                ResetSpreadStepping();

                CurrentOrder = null;
            }
            else if (order.IsDone)
            {
                LoggingUtility.WriteInfo(this, "Order in rejected state. " + order.Text);
            }

            string message = string.Format("ORDER UPDATE: {0}, Status={1}", order.Text, order.Status);

            LoggingUtility.WriteHorizontalBreak(this);
            LoggingUtility.WriteInfo(this, message);
            LoggingUtility.WriteHorizontalBreak(this);

            RunPostOrderStatusChangedImpl();
        }

        protected virtual void RunPostOrderStatusChangedImpl()
        {

        }


        public override void OnQuote(Quote quote)
        {
            try
            {
                CurrentQuoteRef = quote;

                SaveData(Instrument, quote);

                CurrentAskPrice = quote.Ask;
                CurrentBidPrice = quote.Bid;

                //EvaluateIndicatorsAtBeginningOfSession();

                AttempOrderIfApplicable();
                //LoggingUtility.WriteInfoFormat(this, "Current Bid={0:c}, Ask={1:c}", CurrentBidPrice, CurrentAskPrice);
            }
            finally
            {
                CurrentQuoteRef = null;
            }

        }


        public DateTime GetCurrentDateTime()
        {
            if (CurrentBarRef != null && CurrentBarRef.BeginTime.Year >= 2010)
            {
                return CurrentBarRef.BeginTime;
            }

            if (CurrentQuoteRef != null && CurrentQuoteRef.DateTime.Year >= 2010)
            {
                return CurrentQuoteRef.DateTime;
            }

            if (Bar != null && Bar.BeginTime.Year >= 2010)
            {
                return Bar.BeginTime;
            }

            return Clock.Now;
        }

        private void QueueOrder()
        {
            if (!AreConditionsOkForOrderProcessing())
            {
                return;
            }

            if (!IsSpreadTolerable())
            {
                return;
            }

            int quantity = 0;
            double price = 0;

          
            OrderSide ordAction = GetOrderSide();

            if (ordAction == OrderSide.Buy)
            {
                price = GetNewBuyOrderPrice();
                quantity = GetBuyQuantity();
            }
            else
            {
                price = GetNewSellOrderPrice();
                quantity = GetSellQuantity();
            }

            price = RoundPrice(price, this.Instrument);
            bool wasAnOrderPlacedOrReplaced = false;
            if (CurrentOrder == null)
            {
                // this is brand new order
                if (ordAction == OrderSide.Buy)
                {
                    CurrentOrder = BuyLimitOrder(quantity, price, GetEntrySignalName());
                }
                else
                {
                    CurrentOrder = SellLimitOrder(quantity, price, GetEntrySignalName());
                }
                CurrentOrder.Account = IbAccountNumber.ToUpper();
                CurrentOrder.Send();
                wasAnOrderPlacedOrReplaced = true;
            }
            else
            {
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
                }
            }

            if (wasAnOrderPlacedOrReplaced)
            {
                CurrentLastOrderPrice = price;
                OrderQueuedTime = GetCurrentDateTime();

                LoggingUtility.WriteHorizontalBreak(this);
                LoggingUtility.WriteInfoFormat(this, "ORDER QUEUED: {0} {1} shares of {5} @ {2:c}. Bid={3:c}, Ask={4:c}", ordAction, quantity, price, CurrentBidPrice, CurrentAskPrice, Instrument.Symbol);
                LoggingUtility.WriteHorizontalBreak(this);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public double RoundPrice(double input, Instrument instrument)
        {
            double step = GetPriceStep(input, instrument);
            return Math.Round(input / step) * step;
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
                if (string.Compare(instrument.Symbol, "ES", true) == 0)
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

        private bool IsSpreadTolerable()
        {

            if (MinAllowedSpreadToAtrFraction > MaxAllowedSpreadToAtrFraction)
                throw new ArithmeticException(
                    string.Format("Min Allowed spread {0} cannot be more than Max Allowed spread {1}",
                        MinAllowedSpreadToAtrFraction, MaxAllowedSpreadToAtrFraction));


            if (CurrentAtrPrice > 0 &&
                CurrentAskPrice > 0 &&
                CurrentBidPrice > 0)
            {
                double currentSpread = Math.Abs(CurrentBidPrice - CurrentAskPrice);
                if (currentSpread <= MinSpreadInCents)
                {
                    LoggingUtility.WriteInfoFormat(this,
                        "Spread is {0:c} which is minimum possible. Spread eval=TRUE.",
                        currentSpread);
                    return true;
                }

                double remainingSessionPart = (1 - GetRemainderSessionTimeFraction());
                double differenceInAllowedSpread = MaxAllowedSpreadToAtrFraction - MinAllowedSpreadToAtrFraction;
                double maxtolerableSpreadInAtr = MinAllowedSpreadToAtrFraction + (differenceInAllowedSpread * remainingSessionPart);

                double currentSpreadInAtr = currentSpread / CurrentAtrPrice;

                bool isCurrentSpreadOk = currentSpreadInAtr <= maxtolerableSpreadInAtr;

                string currentBidAskMessage = string.Format("Bid={0:c},Ask={1:c},Atr={2:c}",
                    CurrentBidPrice, CurrentAskPrice, CurrentAtrPrice);

                string currentSpreadMessage = string.Format("Spread is {0:c} ({1:p} of ATR {2:c})",
                    currentSpread, currentSpreadInAtr, CurrentAtrPrice);
                string allowedSpreadMessage = string.Format("Allowed is {0:c} ({1:p} of ATR {2:c})",
                    maxtolerableSpreadInAtr * CurrentAtrPrice, maxtolerableSpreadInAtr, CurrentAtrPrice);

                LoggingUtility.WriteInfoFormat(this,
                    "{0}. {1}. {2}. Session Passed: {3:F4}. Spread eval={4}.",
                    currentBidAskMessage,
                    currentSpreadMessage,
                    allowedSpreadMessage,
                    remainingSessionPart,
                    isCurrentSpreadOk.ToString().ToUpper());

                return isCurrentSpreadOk;

            }

            return false;
        }


        private bool GetWhetherItIsOkToQueueOrderBasedOnLastEntryTimestamp()
        {
            double minutesPastSinceLastOrderTry = GetCurrentDateTime().Subtract(OrderQueuedTime).TotalMinutes;
            var effectivePeriod = GetAllowedDurationBetweenRetries();
            return (minutesPastSinceLastOrderTry > effectivePeriod);
        }

        private double GetAllowedDurationBetweenRetries()
        {
            double periodAllowedBetweenRetries = MaximumIntervalInMinutesBetweenOrderRetries*
                                                 GetRemainderSessionTimeFraction();

            double[] periods = new double[] {1.0d, periodAllowedBetweenRetries};
            double effectivePeriod = periods.Max();
            return effectivePeriod;
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
            LoggingUtility.WriteInfoFormat(this, "Bid={0:c}, Ask={1:c}, Ref={2:c}, Adjusted={3:c5}, Diff={4:p}",
                CurrentBidPrice,
                CurrentAskPrice,
                referenecAmount,
                adjusted,
                diff / referenecAmount);
            double[] all = new double[] {  CurrentAskPrice, adjusted };
            return all.Max();
        }

        protected double GetGetOutOfMarketBuyPrice()
        {
            return GetCurrentAsk() - CurrentAtrPrice;
        }

        protected double GetGetOutOfMarketSellPrice()
        {
            return GetCurrentBid() + CurrentAtrPrice;
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
            LoggingUtility.WriteInfoFormat(this, "Bid={0:c}, Ask={1:c}, Ref={2:c}, Adjusted={3:c5}, Diff={4:p}",
                CurrentBidPrice,
                CurrentAskPrice,
                referenecAmount,
                adjusted,
                diff / referenecAmount);
            double[] all = new double[] {  CurrentAskPrice, adjusted };
            return all.Min();
        }

        private double GetCurrentAsk()
        {
            return CurrentAskPrice;
        }

        private double GetCurrentBid()
        {
            return CurrentBidPrice;
        }

        protected abstract int GetSellQuantity();


        protected abstract int GetBuyQuantity();


        protected string GetEntrySignalName()
        {
            return string.Format("{0}.{1}.{2}.{3}", ProjectName,
               GetCurrentDateTime().ToString("ddMMM.HHmmss"),
               GetOrderSide(),
               this.Instrument.Symbol);
        }

        public abstract OrderSide GetOrderSide();

        protected abstract void HandlePartiallyFilledQuantity(Order order);

        private double GetAllowededSlippageAmount()
        {
            if (MinAllowedSlippage > MaxAllowedSlippage)
                throw new ArithmeticException(
                    string.Format("Min Allowed Slippage {0} cannot be more than Max Allowed Slippage {1}",
                        MinAllowedSlippage, MaxAllowedSlippage));

            if (CurrentAtrPrice <= 0)
                throw new ArgumentOutOfRangeException("CurrentAtrPrice", "Atr price must be a positive number");
            double remainingSessionPart = (1 - GetRemainderSessionTimeFraction());
            double totalSlippageGapAllowedToBeConsumedInTheSession =
                GetTotalSlippageGapAllowedToBeConsumedInTheSession();
            double slippageLeftToBeConsumed = totalSlippageGapAllowedToBeConsumedInTheSession*
                                              GetRemainderSessionTimeFraction();
            double allowedSlippageFraction = MaxAllowedSlippage - slippageLeftToBeConsumed;
            double slippageAllowedAmount = allowedSlippageFraction*CurrentAtrPrice;

            string allowedSlippageMessage =
                string.Format("Slippage Fraction: Max={0:F4}, Min={1:F4}, Diff={2:F4}, Remaining={3:F4}",
                    MaxAllowedSlippage,
                    MinAllowedSlippage,
                    totalSlippageGapAllowedToBeConsumedInTheSession,
                    allowedSlippageFraction);

            LoggingUtility.WriteInfoFormat(this,
                "{0}. Session Passed: {1:F4}. So slippage amount to recover {2:c5}.",
                allowedSlippageMessage,
                remainingSessionPart,
                slippageAllowedAmount);



            return slippageAllowedAmount;
        }


        private double GetAllowededSpreadAmount()
        {
            double allowedSpreadAmount = 0;

            if (CurrentAtrPrice > 0 &&
                CurrentAskPrice > 0 &&
                CurrentBidPrice > 0)
            {
                double currentSpread = Math.Abs(CurrentBidPrice - CurrentAskPrice);
                if (currentSpread > MinSpreadInCents)
                {
                    double remainingSessionPart = (1 - GetRemainderSessionTimeFraction());
                    double differenceInAllowedSpread = MaxSpreadFractionToAllocateForPricing -
                                                       MinSpreadFractionToAllocateForPricing;
                    double maxtolerableSpreadFraction = MaxSpreadFractionToAllocateForPricing -
                                                        (differenceInAllowedSpread * remainingSessionPart);
                    allowedSpreadAmount = maxtolerableSpreadFraction * currentSpread;

               
                    string allowedSpreadMessage =
                        string.Format("Spread Fraction: Max={0:F4}, Min={1:F4}, Diff={2:F4}, Remaining={3:F4}",
                            MaxSpreadFractionToAllocateForPricing,
                            MinSpreadFractionToAllocateForPricing,
                            differenceInAllowedSpread,
                            maxtolerableSpreadFraction);

                    LoggingUtility.WriteInfoFormat(this,
                        "{0}. Session Passed: {1:F4}. So Spread amount to recover {2:c5}.",
                        allowedSpreadMessage,
                        remainingSessionPart,
                        allowedSpreadAmount);
                }
            }

            return allowedSpreadAmount;
        }

        private double GetTotalSlippageGapAllowedToBeConsumedInTheSession()
        {
            return MaxAllowedSlippage - MinAllowedSlippage;
        }

        private bool IsBarCloseEnoughForLogging(Bar bar)
        {
            if (null != CurrentOrder)
                return false;

            long barSize = bar.Size;
            if (barSize == PeriodConstants.PERIOD_DAILY)
            {
                return (CurrentValidityDateTime.Subtract(bar.BeginTime.Date).TotalDays <= 7);
            }

            if (bar.BeginTime.Date == CurrentValidityDateTime.Date)
                return true;

            return (bar.BeginTime >= CurrentLastSessionDate.Date.AddHours(12));
        }



        private bool GetHasOrderBeenReTriggered()
        {
            OrderRetrialCheckCounter++;

            if (!AreConditionsOkForOrderProcessing())
            {
                return false;
            }

            if (null == CurrentOrder)
            {
                throw new InvalidOperationException("Cannot retrigger order when there is no prior order in place.");
            }

            double minDurationInMinutesBeforeNextRetry = GetAllowedDurationBetweenRetries();
            double minutesPastSinceLastOrderTry = GetCurrentDateTime().Subtract(OrderQueuedTime).TotalMinutes;
            bool isOrderTriggeredBasedOnTime = minutesPastSinceLastOrderTry >= minDurationInMinutesBeforeNextRetry;
            double timeRemainingForNextTry = minDurationInMinutesBeforeNextRetry - minutesPastSinceLastOrderTry;
            DateTime timeOfNextRetrial = GetCurrentDateTime().AddMinutes(timeRemainingForNextTry);



            OrderSide orderAction = GetOrderSide();

            double adversePriceMovementAllowed = (CurrentAtrPrice*AdverseMovementInPriceAtrThreshold);
            double adversePriceThresholdAmount = 0;
            bool isOrderTriggeredBasedOnAdversePriceMovement = false;
            double newOrderPrice = 0;

            if (CurrentAtrPrice > 0 &&
                adversePriceMovementAllowed > 0 &&
                CurrentLastOrderPrice > 0 &&
                CurrentClosePrice > 0)
            {
                if (orderAction == OrderSide.Buy)
                {
                    adversePriceThresholdAmount = CurrentLastOrderPrice + adversePriceMovementAllowed;
                    isOrderTriggeredBasedOnAdversePriceMovement = CurrentClosePrice >= adversePriceThresholdAmount;

                    newOrderPrice = GetNewBuyOrderPrice();
                }
                else
                {
                    adversePriceThresholdAmount = CurrentLastOrderPrice - adversePriceMovementAllowed;
                    isOrderTriggeredBasedOnAdversePriceMovement = CurrentClosePrice <= adversePriceThresholdAmount;

                    newOrderPrice = GetNewSellOrderPrice();
                }
            }


            LogRetrialOrderParameters(isOrderTriggeredBasedOnTime, timeOfNextRetrial, timeRemainingForNextTry,
                adversePriceMovementAllowed, adversePriceThresholdAmount,
                isOrderTriggeredBasedOnAdversePriceMovement);


            if (CurrentOrder.CumQty <= 0 && IsSpreadTolerable())
            {
                double totalSecondsPast = GetCurrentDateTime().Subtract(OrderQueuedTime).TotalSeconds;
                decimal oldPrice = Convert.ToDecimal(CurrentLastOrderPrice);
                decimal newPrice = Convert.ToDecimal(newOrderPrice);
                if (oldPrice > 0 && newPrice > 0 && totalSecondsPast >= 1)
                {
                    if (oldPrice != newPrice)
                    {
                        LoggingUtility.WriteInfoFormat(this,
                            "Last order price={0:c}, New order price={1:c}. ORDER HAS BEEN RETRIGGERED",
                            CurrentLastOrderPrice, newOrderPrice);

                        return true;
                    }
                }
            }

            return isOrderTriggeredBasedOnAdversePriceMovement || isOrderTriggeredBasedOnTime;
        }

        protected double GetNewSellOrderPrice()
        {
            double newOrderPrice;
            bool isEmaFired = IsEmaCrossUp;
            bool isStochFired = GetIsNormalizedStochInBullishMode();

            // We are selling - check for bearish signs to cancel order
            bool isOrderCancellationTriggeredBasedOnIndicators = isEmaFired && isStochFired;
            if (isOrderCancellationTriggeredBasedOnIndicators && null != CurrentOrder && CurrentOrder.CumQty <= 0)
            {
                newOrderPrice = GetGetOutOfMarketSellPrice();
            }
            else
            {
                newOrderPrice = GetSellPrice();
            }
            return newOrderPrice;
        }

        protected double GetNewBuyOrderPrice()
        {
            double newOrderPrice;
            bool isEmaFired = IsEmaCrossDown;
            bool isStochFired = GetIsNormalizedStochInBearishMode();

            // We are buying - check for bearish signs to cancel order
            bool isOrderCancellationTriggeredBasedOnIndicators = isEmaFired && isStochFired;
            if (isOrderCancellationTriggeredBasedOnIndicators && null != CurrentOrder && CurrentOrder.CumQty <= 0)
            {
                newOrderPrice = GetGetOutOfMarketBuyPrice();
            }
            else
            {
                newOrderPrice = GetBuyPrice();
            }

            return newOrderPrice;
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

        private void CancelCurrentOrder()
        {
            lock (LockObject)
            {
                ResetSpreadStepping();

                LogCurrentIndicatorValues();

                LoggingUtility.WriteInfo(this, "CANCELLING ORDER");

                // Need to cancel the order    
                CurrentOrder.Cancel();

                Thread.Sleep(2000);

            }
        }

        private void ResetSpreadStepping()
        {
            MaxSpreadFractionToAllocateForPricing = OriginalMaxSpreadFractionToAllocateForPricing;
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
            if (!IsParentOrderFilled())
            {
                return false;
            }

            if (!AreConditionsOkForOrderProcessing())
            {
                return false;
            }

            bool isOrderTriggeredBasedOnIndicators = false;
            bool isOrderTriggeredBasedOnTime = HasTimeTriggerFired();
            bool isEmaFired = false;
            bool isStochFired = false;

            OrderSide orderAction = GetOrderSide();

            if (orderAction == OrderSide.Buy)
            {
                isEmaFired = IsEmaCrossUp;
                isStochFired = GetIsNormalizedStochInBullishMode();

                // We are buying - check for bullish signs
                isOrderTriggeredBasedOnIndicators = (isEmaFired && isStochFired) || IsStochInObMode();
            }
            else
            {
                isEmaFired = IsEmaCrossDown;
                isStochFired = GetIsNormalizedStochInBearishMode();

                // We are selling - check for bullish signs
                isOrderTriggeredBasedOnIndicators = (isEmaFired && isStochFired) || IsStochInOsMode();
            }

            string emaPart = string.Format("Ema({0}x{1})=({2:F4}x{3:F4})={4}", 
                FastMaPeriod, 
                SlowMaPeriod,
                FastEmaIndicator.Last,
                SlowEmaIndicator.Last,
                isEmaFired.ToString().ToUpper());

            string stochPart = string.Format("Stochastics({0},{1},{2})=(K={3:F4},D={4:F4})={5}",
                StochasticsKPeriod,
                StochasticsDPeriod,
                StochasticsSmoothPeriod,
                KSlowIndicator.Last,
                DSlowIndicator.Last,
                isStochFired.ToString().ToUpper());

            LoggingUtility.WriteInfoFormat(this, "Order triggers. TIME={0}, INDICATORS={1} ({2}, {3})",
                isOrderTriggeredBasedOnTime,
                isOrderTriggeredBasedOnIndicators,
                emaPart,
                stochPart);


            return isOrderTriggeredBasedOnIndicators || isOrderTriggeredBasedOnTime;

        }

        protected virtual bool IsParentOrderFilled()
        {
            return true;
        }

        protected bool HasTimeTriggerFired()
        {
            // If there ar elast 30-40 minutes left in the session - then fire the 
            // time based trigger
            return GetRemainderSessionTimeFraction() <= 0.08d;
        }


        protected bool AreConditionsOkForOrderProcessing()
        {
            if (GetCurrentDateTime() < CurrentValidityDateTime)
            {
                return false;
            }

            if (IsOrderInCompletedState())
            {
                return false;
            }

            if (!GetCurrentDateTime().IsWithinRegularTradingHours(Instrument.Type))
            {
                return false;
            }

            return true;
        }

        protected bool IsOrderInCompletedState()
        {
            // If everything is filled - then exit:
            if (IsStrategyOrderFilled)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// This is used for several calculations where certain thresholds
        /// need to be loosened up as time passes in the session
        /// </summary>
        /// <returns></returns>
        protected double GetRemainderSessionTimeFraction()
        {

            // Assuming we want to execute at least 30 minutes before the close 
            double totalNumberOfMinutes = Math.Abs(CurrentEndOfSessionTime.Subtract(CurrentStartOfSessionTime).TotalMinutes) - 30;
            TotalNumberOfSlicesAvailableInSession = totalNumberOfMinutes / TimeSliceIntervalInMinutes;

            DateTime sessionDate = GetCurrentDateTime().Date;
            DateTime startOfTodaySession = sessionDate.AddSeconds(PstSessionTimeConstants.StockExchangeStartTimeSeconds);

            double numberOfMinutesElapsedSinceBeginningOfSession = Math.Abs(GetCurrentDateTime().Subtract(startOfTodaySession).TotalMinutes);

            double remainder = totalNumberOfMinutes - numberOfMinutesElapsedSinceBeginningOfSession;
            double fractionRemainder = remainder / totalNumberOfMinutes;

            /*
            LoggingUtility.WriteInfoFormat(this, "{0:F2} minutes passed in the session with {1:F2} minutes ({2:p}) remaining.",
                numberOfMinutesElapsedSinceBeginningOfSession, remainder, fractionRemainder);
            */
            return fractionRemainder;
        }


        private void AdjustSpreadStepping(int fractionMultiplier)
        {
            if (GetCurrentDateTime() < CurrentValidityDateTime)
                return;
            if (null == CurrentOrder)
                return;

            double temp = MaxSpreadFractionToAllocateForPricing +
                          (fractionMultiplier * SpreadFractionSteppingForAllocationOfPricing);
            if (temp > MinSpreadFractionToAllocateForPricing)
            {
                MaxSpreadFractionToAllocateForPricing = temp;
            }
        }


        private void EvaluateIndicatorsAtBeginningOfSession()
        {
            if (firstCrossOverOfIndicatorHappened)
                return;

            DateTime currentDateTime = GetCurrentDateTime();
            DateTime cutOffTime = CurrentValidityDateTime.AddSeconds(15*CurrentExecutionTimePeriodInSeconds);
            if (currentDateTime >= CurrentValidityDateTime && 
                currentDateTime < cutOffTime)
            {
                double slowEmaValue = SlowEmaIndicator.Last;
                double fastEmaValue = FastEmaIndicator.Last;

                double currentKValue = KSlowIndicator.Last;
                double currentDValue = DSlowIndicator.Last;

                if (fastEmaValue > slowEmaValue)
                {
                    IsEmaCrossUp = true;
                    IsEmaCrossDown = false;
                }
                else
                {
                    IsEmaCrossUp = false;
                    IsEmaCrossDown = true;
                }

                if (currentKValue > currentDValue)
                {
                    IsStochCrossUp = true;
                    IsStochCrossDown = false;
                }
                else
                {
                    IsStochCrossDown = true;
                    IsStochCrossUp = false;
                }
            }
        }

        private bool GetIsEmaInBullishMode(Bar bar)
        {
            if (!IsEmaCrossUp && FastEmaIndicator.CrossesAbove(SlowEmaIndicator, bar))
            {
                if (GetCurrentDateTime() > CurrentValidityDateTime)
                {
                    firstCrossOverOfIndicatorHappened = true;
                }

                if (IsBarCloseEnoughForLogging(bar))
                {
                    double slowEmaValue = SlowEmaIndicator.Last;
                    double fastEmaValue = FastEmaIndicator.Last;

                    WriteIndicatorSignal("EMA X UP", string.Format("FastEma={0:F4}, SlowEma={1:F4}", fastEmaValue, slowEmaValue));

                }

                AdjustSpreadSteppingBasedOnIndicatorEvent(true, false, 2);

                IsEmaCrossUp = true;
                IsEmaCrossDown = false;
            }

            return IsEmaCrossUp;
        }

       
        private bool GetIsEmaInBearishMode(Bar bar)
        {
            if (!IsEmaCrossDown && FastEmaIndicator.CrossesBelow(SlowEmaIndicator, bar))
            {
                if (GetCurrentDateTime() > CurrentValidityDateTime)
                {
                    firstCrossOverOfIndicatorHappened = true;
                }

                if (IsBarCloseEnoughForLogging(bar))
                {
                    double slowEmaValue = SlowEmaIndicator.Last;
                    double fastEmaValue = FastEmaIndicator.Last;

                    WriteIndicatorSignal("EMA X DN", string.Format("FastEma={0:F4}, SlowEma={1:F4}", fastEmaValue, slowEmaValue));

                }

                AdjustSpreadSteppingBasedOnIndicatorEvent(false, true, 2);

                IsEmaCrossDown = true;
                IsEmaCrossUp = false;
            }

            return IsEmaCrossDown;
        }



        private bool GetIsStochInBearishMode(Bar bar)
        {
            if (!IsStochCrossDown && KSlowIndicator.CrossesBelow(DSlowIndicator, bar))
            {
                if (GetCurrentDateTime() > CurrentValidityDateTime)
                {
                    firstCrossOverOfIndicatorHappened = true;
                }

                if (IsBarCloseEnoughForLogging(bar))
                {
                    double currentKValue = KSlowIndicator.Last;
                    double currentDValue = DSlowIndicator.Last;

                    WriteIndicatorSignal("STO X DN", string.Format("K={0:F4}, D={1:F4}", currentKValue, currentDValue));

                }


                AdjustSpreadSteppingBasedOnIndicatorEvent(false, true, 1);

                IsStochCrossDown = true;
                IsStochCrossUp = false;
            }

            return IsStochCrossDown;
        }



        private bool GetIsStochInBullishMode(Bar bar)
        {

            if (!IsStochCrossUp && KSlowIndicator.CrossesAbove(DSlowIndicator, bar))
            {
                if (GetCurrentDateTime() > CurrentValidityDateTime)
                {
                    firstCrossOverOfIndicatorHappened = true;
                }

                if (IsBarCloseEnoughForLogging(bar))
                {
                    double currentKValue = KSlowIndicator.Last;
                    double currentDValue = DSlowIndicator.Last;

                    WriteIndicatorSignal("STO X UP", string.Format("K={0:F4}, D={1:F4}", currentKValue, currentDValue));

                }


                AdjustSpreadSteppingBasedOnIndicatorEvent(true, false, 1);

                IsStochCrossUp = true;
                IsStochCrossDown = false;
            }

            return IsStochCrossUp;
        }


        private void WriteIndicatorSignal(string signal, string indicatorString)
        {
            if (null == CurrentOrder)
            {
                LoggingUtility.WriteInfoFormat(this, "*** {0} *** @ {1} ({2})",
                    signal,
                    GetCurrentDateTime(),
                    indicatorString);
            }
        }

        private bool GetIsNormalizedStochInBullishMode()
        {
            if (IsStochInObMode()) 
                return true;

            return IsStochCrossUp;
        }

        private bool IsStochInObMode()
        {
            double currentKValue = KSlowIndicator.Last;
            double currentDValue = DSlowIndicator.Last;

            if (currentKValue >= OverboughtStochThreshold)
            {
                WriteIndicatorSignal("STO X OB", string.Format("K={0:F4}, D={1:F4}", currentKValue, currentDValue));
                return true;
            }
            return false;
        }

        private bool GetIsNormalizedStochInBearishMode()
        {
            if (IsStochInOsMode()) 
                return true;

            return IsStochCrossDown;
        }

        private bool IsStochInOsMode()
        {
            double currentKValue = KSlowIndicator.Last;
            double currentDValue = DSlowIndicator.Last;

            if (currentKValue <= OversoldStochThreshold)
            {
                WriteIndicatorSignal("STO X OS", string.Format("K={0:F4}, D={1:F4}", currentKValue, currentDValue));
                return true;
            }
            return false;
        }


        private void AdjustSpreadSteppingBasedOnIndicatorEvent(bool isBullishEvent, bool isBearishEvent, int factor)
        {

            if (CurrentOrder != null)
            {
                if (isBullishEvent)
                {
                    if (GetOrderSide() == OrderSide.Sell)
                    {
                        AdjustSpreadStepping(factor);
                    }

                    if (GetOrderSide() == OrderSide.Buy)
                    {
                        AdjustSpreadStepping(factor * -1);
                    }
                }

                if (isBearishEvent)
                {
                    if (GetOrderSide() == OrderSide.Sell)
                    {
                        AdjustSpreadStepping(factor * -1);
                    }

                    if (GetOrderSide() == OrderSide.Buy)
                    {
                        AdjustSpreadStepping(factor);
                    }
                }
            }
        }

    }
}
