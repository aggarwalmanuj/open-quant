//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using OpenQuant.API;
//using Spider.Trading.OpenQuant3.Diagnostics;
//using Spider.Trading.OpenQuant3.Enums;
//using Spider.Trading.OpenQuant3.Exceptions;

//namespace Spider.Trading.OpenQuant3.Validators
//{
//    internal static class StopPriceValidator
//    {
//        internal static void SetAndValidateValue(BaseStrategy strategy)
//        {
//            if (strategy.OrderTriggerStrategy == OrderTriggerStrategy.StopPriceBasedTrigger)
//            {
//                switch (strategy.StopCalculationStrategy)
//                {
//                    case StopPriceCalculationStrategy.FixedAmount:
//                        double? stopPrice = strategy.StopPriceHolder.GetStopPrice(strategy.Instrument.Symbol);
//                        if (null == stopPrice)
//                        {
//                            throw new StrategyIncorrectInputException("StopPrice",
//                                                                      "Stop price was not supplied for the instrument");
//                        }
//                        else
//                        {
//                            strategy.EffectiveStopPrice = stopPrice.Value;
//                        }
//                        break;
//                    case StopPriceCalculationStrategy.ProtectiveStopBasedOnAtr:
//                        strategy.EffectiveStopPrice = GetProtectiveStop(strategy);

//                        break;
//                    case StopPriceCalculationStrategy.RetracementEntryBasedOnAtr:
//                        strategy.EffectiveStopPrice = GetRetracementEntryStop(strategy);

//                        break;
//                    default:
//                        throw new NotImplementedException();
//                        //break;
//                }

//                LoggingUtility.WriteInfo(strategy.LoggingConfig,
//                                          string.Format("Found stop price of {0:c} based on calc strategy {1}",
//                                                        strategy.EffectiveStopPrice, strategy.StopCalculationStrategy));

//            }


            
//        }


//        private static double GetProtectiveStop(BaseStrategy strategy)
//        {
//            OrderSide orderSide = strategy.EffectiveOrderSide;
//            double atrValue = strategy.EffectiveAtrPrice;
//            double atrCoeff = strategy.CalculatedStopAtrCoefficient;


//            double returnValue = 0;


//            switch (orderSide)
//            {
//                case OrderSide.Sell:
//                    // Protective is to make sure price does not go lower than yesterday's low
//                    returnValue = strategy.EffectivePreviousLoPrice - (atrValue * atrCoeff);
//                    break;

//                case OrderSide.Buy:
//                    // Protective is to make sure price does not higher than yesterday's high
//                    returnValue = strategy.EffectivePreviousHiPrice + (atrValue * atrCoeff);
//                    break;

//                default:
//                    throw new NotImplementedException();

//            }

//            return returnValue;
//        }



//        private static double GetRetracementEntryStop(BaseStrategy strategy)
//        {
//            OrderSide orderSide = strategy.EffectiveOrderSide;
//            double atrValue = strategy.EffectiveAtrPrice;
//            double atrCoeff = strategy.CalculatedStopAtrCoefficient;


//            double returnValue = 0;


//            switch (orderSide)
//            {
//                case OrderSide.Sell:
//                    // Protective is to make sure price does not go lower than yesterday's low
//                    returnValue = strategy.EffectivePreviousHiPrice + (atrValue * atrCoeff);
//                    break;

//                case OrderSide.Buy:
//                    // Protective is to make sure price does not higher than yesterday's high
//                    returnValue = strategy.EffectivePreviousLoPrice - (atrValue * atrCoeff);
//                    break;

//                default:
//                    throw new NotImplementedException();

//            }

//            return returnValue;
//        }

//    }
//}
