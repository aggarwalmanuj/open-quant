using System;
using Spider.Trading.OpenQuant3.Diagnostics;
using Spider.Trading.OpenQuant3.Enums;

namespace Spider.Trading.OpenQuant3.Calculations.StopPriceCalc
{
    public abstract class BaseStopPriceCalculator : BaseStrategyCalculator
    {
        public BaseStopPriceCalculator(BaseStrategy strategy)
            : base(strategy)
        { }

        protected void ValidateStrategyType(StopPriceCalculationStrategy stratType)
        {
            //if (Strategy.StopCalculationStrategy != stratType)
            //    throw new ArgumentException("StopCalculationStrategy", "StopCalculationStrategy");
        }

        public abstract bool NeedToUpdateCurrentDayHiLoPrice();

        public virtual StopCalculationReferencePriceStrategy GetStopPriceReferenceStrategy()
        {
            return Strategy.StopCalculationReferencePriceStrategy;
        }

        protected void SetIntradayStopPrice(double value, double hi, double lo, double atrCoeff, double atrCoeffPrice)
        {
            if (value != Strategy.EffectiveStopPrice)
            {
                Strategy.EffectiveStopPrice = value;
                LogChangedStopPrice(hi, lo, value, atrCoeff, atrCoeffPrice, "Intraday");
            }
            else
            {
                LogUnchangedStopPrice(hi, lo, value, atrCoeff, atrCoeffPrice, "Intraday");
            }
        }

        protected void SetOpeningGapStopPrice(double value, double hi, double lo, double atrCoeff, double atrCoeffPrice)
        {
            if (value != Strategy.EffectiveOpeningGapStopPrice)
            {
                Strategy.EffectiveOpeningGapStopPrice = value;
                LogChangedStopPrice(hi, lo, value, atrCoeff, atrCoeffPrice, "Opening Gap");
            }
            else
            {
                LogUnchangedStopPrice(hi, lo, value, atrCoeff, atrCoeffPrice, "Opening Gap");
            }
        }

        private void LogUnchangedStopPrice(double hi, double lo, double stopPrice, double atrCoeff, double atrCoeffPrice, string stopType)
        {
            LoggingUtility.WriteDebug(Strategy.LoggingConfig,
                                      string.Format(
                                          "Original '{0}' stop price of {1:c} ({5:p}) was left unchanged. {2} {3} {4}",
                                          stopType,
                                          stopPrice,
                                          GetStrategyStringForStopLogging(),
                                          GetAtrStringForStopLogging(atrCoeff, atrCoeffPrice),
                                          GetPriceStringForStopLogging(hi, lo),
                                          Strategy.GetPriceDiffAsPercentageOfPreviousClosingPrice(stopPrice)
                                          ));
        }

        private void LogChangedStopPrice(double hi, double lo, double stopPrice, double atrCoeff, double atrCoeffPrice, string stopType)
        {
            LoggingUtility.WriteInfo(Strategy.LoggingConfig,
                                     string.Format(
                                          "Found a new '{0}' stop price of {1:c} ({5:p}). {2} {3} {4}",
                                          stopType,
                                          stopPrice,
                                          GetStrategyStringForStopLogging(),
                                          GetAtrStringForStopLogging(atrCoeff, atrCoeffPrice),
                                          GetPriceStringForStopLogging(hi, lo),
                                          Strategy.GetPriceDiffAsPercentageOfPreviousClosingPrice(stopPrice)
                                          ));
        }

        private string GetStrategyStringForStopLogging()
        {
            return string.Format("[Strategy: {0} ({1})]",
                                 Strategy.StopCalculationStrategy,
                                 GetStopPriceReferenceStrategy()
                );
        }

        private string GetAtrStringForStopLogging(double atrCoeff, double atrCoeffPrice)
        {
            return string.Format("[Atr: {0:c}({1:p}). Atr-Coeff: {2}. Atr-Delta: {3:c}({4:p})]",
                                 Strategy.EffectiveAtrPrice,
                                 Strategy.EffectiveAtrAsPercentageOfLastClosePrice,
                                 atrCoeff,
                                 atrCoeffPrice,
                                 Strategy.GetPriceAsPercentageOfPreviousClosingPrice(atrCoeffPrice)
                );
        }

        private string GetPriceStringForStopLogging(double hi, double lo)
        {
            return string.Format("[Hi: {0:c}. Lo: {1:c}. Open: {2:c}. Prev Close: {3:c}]",
                                 hi,
                                 lo,
                                 Strategy.EffectiveOpenPrice,
                                 Strategy.EffectivePreviousClosePrice
                );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected double GetEffectiveHiPrice()
        {
            StopCalculationReferencePriceStrategy priceRefStrategy = GetStopPriceReferenceStrategy();
            double effectiveHiPrice = Strategy.EffectivePreviousHiPrice;

            if (priceRefStrategy == Enums.StopCalculationReferencePriceStrategy.CurrentDayHiLoPrice)
            {
                effectiveHiPrice = Strategy.EffectiveCurrentDayHiPrice;
            }
            else if (priceRefStrategy == StopCalculationReferencePriceStrategy.CurrentDayOpenPrice)
            {
                if (Strategy.EffectiveOpenPrice <= 0)
                    effectiveHiPrice = Strategy.EffectivePreviousHiPrice;
                else
                    effectiveHiPrice = Strategy.EffectiveOpenPrice;
            }
            else if (priceRefStrategy == StopCalculationReferencePriceStrategy.PreviousDayClosePrice)
            {
                effectiveHiPrice = Strategy.EffectivePreviousClosePrice;
            }

            return effectiveHiPrice;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected double GetEffectiveLoPrice()
        {
            StopCalculationReferencePriceStrategy priceRefStrategy = GetStopPriceReferenceStrategy();
            double effectiveLoPrice = Strategy.EffectivePreviousLoPrice;

            if (priceRefStrategy == Enums.StopCalculationReferencePriceStrategy.CurrentDayHiLoPrice)
            {
                effectiveLoPrice = Strategy.EffectiveCurrentDayLoPrice;
            }
            else if (priceRefStrategy == StopCalculationReferencePriceStrategy.CurrentDayOpenPrice)
            {
                if (Strategy.EffectiveOpenPrice <= 0)
                    effectiveLoPrice = Strategy.EffectivePreviousLoPrice;
                else
                    effectiveLoPrice = Strategy.EffectiveOpenPrice;
            }
            else if (priceRefStrategy == StopCalculationReferencePriceStrategy.PreviousDayClosePrice)
            {
                effectiveLoPrice = Strategy.EffectivePreviousClosePrice;
            }

            return effectiveLoPrice;
        }
        
    }
}