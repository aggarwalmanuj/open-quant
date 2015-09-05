using System.Collections.Generic;
using OpenQuant.API;
using Spider.Trading.OpenQuant3.Enums;

namespace Spider.Trading.OpenQuant3.SolutionManagement
{
    public static class ParamsSetterUtility
    {


        public static void SetStopCalculationStrategy(Dictionary<string, object> paramsDictionary, StopPriceCalculationStrategy strategy)
        {
            SetParamValue(paramsDictionary, "StopCalculationStrategy", strategy);

        }

        public static void SetStopCalculationReferenceStrategy(Dictionary<string, object> paramsDictionary, StopCalculationReferencePriceStrategy strategy)
        {
            SetParamValue(paramsDictionary, "StopCalculationReferencePriceStrategy", strategy);
        }


        public static void SetTriggerOnStop(Dictionary<string, object> paramsDictionary)
        {
            SetParamValue(paramsDictionary, "OrderTriggerStrategy", OrderTriggerStrategy.StopPriceBasedTrigger);
            SetStopCalculationReferenceStrategy(paramsDictionary, StopCalculationReferencePriceStrategy.PreviousDayPrice);
            SetStopCalculationStrategy(paramsDictionary, StopPriceCalculationStrategy.ProtectiveStopBasedOnAtr);
            SetParamValue(paramsDictionary, "PriceCalculationStrategy", PriceCalculationStrategy.CalculatedBasedOnAtr);
            SetIntradayOrder(paramsDictionary);
        }

        public static void SetOrderTriggerStrategy(Dictionary<string, object> paramsDictionary, OrderTriggerStrategy strategy)
        {
            SetParamValue(paramsDictionary, "OrderTriggerStrategy", strategy);
        }

        public static void SetIntraday(Dictionary<string, object> paramsDictionary)
        {
            SetParamValue(paramsDictionary, "AccountForGaps", false);
            SetOrderTriggerStrategy(paramsDictionary, OrderTriggerStrategy.TimerBasedTrigger);
            SetParamValue(paramsDictionary, "TargetOrderType", OrderType.Limit);
        }

        public static void SetMarketOnClose(Dictionary<string, object> paramsDictionary)
        {
            SetParamValue(paramsDictionary, "AccountForGaps", false);
            SetOrderTriggerStrategy(paramsDictionary, OrderTriggerStrategy.TimerBasedTrigger);
            SetParamValue(paramsDictionary, "TargetOrderType", OrderType.MarketOnClose);
            SetParamValue(paramsDictionary, "OrderRetrialStrategy", OrderRetrialStrategy.None);
            SetParamValue(paramsDictionary, "MaximumRetries", 0);
            SetExecutionHour(paramsDictionary, 12);
            SetExecutionMinute(paramsDictionary, 30);
        }


        public static void SetOpeningOrder(Dictionary<string, object> paramsDictionary)
        {
            SetIntraday(paramsDictionary);
            SetParamValue(paramsDictionary, "AccountForGaps", true);
            SetExecutionHour(paramsDictionary, 6);
            SetExecutionMinute(paramsDictionary, 30);
        }

        public static void SetExecutionHour(Dictionary<string, object> paramsDictionary, int hour)
        {
            SetParamValue(paramsDictionary, "ValidityTriggerHour", hour);
        }

        public static void SetExecutionMinute(Dictionary<string, object> paramsDictionary, int minute)
        {
            SetParamValue(paramsDictionary, "ValidityTriggerMinute", minute);
        }


        public static void SetIntradayOrder(Dictionary<string, object> paramsDictionary)
        {
            SetParamValue(paramsDictionary, "AccountForGaps", false);
        }

        public static void SetBuyOrderSide(Dictionary<string, object> paramsDictionary)
        {
            SetOrderSide(paramsDictionary, OrderSide.Buy);
        }

        public static void SetSellOrderSide(Dictionary<string, object> paramsDictionary)
        {
            SetOrderSide(paramsDictionary, OrderSide.Sell);
        }

        public static void SetOrderSide(Dictionary<string, object> paramsDictionary, OrderSide orderSide)
        {
            SetParamValue(paramsDictionary, "OrderSideForIwmStopTriggerForClosing", orderSide);
            SetParamValue(paramsDictionary, "OrderSide", orderSide);
        }


        public static void SetParamValue(Dictionary<string, object> paramsDictionary, string paramName, object value)
        {
            if (paramsDictionary.ContainsKey(paramName))
            {
                paramsDictionary[paramName] = value;
            }
            else
            {
                paramsDictionary.Add(paramName, value);

            }
        }

        public static void SetIwmStopCalculationStrategy(Dictionary<string, object> paramsDictionary, StopPriceCalculationStrategy strategy)
        {
            SetParamValue(paramsDictionary, "IwmStopCalculationStrategy", strategy);
           

        }

        public static void SetIwmStopCalculationReferenceStrategy(Dictionary<string, object> paramsDictionary, StopCalculationReferencePriceStrategy strategy)
        {
           SetParamValue( paramsDictionary,"IwmStopCalculationReferencePriceStrategy",
                                 strategy);


        }

        public static void SetIwOpeningGapReferenceStrategy(Dictionary<string, object> paramsDictionary, StopCalculationReferencePriceStrategy strategy)
        {
            SetParamValue(paramsDictionary, "IwmOpeningGapReferencePriceStrategy",
                                 strategy);


        }

        public static void SetTriggerStrategy(Dictionary<string, object> paramsDictionary, OrderTriggerStrategy orderTriggerStrategy)
        {
           SetParamValue( paramsDictionary, "OrderTriggerStrategy", orderTriggerStrategy);
        }
    }
}