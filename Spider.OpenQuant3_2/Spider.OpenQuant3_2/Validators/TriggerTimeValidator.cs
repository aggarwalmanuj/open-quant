using System;
using Spider.OpenQuant3_2.Base;
using Spider.OpenQuant3_2.Common;
using Spider.OpenQuant3_2.Opening;
using Spider.OpenQuant3_2.Util;

namespace Spider.OpenQuant3_2.Validators
{
    internal static class TriggerTimeValidator
    {
        internal static void SetAndValidateValue(BaseStrategy strategy)
        {

            strategy.CurrentValidityDateTime = new DateTime(strategy.ValidityTriggerDate.Year,
                strategy.ValidityTriggerDate.Month, strategy.ValidityTriggerDate.Day)
                .AddHours(strategy.ValidityTriggerHour)
                .AddMinutes(strategy.ValidityTriggerMinute);

            strategy.CurrentStartOfSessionTime = new DateTime(strategy.ValidityTriggerDate.Year,
                strategy.ValidityTriggerDate.Month,
                strategy.ValidityTriggerDate.Day)
                .AddSeconds(PstSessionTimeConstants.StockExchangeStartTimeSeconds);

            strategy.CurrentEndOfSessionTime = new DateTime(strategy.ValidityTriggerDate.Year,
                strategy.ValidityTriggerDate.Month,
                strategy.ValidityTriggerDate.Day)
                .AddSeconds(PstSessionTimeConstants.StockExchangeEndTimeSeconds);

            if (strategy is BaseOpeningStrategy)
            {
                BaseOpeningStrategy opS = strategy as BaseOpeningStrategy;
                LoggingUtility.WriteInfo(opS,
                    string.Format(
                        "Queued order to {0} {1} after {2} for a portfolio of {3:c} with {4} positions",
                        opS.OpeningOrderSide,
                        strategy.Instrument.Symbol,
                        strategy.CurrentValidityDateTime,
                        opS.TotalPortfolioAmount,
                        opS.NumberOfPortfolioPositions));
            }
            else
            {
                LoggingUtility.WriteInfo(strategy,
                    string.Format(
                        "Queued order to {0} {1} after {2}",
                        strategy.GetOrderSide(),
                        strategy.Instrument.Symbol,
                        strategy.CurrentValidityDateTime));

                LoggingUtility.WriteInfo(strategy,
                                         string.Format("Queued order to close {0} after {1}",
                                                       strategy.Instrument.Symbol,
                                                       strategy.CurrentValidityDateTime));
            }




        }
    }
}
