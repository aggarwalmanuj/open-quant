using Spider.OpenQuant.TradeClient5.Common;

namespace Spider.OpenQuant.TradeClient5.Util
{
    public static class BarNameManager
    {
        public static string GetBarName(long barSize)
        {
            if (barSize == PeriodConstants.PERIOD_DAILY)
                return "01D";
            if (barSize == PeriodConstants.PERIOD_MINUTE)
                return "01M";
            if (barSize == PeriodConstants.PERIOD_2_MINUTE)
                return "02M";
            return "01D";

        }
    }
}
