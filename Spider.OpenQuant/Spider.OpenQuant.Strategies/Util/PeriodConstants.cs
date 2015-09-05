using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider.OpenQuant.Strategies.Util
{
    public class PeriodConstants
    {
        public const int PERIOD_MINUTE = 60;
        public const int PERIOD_DAILY = 86400;
    }

    public class PstSessionTimeConstants
    {
        public const int StockExchangeStartTimeSeconds = 23400;
        public const int StockExchangeEndTimeSeconds = 46800;
    }


    public class MinNumberOfBars
    {
        public const int MinuteBars = 1600;
        public const int DailyBars = 55;
    }
}
