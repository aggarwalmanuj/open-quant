using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQuant.API;

namespace Spider.OpenQuant.Strategies.Util
{
    static class BarExtenstions
    {
        public static BarDuration GetBarDuration(this Bar bar)
        {
            BarDuration retVal = BarDuration.Unknown;

            if (null != bar)
            {
                if (bar.Size == PeriodConstants.PERIOD_MINUTE)
                    retVal = BarDuration.Minutely;
                if (bar.Size == PeriodConstants.PERIOD_DAILY )
                    retVal = BarDuration.Daily;
            }

            return retVal;
        }
    


        public static bool IsWithinRegularTradingHours(this Bar bar, InstrumentType insType)
        {
            if (insType != InstrumentType.Stock)
                throw new NotImplementedException("Not implemented for anything other than stocks");

            BarDuration duration = GetBarDuration(bar);
            if (duration == BarDuration.Daily)
                return true;

            if (duration == BarDuration.Minutely)
                return bar.BeginTime.TimeOfDay.TotalSeconds >= PstSessionTimeConstants.StockExchangeStartTimeSeconds && bar.EndTime.TimeOfDay.TotalSeconds <= PstSessionTimeConstants.StockExchangeEndTimeSeconds;

            throw new ArgumentException(string.Format("BarDuration is not known. Duration: {0}, Bar: {1}", bar.Duration, bar));

        }



        public static bool IsSessionOpenBar(this Bar bar, InstrumentType insType)
        {
            if (insType != InstrumentType.Stock)
                throw new NotImplementedException("Not implemented for anything other than stocks");

            BarDuration duration = GetBarDuration(bar);
            if (duration == BarDuration.Daily)
                return true;

            if (duration == BarDuration.Minutely)
                return bar.BeginTime.TimeOfDay.TotalSeconds <= 23400;

            throw new ArgumentException(string.Format("BarDuration is not known. Duration: {0}, Bar: {1}", bar.Duration, bar));

        }


    }
}
