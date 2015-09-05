using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQuant.API;
using OpenQuant.API.Indicators;
using Spider.OpenQuant4.Common;
using Spider.OpenQuant4.Validators;

namespace Spider.OpenQuant4.Base
{
    public abstract partial class BaseStrategy
    {
        protected void SetAndValidateValues()
        {
            CurrentExecutionTimePeriodInSeconds = ExecutionTimePeriod * PeriodConstants.PERIOD_MINUTE;

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
            Draw(DailyAtrIndicator, 3);
        }
    }
}
