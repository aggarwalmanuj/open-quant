using System;
using OpenQuant.API;

namespace Spider.OpenQuant.TradeClient5.Base
{
    public abstract partial class BaseStrategy : Strategy
    {
        #region Properties



        #region Ib Broker Account Management

        [Parameter("Ib Account Number", "Ib Account Management")]
        public string IbAccountNumber = null;

        #endregion

        #region Position Sizing Management

        [Parameter("Position Size Percentage", "Position Sizing Management")]
        public double PositionSizePercentage = 100;



        #endregion

        #region Trade Management

        [Parameter("Execution Time Period", "Trade Management")]
        public int ExecutionTimePeriod = 2;

        [Parameter("Maximum Interval In Minutes Between Order Retries", "Trade Management")]
        public int MaximumIntervalInMinutesBetweenOrderRetries = 5;

        [Parameter("Validity Trigger Date", "Trade Management")]
        public DateTime ValidityTriggerDate = DateTime.Today;

        [Parameter("Validity Trigger Hour", "Trade Management")]
        public int ValidityTriggerHour = 6;

        [Parameter("Validity Trigger Minute", "Trade Management")]
        public int ValidityTriggerMinute = 40;

        [Parameter("Project Name", "Trade Management")]
        public string ProjectName = "P00";

        [Parameter("Maximum order size %age of avg. volume", "Trade Management")]
        public double MaxPercentageOfAvgVolumeOrderSize = 0.25;

        [Parameter("Early morning trading grace period minutes", "Trade Management")]
        public double EarlyMorningTradingGradePeriodMinutes = 7;

        #endregion

        #region Time Slice Management

        [Parameter("Time Slice Interval In Minutes", "Time Slice Management")]
        public int TimeSliceIntervalInMinutes = 15;


        #endregion

        #region Indicator Settings

        [Parameter("Stochastics D Period On Execution Time Frame", "Indicator Settings")]
        public int StochasticsDPeriod = 3;

        [OptimizationParameter(3, 27, 1)]
        [Parameter("Stochastics K Period On Execution Time Frame", "Indicator Settings")]
        public int StochasticsKPeriod = 14;

        [Parameter("Stochastics Smooth Period On Execution Time Frame", "Indicator Settings")]
        public int StochasticsSmoothPeriod = 3;

        [OptimizationParameter(2, 27, 1)]
        [Parameter("Fast Ema Period On Execution Time Frame", "Indicator Settings")]
        public int FastMaPeriod = 3;

        [OptimizationParameter(3, 27, 1)]
        [Parameter("Slow Ema Period On Execution Time Frame", "Indicator Settings")]
        public int SlowMaPeriod = 8;

        [Parameter("Oversold Stoch threshold On Execution Time Frame", "Indicator Settings")]
        public double OversoldStochThreshold = 2.5;

        [Parameter("Overbought Stoch threshold On Execution Time Frame", "Indicator Settings")]
        public double OverboughtStochThreshold = 97.5;

        #endregion

        #region Slippage Management

        [Parameter("Atr Period", "Slippage Management")]
        public int AtrPeriod = 14;

        [Parameter("Min Allowed Slippage", "Slippage Management")]
        public double MinAllowedSlippage = -0.05;

        [Parameter("Max Allowed Slippage", "Slippage Management")]
        public double MaxAllowedSlippage = 0.01;

        [Parameter("Max Spread Compared To ATR", "Slippage Management")]
        public double MaxAllowedSpreadToAtrFraction = 0.45;

        [Parameter("Min Spread Compared To ATR", "Slippage Management")]
        public double MinAllowedSpreadToAtrFraction = 0.18;

        [Parameter("Max Spread For To Allocate For Price Calculation", "Slippage Management")]
        public double MaxSpreadFractionToAllocateForPricing = 0.75;

        [Parameter("Min Spread For To Allocate For Price Calculation", "Slippage Management")]
        public double MinSpreadFractionToAllocateForPricing = 0.05;

        [Parameter("Spread Stepping Fraction to Allocate For Price Calculation", "Slippage Management")]
        public double SpreadFractionSteppingForAllocationOfPricing = 0.05;


        #endregion

        #region Retry Management


        [Parameter("Adverse Movement In Price ATR Threshold", "Retry Management")]
        public double AdverseMovementInPriceAtrThreshold = 0.11;

        #endregion


        #region Quote Client Management


        [Parameter("Quote Client Connection String", "Quote Client Management")]
        public string QuoteClientConnectionString = "host=miami.spider.local;username=manuj;password=manuj";

        [Parameter("Quote Client Enabled", "Quote Client Management")] 
        public bool QuoteClientEnabled = false;


        #endregion

        #endregion
    }
}
