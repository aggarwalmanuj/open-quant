using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenQuant.API;
using Spider.OpenQuant4.Util;
using Spider.OpenQuant4.Common;

namespace Spider.OpenQuant4.Extensions
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
