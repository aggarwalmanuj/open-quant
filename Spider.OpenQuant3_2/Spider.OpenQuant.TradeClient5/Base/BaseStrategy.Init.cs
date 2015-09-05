using System;
using System.Drawing;
using OpenQuant.API;
using OpenQuant.API.Indicators;
using Spider.OpenQuant.TradeClient5.Common;
using Spider.OpenQuant.TradeClient5.Util;
using Spider.OpenQuant.TradeClient5.Validators;

namespace Spider.OpenQuant.TradeClient5.Base
{
    public abstract partial class BaseStrategy
    {
        #region Indicators

        protected readonly int DailySmaPeriod = 20;

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


        protected void SetAndValidateValues()
        {
            OrderRetrialCheckCounter = 0;
            OrderRetrialCounter = 0;

            OriginalMaxSpreadFractionToAllocateForPricing = MaxSpreadFractionToAllocateForPricing;

            CurrentExecutionTimePeriodInSeconds = ExecutionTimePeriod * PeriodConstants.PERIOD_MINUTE;

            SetRandomizedParameters();

            // Validate time trigger
            TriggerTimeValidator.SetAndValidateValue(this);

            // Validate retry interval
            TriggerRetryTimeIntervalValidator.SetAndValidateValue(this);
        }

        private void SetRandomizedParameters()
        {
            RandomDelayBetweenOrdersInSeconds = new Random().Next(10, 60);

            LoggingUtility.WriteInfoFormat(this, "Setting random delay of {0}s between order retries",
                RandomDelayBetweenOrdersInSeconds);

            RandomRetryCounterBetweenSlippageAdjustment = new Random().Next(5, 30);

            LoggingUtility.WriteInfoFormat(this, "Setting random counter divisor for slippage adjustment to {0}",
                RandomRetryCounterBetweenSlippageAdjustment);

            
            RandomDelayInSecondsBetweenSlippageAdjustment = new Random().Next(3, 7);

            LoggingUtility.WriteInfoFormat(this, "Setting random delay of {0}s between slippage adjustment",
                RandomDelayInSecondsBetweenSlippageAdjustment);
             
        }

        protected void SetupIndicators()
        {
            if (SlowMaPeriod <= FastMaPeriod)
            {
                throw new ArgumentOutOfRangeException("SlowMaPeriod",
                    "Slow MA period must be greater than fast MA period");
            }

            CurrentExecutionBarSeries = GetBars(BarType.Time, CurrentExecutionTimePeriodInSeconds);
            SlowEmaIndicator = new EMA(CurrentExecutionBarSeries, SlowMaPeriod, Color.Orange);
            FastEmaIndicator = new EMA(CurrentExecutionBarSeries, FastMaPeriod, Color.Cyan);
            FastEmaIndicator.Color = Color.Cyan;

            Draw(SlowEmaIndicator, 0);
            Draw(FastEmaIndicator, 0);

            KSlowIndicator = new K_Slow(CurrentExecutionBarSeries, StochasticsKPeriod, StochasticsSmoothPeriod, Color.Yellow);
            DSlowIndicator = new D_Slow(CurrentExecutionBarSeries, StochasticsKPeriod, StochasticsDPeriod,
                StochasticsSmoothPeriod,
                Color.Red);

            Draw(KSlowIndicator, 2);
            Draw(DSlowIndicator, 2);

            CurrentDailyBarSeries = GetBars(BarType.Time, PeriodConstants.PERIOD_DAILY);
            DailyAtrIndicator = new ATR(CurrentDailyBarSeries, AtrPeriod, Color.Wheat);
            DailyVolumeSmaIndicator = new SMA(CurrentDailyBarSeries, DailySmaPeriod, BarData.Volume);
            DailyPriceSmaIndicator = new SMA(CurrentDailyBarSeries, DailySmaPeriod, BarData.Close);

            Draw(DailyPriceSmaIndicator, 0);
            Draw(DailyVolumeSmaIndicator, 1);

            Draw(DailyAtrIndicator, 3);
        }

       
    }
}
