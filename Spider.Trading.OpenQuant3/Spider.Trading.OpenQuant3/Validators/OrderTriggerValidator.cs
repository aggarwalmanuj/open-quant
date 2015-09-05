using System;
using Spider.Trading.OpenQuant3.Common;
using Spider.Trading.OpenQuant3.Diagnostics;
using Spider.Trading.OpenQuant3.Util;

namespace Spider.Trading.OpenQuant3.Validators
{
    internal static class OrderTriggerValidator
    {
        internal static void SetAndValidateValue(BaseStrategy strategy)
        {

            switch (strategy.OrderTriggerStrategy)
            {
                case Enums.OrderTriggerStrategy.TimerBasedTrigger:
                    
                    LoggingUtility.WriteVerbose(strategy.LoggingConfig,
                                             string.Format("Order will be triggered at {0} based on {1} order trigger strategy",
                                                           strategy.EffectiveValidityTriggerTime,
                                                           strategy.OrderTriggerStrategy));
                    break;

                case Enums.OrderTriggerStrategy.StopPriceBasedTrigger:

                    if (strategy.StopCalculationStrategy == Enums.StopPriceCalculationStrategy.FixedAmount)
                    {
                        double? stopPrice = SolutionUtility.StopPriceHolder.GetStopPrice(strategy.Instrument.Symbol);
                        if (!stopPrice.HasValue || stopPrice.Value <= 0)
                            throw new ArgumentOutOfRangeException(
                                "Invalid or unavailable stop price for a fixed price stop strategy");

                        LoggingUtility.WriteVerbose(strategy.LoggingConfig,
                                             string.Format(
                                                 "Order will be triggered after {0} if price of {1:c} is breached. [Order Trigger Strategy: {2}.{3}]",
                                                 strategy.EffectiveValidityTriggerTime,
                                                 stopPrice.Value,
                                                 strategy.OrderTriggerStrategy,
                                                 strategy.StopCalculationStrategy));
                    }
                    else
                    {
                        if (strategy.CalculatedStopAtrCoefficient <=0)
                                throw new ArgumentOutOfRangeException(
                                    "Invalid or unavailable CalculatedStopAtrCoefficient for a calculated stop price strategy");

                        LoggingUtility.WriteVerbose(strategy.LoggingConfig,
                                             string.Format(
                                                 "Order will be triggered after {0} if price breaches the previous ref price of {1} by {2} ATR. [Order Trigger Strategy: {3}]",
                                                 strategy.EffectiveValidityTriggerTime,
                                                 strategy.StopCalculationReferencePriceStrategy,
                                                 strategy.CalculatedStopAtrCoefficient,
                                                 strategy.OrderTriggerStrategy));
                    }
                    break;

                case Enums.OrderTriggerStrategy.IwmStopPriceBasedTrigger:

                    if (strategy.IwmStopCalculationStrategy == Enums.StopPriceCalculationStrategy.FixedAmount)
                    {
                        double? stopPrice = SolutionUtility.StopPriceHolder.GetStopPrice(IwmStopTriggerConstants.IwmSymbol);
                        if (!stopPrice.HasValue || stopPrice.Value <= 0)
                            throw new ArgumentOutOfRangeException(
                                "Invalid or unavailable stop price for IWM for fixed price stop strategy based on IWM stop price.");

                        LoggingUtility.WriteVerbose(strategy.LoggingConfig,
                                             string.Format(
                                                 "Order will be triggered after {0} if IWM breaches the price of {1:c}. [Order Trigger Strategy: {2}.{3}]",
                                                 strategy.EffectiveValidityTriggerTime,
                                                 stopPrice.Value,
                                                 strategy.OrderTriggerStrategy,
                                                 strategy.StopCalculationStrategy));
                    }
                    else
                    {
                        if (strategy.IwmCalculatedStopAtrCoefficient <= 0)
                            throw new ArgumentOutOfRangeException(
                                "Invalid or unavailable IwmCalculatedStopAtrCoefficient for a calculated stop price strategy based on IWM price");

                        LoggingUtility.WriteVerbose(strategy.LoggingConfig,
                                             string.Format(
                                                 "Order will be triggered after {0} if IWM price breaches the previous ref price of {1} by {2} ATR. [Order Trigger Strategy: {3}]",
                                                 strategy.EffectiveValidityTriggerTime,
                                                 strategy.IwmStopCalculationReferencePriceStrategy,
                                                 strategy.IwmCalculatedStopAtrCoefficient,
                                                 strategy.OrderTriggerStrategy));
                    }
                    break;

                case Enums.OrderTriggerStrategy.DailyLossPercentageTrigger:
                    
                    if (strategy.PositionLossBasedStopPercentageValue == 0)
                        throw new ArgumentOutOfRangeException(
                            "For a position loss based trigger strategy the PositionLossBasedStopPercentageValue must be set to something other than 0");

                    LoggingUtility.WriteVerbose(strategy.LoggingConfig,
                                             string.Format(
                                                 "Order will be triggered if the position loss after {0} is more than {1}% based on {2} price. [Order Trigger Strategy: {3}]",
                                                 strategy.EffectiveValidityTriggerTime,
                                                 strategy.PositionLossBasedStopPercentageValue,
                                                 strategy.LossBasedStopPriceReferenceStrategy,
                                                 strategy.OrderTriggerStrategy));

                    break;

                case Enums.OrderTriggerStrategy.PortfolioLossPercentageTrigger:

                    if (strategy.PortfolioLossBasedStopPercentageValue == 0)
                        throw new ArgumentOutOfRangeException(
                            "For a portfolio loss based trigger strategy the PortfolioLossBasedStopPercentageValue must be set to something other than 0");

                    LoggingUtility.WriteVerbose(strategy.LoggingConfig,
                                             string.Format(
                                                 "Order will be triggered if the total portfolio loss after {0} is more than {1}% based on {2} price. [Order Trigger Strategy: {3}]",
                                                 strategy.EffectiveValidityTriggerTime,
                                                 strategy.PortfolioLossBasedStopPercentageValue,
                                                 strategy.LossBasedStopPriceReferenceStrategy,
                                                 strategy.OrderTriggerStrategy));
                    

                    break;

            }
        }
    }
}