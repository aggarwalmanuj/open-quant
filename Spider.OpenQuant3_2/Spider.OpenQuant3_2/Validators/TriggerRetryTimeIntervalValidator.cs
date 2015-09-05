using Spider.OpenQuant3_2.Base;
using Spider.OpenQuant3_2.Util;

namespace Spider.OpenQuant3_2.Validators
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