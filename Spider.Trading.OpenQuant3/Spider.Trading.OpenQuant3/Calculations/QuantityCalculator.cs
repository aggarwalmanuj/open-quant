using System;
using Spider.Trading.OpenQuant3.Diagnostics;
using Spider.Trading.OpenQuant3.Entities;
using Spider.Trading.OpenQuant3.Enums;

namespace Spider.Trading.OpenQuant3.Calculations
{
    public class QuantityCalculator
    {
        private LoggingConfig logConfig = null;

        public QuantityCalculator(LoggingConfig logConfig)
        {
            this.logConfig = logConfig;
        }

        public double CalculateOpeningQuantity(OpeningQuantityCalculatorInput input)
        {
            double quantity = 0;

            if (input.PositionSizingCalculationStrategy == PositionSizingCalculationStrategy.FixedAmount)
            {
                // calculate the size based on fixed size
                quantity = input.MaxAmountToInvest / input.TargetPrice;
            }
            else if (input.PositionSizingCalculationStrategy == PositionSizingCalculationStrategy.CalculatedBasedOnRiskAmount)
            {
                // calculate the size based on risk
                double riskPerUnit = input.TargetPrice * input.StopPercentage / 100d;
                quantity = input.MaxAmountToRisk / riskPerUnit;


                double totalAmount = quantity*input.TargetPrice;
                if (totalAmount > input.MaxAmountToInvest)
                {
                    quantity = input.MaxAmountToInvest/input.TargetPrice;
                }

            }



            quantity = CalculateClosingQuantity(quantity, new ClosingQuantityCalculatorInput()
                                                          {
                                                              RoundLots = input.RoundLots,
                                                              PositionSizePercentage = input.PositionSizePercentage
                                                          });

            if (quantity < input.MinimumPosition)
                quantity = input.MinimumPosition;

            return quantity;
        }

        public double CalculateClosingQuantity(double quantity, ClosingQuantityCalculatorInput input)
        {
            double returnValue = quantity;

            returnValue = returnValue * input.PositionSizePercentage / 100;
            returnValue = Math.Round(returnValue, 0);
            if (input.RoundLots)
                returnValue = returnValue - (returnValue % 100);

            LoggingUtility.WriteInfo(
                logConfig,
                string.Format(
                    "Total quantity: {0:n2} with a position size of {1:p} = {2}",
                    quantity,
                    input.PositionSizePercentage / 100,
                    returnValue));


            if (returnValue < input.MinimumPosition)
                returnValue = input.MinimumPosition;


            return returnValue;
        }

    }
}