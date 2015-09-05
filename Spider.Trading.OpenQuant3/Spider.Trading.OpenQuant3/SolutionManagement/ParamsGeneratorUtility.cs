using System;
using System.Collections.Generic;
using OpenQuant.API;
using Spider.Trading.OpenQuant3.Enums;
using Spider.Trading.OpenQuant3.Strategies.Opening;

namespace Spider.Trading.OpenQuant3.SolutionManagement
{
    public static class ParamsGeneratorUtility
    {
        

        public static Dictionary<string, object> GetGenericParamsDictionary()
        {
            var paramsDictionary = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);


            paramsDictionary.Add("Currency", "USD");
            paramsDictionary.Add("DaysToGoBackForMinutelyData", 0);
            paramsDictionary.Add("PersistHistoricalData", true);
            paramsDictionary.Add("AccountForGaps", false);
            paramsDictionary.Add("FavorableGap", 0.213);
            paramsDictionary.Add("FavorableGapAllowedSlippage", -0.19);
            paramsDictionary.Add("UnfavorableGap", 0.213);
            paramsDictionary.Add("UnfavorableGapAllowedSlippage", -0.07);

            paramsDictionary.Add("IsIwmAlsoToBeIncludedInOrder", false);
            paramsDictionary.Add("OrderSideForIwmStopTriggerForClosing", OrderSide.Buy);
            paramsDictionary.Add("IwmStopCalculationStrategy", StopPriceCalculationStrategy.FixedAmount);
            paramsDictionary.Add("IwmCalculatedStopAtrCoefficient", 0.42);
            paramsDictionary.Add("IwmOpeningGapAtrCoefficient", 0.33);
            paramsDictionary.Add("IwmStopCalculationReferencePriceStrategy",
                                 StopCalculationReferencePriceStrategy.PreviousDayPrice);
            paramsDictionary.Add("IwmOpeningGapReferencePriceStrategy",
                                 StopCalculationReferencePriceStrategy.PreviousDayClosePrice);

            paramsDictionary.Add("IsLogDebugEnabled", true);
            paramsDictionary.Add("IsLogWarnEnabled", true);
            paramsDictionary.Add("IsLogInfoEnabled", true);
            paramsDictionary.Add("IsLogVerboseEnabled", false);

            paramsDictionary.Add("LossBasedStopPriceReferenceStrategy",
                                 LossBasedStopPriceReferenceStrategy.PreviousDayClose);
            paramsDictionary.Add("PortfolioLossBasedStopPercentageValue", 5.1);
            paramsDictionary.Add("PositionLossBasedStopPercentageValue", 11.5);


            paramsDictionary.Add("AutoSubmit", true);
            paramsDictionary.Add("MinimumOrderSize", 2);
            paramsDictionary.Add("OrderSide", OrderSide.Buy);
            paramsDictionary.Add("OrderTriggerStrategy", OrderTriggerStrategy.TimerBasedTrigger);
            paramsDictionary.Add("PriceCalculationStrategy", PriceCalculationStrategy.CalculatedBasedOnAtr);
            paramsDictionary.Add("RoundLots", false);
            paramsDictionary.Add("TargetOrderType", OrderType.Limit);


            paramsDictionary.Add("GrandTotalPortfolioAmount", 10000);
            paramsDictionary.Add("PortfolioAllocationPercentage", 100);
            paramsDictionary.Add("NumberOfPortfolioPositions", 5);
            paramsDictionary.Add("MaxPortfolioRisk", 10);
            paramsDictionary.Add("MaxPositionRisk", 2);
            paramsDictionary.Add("StopPercentage", 10);

            paramsDictionary.Add("PositionSizePercentage", 100);
            paramsDictionary.Add("RiskAppetiteStrategy", RiskAppetiteStrategy.MinRisk);

            paramsDictionary.Add("FixedAmountToInvestPerPosition", 0);
            paramsDictionary.Add("FixedAmountToRiskPerPosition", 0);

            paramsDictionary.Add("PositionSizingCalculationStrategy",
                                 PositionSizingCalculationStrategy.CalculatedBasedOnRiskAmount);
            paramsDictionary.Add("RiskAmountCalculationStrategy",
                                 RiskAmountCalculationStrategy.CalculatedBasedOnPortfolioAmount);



            paramsDictionary.Add("OrderRetrialStrategy", OrderRetrialStrategy.TimerAndAdverseMarketMovementBased);
            paramsDictionary.Add("MaximumRetries", 5);
            paramsDictionary.Add("RetryTriggerIntervalMinute", 2);
            paramsDictionary.Add("RetryTriggerIntervalSecond", 30);
            paramsDictionary.Add("AdverseMovementInPriceAtrThreshold", 0.05);
            paramsDictionary.Add("SubmitLastRetryAsMarketOrder", true);

            paramsDictionary.Add("AllowedSlippage", -0.05);
            paramsDictionary.Add("MinAllowedSlippage", -0.1);
            paramsDictionary.Add("MaxAllowedSlippage", 0.01);
            paramsDictionary.Add("AtrPeriod", 14);


            paramsDictionary.Add("CalculatedStopAtrCoefficient", 0.42);
            paramsDictionary.Add("StopCalculationReferencePriceStrategy",
                                 StopCalculationReferencePriceStrategy.PreviousDayPrice);
            paramsDictionary.Add("OpeningGapAtrCoefficient", 0.33);
            paramsDictionary.Add("StopCalculationStrategy", StopPriceCalculationStrategy.FixedAmount);
            paramsDictionary.Add("OpeningGapReferencePriceStrategy",
                                 StopCalculationReferencePriceStrategy.PreviousDayClosePrice);



            return paramsDictionary;
        }


    }
}