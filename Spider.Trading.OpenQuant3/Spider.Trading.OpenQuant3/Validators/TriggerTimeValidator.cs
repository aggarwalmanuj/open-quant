using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQuant.API;
using Spider.Trading.OpenQuant3.Common;
using Spider.Trading.OpenQuant3.Diagnostics;
using Spider.Trading.OpenQuant3.Strategies.Opening;

namespace Spider.Trading.OpenQuant3.Validators
{

   
    internal static class TriggerTimeValidator
    {
        internal static void SetAndValidateValue(BaseStrategy strategy)
        {

            strategy.EffectiveValidityTriggerTime = new DateTime(strategy.ValidityTriggerDate.Year, strategy.ValidityTriggerDate.Month, strategy.ValidityTriggerDate.Day)
                .AddHours(strategy.ValidityTriggerHour)
                .AddMinutes(strategy.ValidityTriggerMinute)
                .AddSeconds(strategy.ValidityTriggerSecond);

            strategy.EffectiveStartOfSessionTime = new DateTime(strategy.ValidityTriggerDate.Year,
                                                                strategy.ValidityTriggerDate.Month,
                                                                strategy.ValidityTriggerDate.Day)
                .AddSeconds(PstSessionTimeConstants.StockExchangeStartTimeSeconds);

            if (strategy is BaseOpeningStrategy)
            {
                BaseOpeningStrategy opS = strategy as BaseOpeningStrategy;
                LoggingUtility.WriteInfo(strategy.LoggingConfig,
                                         string.Format(
                                             "Queued order to {0} {1} after {2} for a portfolio of {3:c} with {4} positions",
                                             opS.OrderSide,
                                             strategy.Instrument.Symbol,
                                             strategy.EffectiveValidityTriggerTime,
                                             opS.GrandTotalPortfolioAmount,
                                             opS.NumberOfPortfolioPositions));
            }
            else
            {

                LoggingUtility.WriteInfo(strategy.LoggingConfig,
                                         string.Format("Queued order to close {0} after {1}",
                                                       strategy.Instrument.Symbol,
                                                       strategy.EffectiveValidityTriggerTime));
            }
          



            LoggingUtility.WriteVerbose(strategy.LoggingConfig, "Completed basic initializationation");
        }
    }
}
