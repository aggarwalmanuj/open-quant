using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQuant.API;
using Spider.Trading.OpenQuant3.Calculations;
using Spider.Trading.OpenQuant3.Common;
using Spider.Trading.OpenQuant3.Diagnostics;
using Spider.Trading.OpenQuant3.Enums;
using Spider.Trading.OpenQuant3.Events;
using Spider.Trading.OpenQuant3.Util;
using Spider.Trading.OpenQuant3.Validators;

namespace Spider.Trading.OpenQuant3
{



    public abstract partial class BaseStrategy : Strategy
    {


        #region Effective Variables

        public DateTime EffectiveValidityTriggerTime = DateTime.MaxValue;
        public DateTime EffectiveStartOfSessionTime = DateTime.MaxValue;
        public double EffectiveRetryIntervalInSeconds;
        public double EffectivePreviousClosePrice;
        public double EffectiveAtrPrice;
        public double EffectivePreviousHiPrice;
        public double EffectivePreviousLoPrice;
        public double EffectiveCurrentDayHiPrice;
        public double EffectiveCurrentDayLoPrice;
        public double EffectiveStopPrice;
        public double EffectiveOriginalOpeningPriceAtStartOfStrategy;
        public double EffectiveOpeningGapStopPrice;
        public double EffectiveImmediatePastQuotePrice;
        public double EffectiveQuotePrice;
        public double EffectiveChangeInPrice;
        public double EffectiveOpenPrice = -1;
        public double EffectiveOriginalOrderPrice = -1;
        public double EffectiveLastOrderPrice = -1;
        public DateTime EffectiveStartOfStrategyDateTime = DateTime.MinValue;
        public DateTime EffectiveOriginalOrderDateTime = DateTime.MinValue;
        public DateTime EffectiveLastOrderDateTime = DateTime.MinValue;
        public int EffectiveOrderRetriesConsumed = 0;
        public double EffectiveAdverseMovementInPriceAtrThresholdForRetrialValue = -1;
        public bool EffectiveOpeningSessionBarAlreadyProcessed = false;
        public double EffectiveMaxAllowedSlippage = 0;
        public double EffectiveMinAllowedSlippage = 0;
        

        public OrderSide EffectiveOrderSide = OrderSide.Buy;
        public bool IsValidityTimeTriggered;
        private bool alreadyAutoCalculatedRetries = false;

        //private StopPriceHolder stopHolder;
        private StopPriceTriggerCalculator stopPriceTriggerCalc;
        protected Order StrategyOrder = null;
        protected BaseStrategyCalculator StopPriceCalculator = null;
        protected ClosingStrategyPortfolioManager ClosingStrategyPortfolioManager = null;
        protected StrategyTypeOpenClose StrategyTypeOpenClose = StrategyTypeOpenClose.Unknown;

        #endregion
        

        #region Events

        

        #endregion


        public BarSeries DailyBarSeries = null;
        private BarSeries MinutelyBarSeries = null;
        protected LoggingConfig logConf = null;

        #region Abstract Methods

        //protected abstract void HandleOrderFill(Order order);

        protected abstract void HandleValidateInput();

        protected abstract void HandleStrategyStarting();

        //protected abstract void HandleStrategyInitialization();

        //protected abstract void HandleStrategyReInitialization();

        protected abstract void HandleBarOpened(Bar bar);

        protected abstract void HandleOrderTriggered(Bar bar, double targetPrice);

        protected abstract void HandleStrategyStarted();

        protected abstract OrderSide GetEffectiveOrderSide();

        #endregion

        /// <summary>
        /// The entry point into the strategy - when it starts for the first tiime
        /// </summary>
        protected virtual void OnStrategyStartReceived()
        {
            Console.WriteLine();
            Console.WriteLine("!!!! REMEMBER TO CHANGE STOPS AND QUANTITIES !!!!");
            Console.WriteLine();

            EffectiveStartOfStrategyDateTime = DateTime.Now;

            // First setup the main objects
            SetAndValidateValues();

            // Give a change to inherited strategies to start their own init data
            HandleStrategyStarting();

            // Setup the required daily data from data provider
            SetupData();

            // Now comes the initialization
            OnStrategyInitialize();

            // Get the order side
            EffectiveOrderSide = GetEffectiveOrderSide();

            // Calculate the stop prices
            SetAndValidateStopPrice();

            // Setup Slippage
            SlippageValidator.SetAndValidateValue(this);

            // Validate order triggers
            SetAndValidateOrderTrigger();

            // Allow inherited strategies to perform some post start setup
            HandleStrategyStarted();
        }


        protected virtual void OnBarOpenReceived(Bar bar)
        {

            if (!IsBarValid(bar))
            {
                LoggingUtility.WriteWarn(LoggingConfig, "Discarding an invalid Bar. waiting for next bar.");
                return;
            }

            ProcessSessionOpeningBarIfRequired(bar);

            AutoCalculateRetrialParamsIfRequired(bar);

            if (!IsValidityTimeTriggered && bar.BeginTime >= EffectiveValidityTriggerTime)
            {
                IsValidityTimeTriggered = true;

                LoggingUtility.LogOkToTriggerOrder(LoggingConfig, bar);


                EffectiveOriginalOpeningPriceAtStartOfStrategy = EffectiveOpenPrice;
            }

            RecalculateStopPriceBasedOnNewHiLoForTheDay(bar);

            bool openingGapTriggered = EffectiveOpeningSessionBarAlreadyProcessed
                                       && CanOrderBeTriggeredAtOpeningGap()
                                       && DidOpeningGapStopPriceTriggered(bar, "Opening Gap Stop Test");

            if (IsValidityTimeTriggered || openingGapTriggered)
            {
                LoggingUtility.LogCurrentBarArrival(LoggingConfig, bar);

                EffectiveImmediatePastQuotePrice = EffectiveQuotePrice;
                EffectiveQuotePrice = bar.Open;
                if (EffectiveQuotePrice <= 0)
                    EffectiveQuotePrice = bar.Close;
                EffectiveChangeInPrice = EffectiveQuotePrice - EffectivePreviousClosePrice;

                CheckIfIwmStopPriceHasTriggered(bar);

                HandleBarOpened(bar);

                TriggerOrderIfRequired(bar);
            }
        }




        /// <summary>
        /// The entry point into the strategy - when it starts for the first tiime
        /// </summary>
        protected virtual void OnStrategyInitialize()
        {
            
            // Now comes the initialization
            if (!IsInitialized)
                StrategyInitialize();
            else
                StrategyReInitialize();

        }


    }
}
