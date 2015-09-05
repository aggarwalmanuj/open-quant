using System;
using System.Runtime.Serialization.Formatters;

using OpenQuant.API;

using Spider.OpenQuant.Indicators.Util;

namespace Spider.OpenQuant.Indicators.Extensions
{
    internal static class DateTimeExtensions
    {
       

        public static bool IsWithinRegularTradingHours(this DateTime datetime, InstrumentType instrumentType)
        {
            if (datetime.DayOfWeek == DayOfWeek.Saturday)
                return false;

            if (instrumentType == InstrumentType.Stock)
            {
                return datetime.TimeOfDay.TotalSeconds >= PstSessionTimeConstants.StockExchangeStartTimeSeconds
                       && datetime.TimeOfDay.TotalSeconds <= PstSessionTimeConstants.StockExchangeEndTimeSeconds
                       && datetime.DayOfWeek != DayOfWeek.Saturday
                       && datetime.DayOfWeek != DayOfWeek.Sunday;
            }

            if (instrumentType == InstrumentType.Futures)
            {
                if (datetime.DayOfWeek == DayOfWeek.Sunday)
                {
                    return datetime.TimeOfDay.TotalSeconds >=
                           PstSessionTimeConstants.FuturesExchangeStartTimeInSecods;
                }
                else
                {
                    return datetime.TimeOfDay.TotalSeconds >=
                           PstSessionTimeConstants.FuturesExchangeStartTimeInSecods ||
                           datetime.TimeOfDay.TotalSeconds <=
                           PstSessionTimeConstants.FuturesExchangeEndTimeInSecods;
                }
            }

            if (instrumentType == InstrumentType.FX)
            {
                if (datetime.DayOfWeek == DayOfWeek.Sunday)
                {
                    return datetime.TimeOfDay.TotalSeconds >=
                           PstSessionTimeConstants.FxExchangeSundayStartTimeInSecods;
                }
                else
                {
                    return true;
                }
            }

            throw new NotSupportedException("Does not support instrument type of: " + instrumentType.ToString());
        }

        public static bool IsSessionOpeningBar(this DateTime datetime, long barSize, InstrumentType instrumentType)
        {

            if (!datetime.IsWithinRegularTradingHours(instrumentType))
                return false;

            if (instrumentType == InstrumentType.Futures)
            {
                return datetime.TimeOfDay.TotalSeconds <= PstSessionTimeConstants.FuturesExchangeStartTimeInSecods + barSize + 1;
            }

            if (instrumentType == InstrumentType.FX)
            {
                return datetime.TimeOfDay.TotalSeconds <= PstSessionTimeConstants.FxExchangeSundayStartTimeInSecods + barSize + 1;
            }

            if (instrumentType == InstrumentType.Stock)
            {
                return datetime.TimeOfDay.TotalSeconds <= PstSessionTimeConstants.StockExchangeStartTimeSeconds + barSize + 1;
            }

            throw new NotSupportedException("Does not support instrument type of: " + instrumentType.ToString());
            
        }

    }
}