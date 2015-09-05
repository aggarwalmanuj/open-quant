using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQuant.API;
using OpenQuant.API.Indicators;
using Spider.Trading.OpenQuant3.Calculations;
using Spider.Trading.OpenQuant3.Common;
using Spider.Trading.OpenQuant3.Diagnostics;
using Spider.Trading.OpenQuant3.Enums;
using Spider.Trading.OpenQuant3.Factories;
using Spider.Trading.OpenQuant3.Util;
using Spider.Trading.OpenQuant3.Validators;

namespace Spider.Trading.OpenQuant3
{
    public abstract partial class BaseStrategy
    {

        private void TriggerOrderIfRequired(Bar bar)
        {
            bool needToTrigger = false;

            if (null != StrategyOrder)
            {
                TriggerOrderRetrialIfRequired(bar);
                return;
            }

            if (IsCurrentInstrumentIwmAndIgnorable())
                // Since IWM is being used only for trigger mechanism - we can ignore it for oredering purpose
                return;

            // Trigger based on time
            if (OrderTriggerStrategy == Enums.OrderTriggerStrategy.TimerBasedTrigger)
            {
                needToTrigger = true;
                LoggingUtility.WriteInfo(LoggingConfig, "Triggering order based on time trigger.");
            }

            // Trigger based on stop
            if (!needToTrigger
                && OrderTriggerStrategy == Enums.OrderTriggerStrategy.StopPriceBasedTrigger
                && null != StopPriceTriggerCalculator
                && DidStopPriceTrigger(bar, "STOP ORDER"))
            {
                needToTrigger = true;
                LoggingUtility.WriteInfo(LoggingConfig,
                                         string.Format("Triggering order based on '{0}' stop price trigger.",
                                                       Instrument.Symbol));
            }


            // Trigger based on position stop losses
            if (!needToTrigger
                && IsPortfolioOrPositionStopLossEnabled
                && null != ClosingStrategyPortfolioManager)
            {
                if (IsPositionStopLossEnabled
                    && ClosingStrategyPortfolioManager.GetWhetherPositionStopHasTriggered())
                {
                    needToTrigger = true;
                    LoggingUtility.WriteInfo(LoggingConfig,
                                             string.Format(
                                                 "Triggering order for '{0}' based on daily loss of {1}% per position trigger.",
                                                 Instrument.Symbol, Math.Abs(PositionLossBasedStopPercentageValue)));

                }

                if (IsPortfolioStopLossEnabled
                    && ClosingStrategyPortfolioManager.GetWhetherPortfolioStopHasTriggered())
                {
                    needToTrigger = true;
                    LoggingUtility.WriteInfo(LoggingConfig,
                                             string.Format(
                                                 "Triggering order for '{0}' based on daily loss of {1}% on whole portfolio trigger.",
                                                 Instrument.Symbol, Math.Abs(PortfolioLossBasedStopPercentageValue)));

                }
            }

            if (!needToTrigger
                && OrderTriggerStrategy == Enums.OrderTriggerStrategy.IwmStopPriceBasedTrigger
                && GetIwmStopPriceHasTriggered())
            {
                needToTrigger = true;
                LoggingUtility.WriteInfo(LoggingConfig, "Triggering order based on 'IWM' stop price trigger.");
            }

            if (needToTrigger)
            {
                TriggerOrder(bar);
            }
        }

        private void TriggerOrderRetrialIfRequired(Bar bar)
        {
            if (null == StrategyOrder || (StrategyOrder.IsFilled || StrategyOrder.IsPartiallyFilled))
                // no order has been placed - so no need to retry
                return;

            if (OrderRetrialStrategy == Enums.OrderRetrialStrategy.None)
                return;

            bool allRetriesExpired = EffectiveOrderRetriesConsumed > MaximumRetries;

            if (allRetriesExpired && !SubmitLastRetryAsMarketOrder)
                return;
            

            bool needToReTryOrder = false;

            bool isTimerElapsedSinceLastOrder = bar.BeginTime.Subtract(EffectiveLastOrderDateTime).TotalSeconds >
                                                EffectiveRetryIntervalInSeconds;

            bool isAdverseMarketConditionsMetSinceLastOrderRetry = false;

            if (OrderRetrialStrategy == Enums.OrderRetrialStrategy.AdverseMarketMovementBased ||
                OrderRetrialStrategy == Enums.OrderRetrialStrategy.TimerAndAdverseMarketMovementBased)
            {

                bool needToCheckStopPriceForReTrial = OrderRetrialStrategy == OrderRetrialStrategy.AdverseMarketMovementBased;

                if (OrderRetrialStrategy == OrderRetrialStrategy.TimerAndAdverseMarketMovementBased
                    && isTimerElapsedSinceLastOrder)
                    needToCheckStopPriceForReTrial = true;

                if (needToCheckStopPriceForReTrial
                    && EffectiveAdverseMovementInPriceAtrThresholdForRetrialValue > 0
                    && StopPriceTriggerCalculator.IsStopPriceMet(
                        EffectiveQuotePrice,
                        EffectiveAdverseMovementInPriceAtrThresholdForRetrialValue,
                        EffectiveOrderSide,
                        StopPriceCalculationStrategy.ProtectiveStopBasedOnAtr,
                        "RE-TRIAL STOP ORDER"))
                {
                    ReAdjustTheNumberOfRetrialsConsumed(EffectiveQuotePrice,
                                                        EffectiveOrderSide
                                                        );

                    isAdverseMarketConditionsMetSinceLastOrderRetry = true;
                }
            }

            if (OrderRetrialStrategy == Enums.OrderRetrialStrategy.TimerBased)
            {
                if (isTimerElapsedSinceLastOrder)
                {
                    needToReTryOrder = true;
                    LoggingUtility.WriteInfo(LoggingConfig, "Triggering RE-TRIAL order based on time trigger.");
                }
            }

            if (!needToReTryOrder
                && OrderRetrialStrategy == Enums.OrderRetrialStrategy.AdverseMarketMovementBased
                && isAdverseMarketConditionsMetSinceLastOrderRetry)
            {
                needToReTryOrder = true;
                LoggingUtility.WriteInfo(LoggingConfig, string.Format("Triggering RE-TRIAL order based on '{0}' stop price trigger.", Instrument.Symbol));
            }

            if (!needToReTryOrder
                && OrderRetrialStrategy == Enums.OrderRetrialStrategy.TimerAndAdverseMarketMovementBased
                && isTimerElapsedSinceLastOrder
                && isAdverseMarketConditionsMetSinceLastOrderRetry)
            {
                needToReTryOrder = true;
                LoggingUtility.WriteInfo(LoggingConfig, string.Format("Triggering RE-TRIAL order based on time trigger AND '{0}' stop price trigger.", Instrument.Symbol));
            }

            if (needToReTryOrder)
            {
                TriggerRetrialOrder(bar);
            }
        }

        private void ReAdjustTheNumberOfRetrialsConsumed(double lastPrice, OrderSide orderSide)
        {
            double originalOpeningPrice = EffectiveOriginalOpeningPriceAtStartOfStrategy;

            if (lastPrice <= 0)
            {
                LoggingUtility.WriteWarn(LoggingConfig,
                                         string.Format("Cannot calculate stop price condition for LAST price {0:c}",
                                                       lastPrice));
                return;
            }

            bool needToReadjustRetryCount = true;


            if (orderSide == OrderSide.Buy)
            {
                needToReadjustRetryCount = lastPrice > originalOpeningPrice;
            }
            else
            {
                needToReadjustRetryCount = lastPrice < originalOpeningPrice;
            }

            if (!needToReadjustRetryCount)
                return;

            double absPriceDiff=0;
            double atrPriceDiff=0;

            absPriceDiff = Math.Abs(lastPrice - originalOpeningPrice);
            if (EffectiveAtrPrice > 0)
                atrPriceDiff = absPriceDiff/EffectiveAtrPrice;

            // Retrial consumed has already been incremented by 1. So substract 1 from overall count calculated
            int retriesConsumed = Convert.ToInt32(atrPriceDiff/AdverseMovementInPriceAtrThreshold) - 1;


            if (retriesConsumed > 0 && EffectiveOrderRetriesConsumed < retriesConsumed)
            {
                EffectiveOrderRetriesConsumed = retriesConsumed;

                LoggingUtility.WriteInfo(
                    LoggingConfig,
                    string.Format(
                        "Incrementing the EffectiveOrderRetriesConsumed to {0} because the price {1:c} has moved {2} ATR in adverse direction from the original opening price of {3:c}",
                        retriesConsumed, lastPrice, atrPriceDiff, originalOpeningPrice));

                if (EffectiveOrderRetriesConsumed >= MaximumRetries)
                    EffectiveOrderRetriesConsumed = MaximumRetries;
                
            }

        }

        private void SetParamsForRetrialOrder()
        {
            // Since original order was already placed - we are not going to account for gaps anymore
            AccountForGaps = false;

            int tempNextRetrialNumber = EffectiveOrderRetriesConsumed;
            if (tempNextRetrialNumber >= MaximumRetries && SubmitLastRetryAsMarketOrder)
                TargetOrderType = OrderType.Market;
        }


        private void TriggerRetrialOrder(Bar bar)
        {
            bool previousCancelled = false;

            try
            {
                StrategyOrder.Cancel();
                StrategyOrder = null;

                previousCancelled = true;
                LoggingUtility.WriteWarn(LoggingConfig,
                                         string.Format("Cancelling previous order and placing a new order."));
            }
            catch (Exception ex)
            {
                LoggingUtility.WriteError(LoggingConfig, ex.ToString());
            }

            if (!previousCancelled)
                return;

            ProcessAndPlaceOrder(bar);
        }


        private void TriggerOrder(Bar bar)
        {
            ProcessAndPlaceOrder(bar);

            CaptureOriginalTimeAndPriceForOrder();
        }

        private void CaptureOriginalTimeAndPriceForOrder()
        {
            if (EffectiveOriginalOrderDateTime <= new DateTime(2000, 1, 1))
                EffectiveOriginalOrderDateTime = EffectiveLastOrderDateTime;

            if (EffectiveOriginalOrderPrice <= 0)
                EffectiveOriginalOrderPrice = EffectiveLastOrderPrice;
        }


        private void ProcessAndPlaceOrder(Bar bar)
        {
            double targetPrice = GetTargetPrice(bar);

            EffectiveLastOrderDateTime = DateTime.Now;
            EffectiveLastOrderPrice = targetPrice;

            EffectiveOrderRetriesConsumed++;

            UpdateRetrialStopPrice(bar);

            SetParamsForRetrialOrder();

            HandleOrderTriggered(bar, targetPrice);
        }
    }
}
