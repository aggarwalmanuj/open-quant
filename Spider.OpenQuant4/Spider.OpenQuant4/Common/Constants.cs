using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spider.OpenQuant4.Common
{
    public class PeriodConstants
    {
        public const int PERIOD_MINUTE = 60;
        public const int PERIOD_2_MINUTE = 60*2;
        public const int PERIOD_DAILY = 86400;
    }

    public class PstSessionTimeConstants
    {
        public const int StockExchangeStartTimeSeconds = 23400;
        public const int StockExchangeEndTimeSeconds = 46800;
    }


    public class MinNumberOfBars
    {
        public const int MinuteBars = 200;
        public const int DailyBars = 55;
    }
}
