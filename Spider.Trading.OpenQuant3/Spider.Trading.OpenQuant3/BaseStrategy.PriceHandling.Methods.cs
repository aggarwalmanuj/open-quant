using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQuant.API;
using Spider.Trading.OpenQuant3.Calculations;
using Spider.Trading.OpenQuant3.Common;
using Spider.Trading.OpenQuant3.Diagnostics;
using Spider.Trading.OpenQuant3.Entities;
using Spider.Trading.OpenQuant3.Enums;
using Spider.Trading.OpenQuant3.Util;
using Spider.Trading.OpenQuant3.Validators;

namespace Spider.Trading.OpenQuant3
{
    /// <summary>
    /// 
    /// </summary>
    public abstract partial class BaseStrategy
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bar"></param>
        /// <returns></returns>
        protected double GetTargetPrice(Bar bar)
        {
            GapType gap = GapType.Allowed;
            double targetPrice = bar.Open;

            PriceCalculatorInput priceInput = new PriceCalculatorInput()
                                                  {
                                                      Strategy = this,
                                                      Atr = EffectiveAtrPrice,
                                                      CurrentBar = bar,
                                                      Instrument = Instrument,
                                                      OrderSide = EffectiveOrderSide
                                                  };

            PriceCalculator calc = new PriceCalculator(LoggingConfig);

            if (AccountForGaps)
            {
                double currentPrice = 0;
                double lastPrice = 0;

                gap = GetGapType(bar, ref currentPrice, ref lastPrice);

                double change = Math.Abs(currentPrice - lastPrice);

                if (gap != GapType.Allowed)
                {
                    LoggingUtility.WriteWarn(LoggingConfig, string.Format(
                        "Found a gap of {0:c} ({1:p}) which is {2} from previous price of {3} to new price of {4}",
                        change,
                        change/lastPrice,
                        gap,
                        lastPrice,
                        currentPrice));

                }

                switch (gap)
                {
                    case GapType.Allowed:
                        break;

                    case GapType.Favorable:
                        UpdateAllowedSlippages(FavorableGapAllowedSlippage, GapType.Favorable);
                        break;

                    case GapType.Unfavorable:
                        UpdateAllowedSlippages(UnfavorableGapAllowedSlippage, GapType.Unfavorable);
                        break;

                    default:

                        throw new InvalidOperationException("");
                }

                targetPrice = calc.CalculateSlippageAdjustedPrice(priceInput);

            }
            else
            {
                if (PriceCalculationStrategy == Enums.PriceCalculationStrategy.CurrentMarketPrice)
                    targetPrice = bar.Open;
                else
                    targetPrice = calc.CalculateSlippageAdjustedPrice(priceInput);
            }

            LoggingUtility.WriteDebug(LoggingConfig,
                                      string.Format("Calculated target price based on {0} strategy: {1:c}",
                                                    PriceCalculationStrategy, targetPrice));


            return targetPrice;
        }


        private void UpdateAllowedSlippages(double newSlippage, GapType gap)
        {
            double[] arr = new double[] {newSlippage, EffectiveMinAllowedSlippage, EffectiveMaxAllowedSlippage};
            double minSlip = arr.Min();
            double maxSlip = arr.Max();

            LoggingUtility.WriteDebug(LoggingConfig,
                                      string.Format(
                                          "An opening gap of '{0}' caused changes to allowed slippages. [Before - Min: {1}. Max {2}] [After - Min: {3}. Max: {4}]",
                                          gap,
                                          EffectiveMinAllowedSlippage,
                                          EffectiveMaxAllowedSlippage,
                                          minSlip,
                                          maxSlip));

            EffectiveMaxAllowedSlippage = maxSlip;
            EffectiveMinAllowedSlippage = minSlip;
        }


        protected GapType GetGapType(Bar bar, ref double currentPrice, ref double previousPrice)
        {
            Bar prevBar = GetPreviousBar(bar, PeriodConstants.PERIOD_MINUTE);

            currentPrice = bar.Open;
            previousPrice = prevBar.Close;

            PriceGapCalculatorInput priceInput = new PriceGapCalculatorInput()
                                                     {
                                                         CurrentBar = Bar,
                                                         PreviousBar = prevBar,
                                                         Atr = EffectiveAtrPrice,
                                                         FavorableGap = FavorableGap,
                                                         FavorableGapAllowedSlippage = FavorableGapAllowedSlippage,
                                                         UnfavorableGap = UnfavorableGap,
                                                         UnfavorableGapAllowedSlippage = UnfavorableGapAllowedSlippage,
                                                         OrderSide = EffectiveOrderSide,
                                                     };

            PriceGapTriggerCalculator gapTriggCalc = new PriceGapTriggerCalculator(LoggingConfig);
            return gapTriggCalc.GetGapType(priceInput);

        }

        protected bool CanStopBeTriggeredOnOpeningGap()
        {
            if (!(OrderTriggerStrategy == OrderTriggerStrategy.StopPriceBasedTrigger
                 || OrderTriggerStrategy == OrderTriggerStrategy.IwmStopPriceBasedTrigger))
                return false;


            if (!(StopCalculationStrategy == StopPriceCalculationStrategy.OpeningGap
                || StopCalculationStrategy == StopPriceCalculationStrategy.OpeningGapOrProtectiveStop
                || StopCalculationStrategy == StopPriceCalculationStrategy.OpeningGapOrRetracementEntry
                || StopCalculationStrategy == StopPriceCalculationStrategy.OpeningGapAndRetracementEntry
                || StopCalculationStrategy == StopPriceCalculationStrategy.OpeningGapAndRetracementEntry
                || StopCalculationStrategy == StopPriceCalculationStrategy.AbIfStopBasedOnAtr))
                return false;

            return true;
        }

        protected bool CanOrderBeTriggeredAtOpeningGap()
        {
            if (!(OrderTriggerStrategy == OrderTriggerStrategy.StopPriceBasedTrigger
                  || OrderTriggerStrategy == OrderTriggerStrategy.IwmStopPriceBasedTrigger))
                return false;

            if (!(StopCalculationStrategy == StopPriceCalculationStrategy.OpeningGap
                  || StopCalculationStrategy == StopPriceCalculationStrategy.OpeningGapOrProtectiveStop
                  || StopCalculationStrategy == StopPriceCalculationStrategy.OpeningGapOrRetracementEntry))
                return false;

            return true;
        }

        protected bool DidOpeningGapStopPriceTriggered(Bar bar, string stopName)
        {
            if (!CanStopBeTriggeredOnOpeningGap())
                return false;

            bool returnValue = StopPriceTriggerCalculator.IsStopPriceMet(
                EffectiveOpenPrice,
                EffectiveOpeningGapStopPrice,
                EffectiveOrderSide,
                StopCalculationStrategy,
                string.Format("{0} - OPENING GAP STOP [{1}]", Instrument.Symbol.ToUpper(), stopName));

            return returnValue;
        }

        protected bool DidStopPriceTrigger(Bar bar, string stopName)
        {
            bool openGapStopMet = DidOpeningGapStopPriceTriggered(bar, stopName);

            bool intradayStopPriceMet = StopPriceTriggerCalculator.IsStopPriceMet(
                EffectiveQuotePrice,
                EffectiveStopPrice,
                EffectiveOrderSide,
                StopCalculationStrategy,
                string.Format("{0} - INTRADAY STOP [{1}]", Instrument.Symbol.ToUpper(), stopName));

            bool returnValue = false;

            switch (StopCalculationStrategy)
            {
                case StopPriceCalculationStrategy.OpeningGap:
                    returnValue = openGapStopMet;
                    break;
                case StopPriceCalculationStrategy.OpeningGapOrProtectiveStop:
                case StopPriceCalculationStrategy.OpeningGapOrRetracementEntry:
                    returnValue = openGapStopMet || intradayStopPriceMet;
                    break;
                case StopPriceCalculationStrategy.OpeningGapAndProtectiveStop:
                case StopPriceCalculationStrategy.OpeningGapAndRetracementEntry:
                    returnValue = openGapStopMet && intradayStopPriceMet;
                    break;
                case StopPriceCalculationStrategy.FixedAmount:
                case StopPriceCalculationStrategy.ProtectiveStopBasedOnAtr:
                case StopPriceCalculationStrategy.RetracementEntryBasedOnAtr:
                    returnValue = intradayStopPriceMet;
                    break;
                case StopPriceCalculationStrategy.AbIfStopBasedOnAtr:
                    double openingBenchmarkPrice = EffectiveOpenPrice;
                    double previousBenchmarkPrice = EffectivePreviousClosePrice;

                    bool currentPriceStopInRelationToOpeningBenchmarkPrice = StopPriceTriggerCalculator.IsStopPriceMet(
                        EffectiveQuotePrice,
                        openingBenchmarkPrice,
                        EffectiveOrderSide,
                        StopCalculationStrategy,
                        string.Format("{0} - AB BENCHMARK PRICE STOP [OPENING PRICE]", Instrument.Symbol.ToUpper()));

                    bool currentPriceStopInRelationToPreviousCloseBenchmarkPrice = StopPriceTriggerCalculator.IsStopPriceMet(
                        EffectiveQuotePrice,
                        previousBenchmarkPrice,
                        EffectiveOrderSide,
                        StopCalculationStrategy,
                        string.Format("{0} - AB BENCHMARK PRICE STOP [PREV. CLOSING PRICE]", Instrument.Symbol.ToUpper()));


                    bool abCondition1 = (openGapStopMet
                                         && currentPriceStopInRelationToPreviousCloseBenchmarkPrice
                                         && currentPriceStopInRelationToOpeningBenchmarkPrice);

                    bool abCondition2 = (intradayStopPriceMet
                                         && currentPriceStopInRelationToPreviousCloseBenchmarkPrice
                                         && currentPriceStopInRelationToOpeningBenchmarkPrice);

                    returnValue = abCondition1 || abCondition2;

                    LoggingUtility.WriteInfo(LoggingConfig, string.Format(
                        "AB IF Stop Calc: {0} [Condition1: {1} (Open Gap: {2}, Prev. Close Bench: {3}, Open Bench: {4})]   [Condition2: {5} (Intraday Stop: {6}, Prev. Close Bench: {3}, Open Bench: {4})]",
                        returnValue,
                        abCondition1,
                        openGapStopMet,
                        currentPriceStopInRelationToPreviousCloseBenchmarkPrice,
                        currentPriceStopInRelationToOpeningBenchmarkPrice,
                        abCondition2,
                        intradayStopPriceMet));

                    break;
            }

            return returnValue;
        }


        protected void CheckIfIwmStopPriceHasTriggered(Bar bar)
        {
            if (!IsCurrentInstrumentIwmAndIgnorable())
                return;

            if (!IsStopPriceApplicableToThisInstrument())
                return;

            if (null == StopPriceTriggerCalculator)
                return;

            if (GetIwmStopPriceHasTriggered())
                // already triggered - no point in checking again
                return;

            bool iwmStopPriceMet = DidStopPriceTrigger(bar, "IWM PRICE TRIGGER");

            if (iwmStopPriceMet)
            {
                
            }

            if (EffectiveOrderSide == OrderSide.Buy)
                Strategy.Global[IwmStopTriggerConstants.IwmStopPriceMetForBuySide] = iwmStopPriceMet;
            else
                Strategy.Global[IwmStopTriggerConstants.IwmStopPriceMetForSellSide] = iwmStopPriceMet;
        }


        


        /// <summary>
        /// 
        /// </summary>
        /// <param name="bar"></param>
        protected void AutoCalculateRetrialParamsIfRequired(Bar bar)
        {
            if (OrderTriggerStrategy == OrderTriggerStrategy.TimerBasedTrigger)
                return;

            if (OrderRetrialStrategy == Enums.OrderRetrialStrategy.None)
                return;

            double timeDifferenceInSessionClose = GetSecondsLeftInSessionEnd(bar);

            if (timeDifferenceInSessionClose <= 0)
                return;

            if (alreadyAutoCalculatedRetries)
                return;

            int maxRetriesToBeAccomodated =
                Convert.ToInt32(Math.Floor(timeDifferenceInSessionClose/EffectiveRetryIntervalInSeconds));

            if (MaximumRetries > maxRetriesToBeAccomodated)
            {
                if (maxRetriesToBeAccomodated <= 0)
                {
                    maxRetriesToBeAccomodated = 1;
                    if (EffectiveRetryIntervalInSeconds > (timeDifferenceInSessionClose - 120))
                        EffectiveRetryIntervalInSeconds = Math.Abs((timeDifferenceInSessionClose - 120));
                }

                LoggingUtility.WriteInfo(LoggingConfig,
                                         string.Format(
                                             "Changing the maximum retry count to {0} from {1} because only {0} retries can be accomodated at an interval of {2} with {3} seconds remaining in the close.",
                                             maxRetriesToBeAccomodated,
                                             MaximumRetries,
                                             EffectiveRetryIntervalInSeconds,
                                             timeDifferenceInSessionClose
                                             ));

                MaximumRetries = maxRetriesToBeAccomodated;
            }

            alreadyAutoCalculatedRetries = true;
        }


        protected void ProcessSessionOpeningBarIfRequired(Bar bar)
        {
            if (EffectiveOpeningSessionBarAlreadyProcessed)
            {
                LoggingUtility.WriteVerbose(LoggingConfig, string.Format("Opening bar for this session has already been processed so exiting"));
                return;
            }

            bool needToProcessOpeningBar = !EffectiveStartOfStrategyDateTime.IsWithinRegularTradingHours(Instrument.Type)
                                         && bar.BeginTime.IsWithinRegularTradingHours(Instrument.Type)
                                         && bar.BeginTime >= EffectiveStartOfSessionTime;

            if (needToProcessOpeningBar)
            {
                EffectiveCurrentDayHiPrice = bar.High;
                EffectiveCurrentDayLoPrice = bar.Low;
                EffectiveOpenPrice = bar.Open;

                if (EffectiveCurrentDayHiPrice <= 0)
                    EffectiveCurrentDayHiPrice = EffectiveOpenPrice;

                if (EffectiveCurrentDayLoPrice <= 0)
                    EffectiveCurrentDayLoPrice = EffectiveOpenPrice;

                if (IsStopPriceApplicableToThisInstrument() && StopPriceCalculator != null)
                {
                    StopPriceCalculator.Calculate(null);
                }

                if (IsPortfolioOrPositionStopLossEnabled || ClosingStrategyPortfolioManager != null)
                {
                    ClosingStrategyPortfolioManager.UpdatePricesForPosition();
                }

                LoggingUtility.WriteDebug(LoggingConfig, string.Format("New opening bar was processed {0}", bar));

                EffectiveOpeningSessionBarAlreadyProcessed = true;
            }
            else
            {
                if (bar.BeginTime >= EffectiveStartOfSessionTime && bar.BeginTime.IsWithinRegularTradingHours(Instrument.Type))
                {
                    LoggingUtility.WriteVerbose(LoggingConfig, string.Format("The strategy started within trading hours - so opening bar processing is not required."));
                    EffectiveOpeningSessionBarAlreadyProcessed = true;
                }
            }
            
        }

        protected bool GetIwmStopPriceHasTriggered()
        {
            bool returnValue = false;
            string key = null;

            if (EffectiveOrderSide == OrderSide.Buy)
                key = IwmStopTriggerConstants.IwmStopPriceMetForBuySide;
            else
                key = IwmStopTriggerConstants.IwmStopPriceMetForSellSide;

            if (!string.IsNullOrEmpty(key))
            {
                if (Strategy.Global.ContainsKey(key))
                {
                    object val = Strategy.Global[key];
                    try
                    {
                        if (null != val)
                            returnValue = Convert.ToBoolean(val);
                    }
                    catch
                    {
                    }
                }
            }

            if (returnValue)
            {
                
            }

            return returnValue;
        }


        internal double GetEffectiveAllowedSlippage(Bar bar)
        {
            if (Instrument.Type != InstrumentType.Stock)
                return 0;

            if (TargetOrderType == OrderType.Market)
                return 0;

            double timeDifferenceInSessionClose = GetSecondsLeftInSessionEnd(bar);
            double returnValue = EffectiveMaxAllowedSlippage;

            double differencePer30Minutes = (EffectiveMaxAllowedSlippage - EffectiveMinAllowedSlippage) / 12;


            double timeDifferenceInMinutes = timeDifferenceInSessionClose / 60;
            int halfHourPeriodsLeft = Convert.ToInt32(Math.Floor(timeDifferenceInMinutes / 30));

            if (halfHourPeriodsLeft > 12)
                halfHourPeriodsLeft = 12;
            if (halfHourPeriodsLeft < 0)
                halfHourPeriodsLeft = 0;

            returnValue = EffectiveMaxAllowedSlippage - (differencePer30Minutes * halfHourPeriodsLeft);

            LoggingUtility.WriteDebug(LoggingConfig,
                                      string.Format(
                                          "Calculated allowed slippage: {0}. [Min: {1}. Max: {2}. Half Hour Period {3}]",
                                          returnValue,
                                          EffectiveMinAllowedSlippage,
                                          EffectiveMaxAllowedSlippage,
                                          halfHourPeriodsLeft));

            return returnValue;
        }
    }
}
