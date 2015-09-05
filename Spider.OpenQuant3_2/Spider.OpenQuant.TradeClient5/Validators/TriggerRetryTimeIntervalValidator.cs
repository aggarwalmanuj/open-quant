using Spider.OpenQuant.TradeClient5.Base;
using Spider.OpenQuant.TradeClient5.Util;

namespace Spider.OpenQuant.TradeClient5.Validators
{
    internal static class TriggerRetryTimeIntervalValidator
    {
        internal static void SetAndValidateValue(BaseStrategy strategy)
        {

            LoggingUtility.WriteInfo(strategy,
                string.Format("Order will be retried at minimum 01m and maximum of {0}m intervals.",
                    strategy.MaximumIntervalInMinutesBetweenOrderRetries.ToString("00")));

        }
    }
}