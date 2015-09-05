using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQuant.API;

namespace Spider.OpenQuant.Strategies.Util
{
    static class DateTimeExtensions
    {
        public static bool IsPastRegularTradingHours(this DateTime datetime, InstrumentType insType)
        {
            if (insType != InstrumentType.Stock)
                throw new NotImplementedException("Not implemented for anything other than stocks");

            return datetime.TimeOfDay.TotalSeconds > PstSessionTimeConstants.StockExchangeEndTimeSeconds;


        }
    }
}
