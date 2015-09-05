﻿using System;
using OpenQuant.API;
using Spider.Trading.OpenQuant3.Common;

namespace Spider.Trading.OpenQuant3.Util
{
    static class DateTimeExtensions
    {
        public static bool IsPastRegularTradingHours(this DateTime datetime, InstrumentType insType)
        {
            if (insType != InstrumentType.Stock)
                throw new NotImplementedException("Not implemented for anything other than stocks");

            return datetime.TimeOfDay.TotalSeconds > PstSessionTimeConstants.StockExchangeEndTimeSeconds
                || datetime.DayOfWeek == DayOfWeek.Sunday
                || datetime.DayOfWeek == DayOfWeek.Saturday;


        }

        public static bool IsWithinRegularTradingHours(this DateTime datetime, InstrumentType insType)
        {
            if (insType != InstrumentType.Stock)
                throw new NotImplementedException("Not implemented for anything other than stocks");

            return datetime.TimeOfDay.TotalSeconds >= PstSessionTimeConstants.StockExchangeStartTimeSeconds
                   && datetime.TimeOfDay.TotalSeconds <= PstSessionTimeConstants.StockExchangeEndTimeSeconds
                   && datetime.DayOfWeek != DayOfWeek.Saturday
                   && datetime.DayOfWeek != DayOfWeek.Sunday;
        }
    }
}