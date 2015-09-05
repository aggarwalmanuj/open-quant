using System;
using OpenQuant.API;
using Spider.OpenQuant3_2.Common;

namespace Spider.OpenQuant3_2.Extensions
{
    public static class BarExtenstions
    {
        

        public static bool IsDailyBar(this Bar bar)
        {
            return bar.Size == PeriodConstants.PERIOD_DAILY;
        }


        public static bool IsWithinRegularTradingHours(this Bar bar, InstrumentType insType)
        {
            if (insType != InstrumentType.Stock)
                throw new NotImplementedException("Not implemented for anything other than stocks");

            if (IsDailyBar(bar))
                return true;

            return bar.BeginTime.IsWithinRegularTradingHours(insType);
        }



        public static bool IsSessionOpenBar(this Bar bar, InstrumentType insType)
        {
            if (insType != InstrumentType.Stock)
                throw new NotImplementedException("Not implemented for anything other than stocks");

            if (IsDailyBar(bar))
                return true;

            return bar.BeginTime.TimeOfDay.TotalSeconds <= 23400;
        }

        
    }
}
