using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQuant.API;
using Spider.Trading.OpenQuant3.Diagnostics;
using Spider.Trading.OpenQuant3.Enums;
using Spider.Trading.OpenQuant3.Validators;

namespace Spider.Trading.OpenQuant3
{
    public abstract partial class BaseStrategy
    {


        #region Gap Management

        [Parameter("Account For Gaps", "Gap Management")] 
        public bool AccountForGaps = false;

        [Parameter("Favorable Gap", "Gap Management")]
        public double FavorableGap = 0.5;

        [Parameter("Favorable Gap Allowed Slippage", "Gap Management")]
        public double FavorableGapAllowedSlippage = -0.15;


        [Parameter("Unfavorable Gap", "Gap Management")]
        public double UnfavorableGap = 0.5;

        [Parameter("Unfavorable Gap Allowed Slippage", "Gap Management")]
        public double UnfavorableGapAllowedSlippage = -0.15;

        #endregion

        #region Position Sizing Management

        [Parameter("Position Size Percentage", "Position Sizing Management")]
        public double PositionSizePercentage = 100;

        #endregion

        #region Slippage Management

        [Parameter("Atr Period", "Slippage Management")]
        public int AtrPeriod = 14;

        [Parameter("Allowed Slippage", "Slippage Management")]
        [Obsolete("Now we use auto calculated slippage based on min and max and time duration of the session")]
        public double AllowedSlippage = -0.05;

        [Parameter("Min Allowed Slippage", "Slippage Management")]
        public double MinAllowedSlippage = -0.1;

        [Parameter("Max Allowed Slippage", "Slippage Management")]
        public double MaxAllowedSlippage = 0.01;

        #endregion

        #region Data Management

        [Parameter("Persist Historical Data", "Data Options")]
        public bool PersistHistoricalData = true;

        [Parameter("Days To Go Back For Minutely Data", "Data Options")]
        public int DaysToGoBackForMinutelyData = 0;

        #endregion

        #region Time Trigger Management

        [Parameter("Validity Trigger Date", "Time Trigger Management")]
        public DateTime ValidityTriggerDate = DateTime.Today;

        [Parameter("Validity Trigger Hour", "Time Trigger Management")]
        public int ValidityTriggerHour = 6;

        [Parameter("Validity Trigger Minute", "Time Trigger Management")]
        public int ValidityTriggerMinute = 30;

        [Parameter("Validity Trigger Second", "Time Trigger Management")]
        public int ValidityTriggerSecond = 0;

        #endregion

        #region Iwm Stop Price Trigger Management

        [Parameter("Inlcude Iwm In Order", "Iwm Stop Price Trigger Management")] 
        public bool IsIwmAlsoToBeIncludedInOrder = false;

        [Parameter("OrderSide For Iwm StopTrigger Closing Orders", "Iwm Stop Price Trigger Management")]
        public OrderSide OrderSideForIwmStopTriggerForClosing = OrderSide.Buy;

        [Parameter("Iwm Stop Calculation Strategy", "Iwm Stop Price Trigger Management")]
        public StopPriceCalculationStrategy IwmStopCalculationStrategy = StopPriceCalculationStrategy.FixedAmount;

        [Parameter("Iwm Calculated Stop Atr Coefficient", "Iwm Stop Price Trigger Management")]
        public double IwmCalculatedStopAtrCoefficient = 0.42;

        [Parameter("Iwm Opening Gap Atr Coefficient", "Iwm Stop Price Trigger Management")]
        public double IwmOpeningGapAtrCoefficient = 0.33;

        [Parameter("Iwm Stop Calculation Reference Price", "Iwm Stop Price Trigger Management")]
        public StopCalculationReferencePriceStrategy IwmStopCalculationReferencePriceStrategy = StopCalculationReferencePriceStrategy.PreviousDayPrice;

        [Parameter("Iwm Opening Gap Reference Price", "Iwm Stop Price Trigger Management")]
        public StopCalculationReferencePriceStrategy IwmOpeningGapReferencePriceStrategy = StopCalculationReferencePriceStrategy.PreviousDayClosePrice;

        //[Parameter("Validity Trigger Hour", "Iwm Stop Price Trigger Management")]
        //public int ValidityTriggerHour = 6;

        //[Parameter("Validity Trigger Minute", "Iwm Stop Price Trigger Management")]
        //public int ValidityTriggerMinute = 30;

        //[Parameter("Validity Trigger Second", "Iwm Stop Price Trigger Management")]
        //public int ValidityTriggerSecond = 0;

        #endregion

        #region Log Management


        [Parameter("Log Debug Enabled", "Log Management")]
        public bool IsLogDebugEnabled = true;

        [Parameter("Log Warn Enabled", "Log Management")]
        public bool IsLogWarnEnabled = true;

        [Parameter("Log Info Enabled", "Log Management")]
        public bool IsLogInfoEnabled = true;

        [Parameter("Log Verbose Enabled", "Log Management")]
        public bool IsLogVerboseEnabled = false;

        #endregion

        #region Retry Management

        [Parameter("Order Retrial Strategy", "Retry Management")]
        public OrderRetrialStrategy OrderRetrialStrategy = OrderRetrialStrategy.None;


        [Parameter("MaximumRetries", "Retry Management")]
        public int MaximumRetries = 5;


        [Parameter("Retry Trigger Interval Minute", "Retry Management")]
        public int RetryTriggerIntervalMinute = 2;


        [Parameter("Retry Trigger Interval Second", "Retry Management")]
        public int RetryTriggerIntervalSecond = 30;

        [Parameter("Adverse Movement In Price ATR Threshold", "Retry Management")]
        public double AdverseMovementInPriceAtrThreshold = 0.11;

        [Parameter("Submit Last Retry As Market Order", "Retry Management")]
        public bool SubmitLastRetryAsMarketOrder = false;
        #endregion

        #region Loss Based Stop Management

        [Parameter("Loss Based Stop Reference Price Strategy", "Loss Based Stop Management")] 
        public LossBasedStopPriceReferenceStrategy LossBasedStopPriceReferenceStrategy = LossBasedStopPriceReferenceStrategy.PreviousDayClose;


        [Parameter("Portfolio Loss Based Stop Percentage Value", "Loss Based Stop Management")] 
        public double PortfolioLossBasedStopPercentageValue = 5.1;


        [Parameter("Position Loss Based Stop Percentage Value", "Loss Based Stop Management")]
        public double PositionLossBasedStopPercentageValue = 11.5;

        
        #endregion


        #region Stop Management

        [Parameter("Stop Calculation Strategy", "Stop Management")]
        public StopPriceCalculationStrategy StopCalculationStrategy = StopPriceCalculationStrategy.FixedAmount;

        [Parameter("Calculated Stop Atr Coefficient", "Stop Management")]
        public double CalculatedStopAtrCoefficient = 0.42;

        [Parameter("Opening Gap Atr Coefficient", "Stop Management")]
        public double OpeningGapAtrCoefficient = 0.33;

        [Parameter("Stop Calculation Reference Price", "Stop Management")] 
        public StopCalculationReferencePriceStrategy StopCalculationReferencePriceStrategy = StopCalculationReferencePriceStrategy.PreviousDayPrice;

        [Parameter("Opening Gap Reference Price", "Stop Management")]
        public StopCalculationReferencePriceStrategy OpeningGapReferencePriceStrategy = StopCalculationReferencePriceStrategy.PreviousDayClosePrice;

        #endregion


        #region Order Management

        [Parameter("Order Trigger Strategy", "Order Management")]
        public OrderTriggerStrategy OrderTriggerStrategy = OrderTriggerStrategy.TimerBasedTrigger;

        [Parameter("Price Calculation Strategy", "Order Management")]
        public PriceCalculationStrategy PriceCalculationStrategy = PriceCalculationStrategy.CalculatedBasedOnAtr;

        [Parameter("Target Order Type", "Order Management")]
        public OrderType TargetOrderType = OrderType.Limit;

        [Parameter("Auto Submit", "Order Management")]
        public bool AutoSubmit = true;

        [Parameter("Minimum Order Size", "Order Management")]
        public int MinimumOrderSize = 2;

        [Parameter("Round Lots", "Order Management")]
        public bool RoundLots = false;


        #endregion


        #region Ib Broker Account Management

        [Parameter("Ib Broker Account Number", "Ib Broker Account Management")] 
        public string IbBrokerAccountNumber = null;

        #endregion
    }
}
