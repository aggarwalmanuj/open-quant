using System;
using System.Data.Odbc;
using System.Drawing;
using OpenQuant.API;
using OpenQuant.API.Indicators;
using Spider.OpenQuant3_2.Common;
using Spider.OpenQuant3_2.Util;
using Spider.OpenQuant3_2.Validators;

namespace Spider.OpenQuant3_2.Base
{
    public abstract partial class BaseStrategy
    {
        protected void SetAndValidateValues()
        {
            CurrentExecutionTimePeriodInSeconds = ExecutionTimePeriod*PeriodConstants.PERIOD_MINUTE;

            // Validate time trigger
            TriggerTimeValidator.SetAndValidateValue(this);

            // Validate retry interval
            TriggerRetryTimeIntervalValidator.SetAndValidateValue(this);
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
            DailyVolumeSmaIndicator = new SMA(CurrentDailyBarSeries, 20, BarData.Volume);
            DailyPriceSmaIndicator = new SMA(CurrentDailyBarSeries, 20, BarData.Close);
            Draw(DailyAtrIndicator, 3);
        }

       
    }
}
