using System;
using Spider.Trading.OpenQuant3.Diagnostics;

namespace Spider.Trading.OpenQuant3.Validators
{
    internal static class TriggerRetryTimeIntervalValidator
    {
        internal static void SetAndValidateValue(BaseStrategy strategy)
        {
            if (strategy.OrderRetrialStrategy == Enums.OrderRetrialStrategy.AdverseMarketMovementBased
                || strategy.OrderRetrialStrategy == Enums.OrderRetrialStrategy.TimerAndAdverseMarketMovementBased)
            {
                if (strategy.AdverseMovementInPriceAtrThreshold <= 0)
                    throw new ArgumentOutOfRangeException(
                        "AdverseMovementInPriceAtrThreshold must be greater than 0 for retrial strategy AdverseMarketMovementBased");
            }
            strategy.EffectiveRetryIntervalInSeconds = (strategy.RetryTriggerIntervalMinute*60) +
                                                       strategy.RetryTriggerIntervalSecond;
            if (strategy.OrderRetrialStrategy == Enums.OrderRetrialStrategy.TimerBased
                || strategy.OrderRetrialStrategy == Enums.OrderRetrialStrategy.TimerAndAdverseMarketMovementBased)
            {
                if (strategy.EffectiveRetryIntervalInSeconds <= 0)
                    throw new ArgumentOutOfRangeException(
                        "Retry inverval must be greater than 0 for retrial strategy TimerBased");
            }

            if (strategy.OrderRetrialStrategy != Enums.OrderRetrialStrategy.None)
            {
                if (strategy.OrderRetrialStrategy == Enums.OrderRetrialStrategy.AdverseMarketMovementBased || strategy.OrderRetrialStrategy == Enums.OrderRetrialStrategy.TimerAndAdverseMarketMovementBased)
                    LoggingUtility.WriteVerbose(strategy.LoggingConfig,
                                             string.Format(
                                                 "Order will be retried every {0} seconds based on {1} strategy for maximum of {2} times. Adverse movement ATR: {3}",
                                                 strategy.EffectiveRetryIntervalInSeconds,
                                                 strategy.OrderRetrialStrategy,
                                                 strategy.MaximumRetries,
                                                 strategy.AdverseMovementInPriceAtrThreshold));
                else
                    LoggingUtility.WriteVerbose(strategy.LoggingConfig,
                                             string.Format(
                                                 "Order will be retried every {0} seconds based on {1} strategy for maximum of {2} times",
                                                 strategy.EffectiveRetryIntervalInSeconds,
                                                 strategy.OrderRetrialStrategy,
                                                 strategy.MaximumRetries));
                

            }

        }
    }
}