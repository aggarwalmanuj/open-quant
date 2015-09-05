using System;
using Spider.Trading.OpenQuant3.Calculations.QuantityCalc;
using Spider.Trading.OpenQuant3.Calculations.StopPriceCalc;
using Spider.Trading.OpenQuant3.Enums;
using Spider.Trading.OpenQuant3.Strategies.Opening;

namespace Spider.Trading.OpenQuant3.Factories
{
    public static class QuantityCalculatorFactory
    {
        public static BaseQuantityCalculator GetCalculator(BaseStrategy strategy)
        {
            BaseQuantityCalculator returnVal = null;


            switch ((strategy as BaseOpeningStrategy).PositionSizingCalculationStrategy)
            {
                case PositionSizingCalculationStrategy.FixedAmount:
                    returnVal = new FixedAmountQuantityCalculator(strategy);
                    break;
                case PositionSizingCalculationStrategy.CalculatedBasedOnRiskAmount:
                    returnVal = new CalculatedBasedOnRiskAmountQuantityCalculator(strategy);
                    break;
                default:
                    throw new NotImplementedException();
            }



            return returnVal;
        }
    }
}