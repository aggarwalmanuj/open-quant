using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQuant.API;

namespace Spider.OpenQuant.Strategies.Util
{
    class QuantityCalculator
    {
        private LoggingConfig logConfig = null;

        public QuantityCalculator(LoggingConfig logConfig)
        {
            this.logConfig = logConfig;
        }

        public double Calculate(QuantityCalculatorInput input)
        {
            if (input.PortfolioAllocationPercentage <= 0)
                throw new ArgumentOutOfRangeException("input.PortfolioAllocationPercentage", input.PortfolioAllocationPercentage, "Portfolio allocation cannot be zero or negative");

            double allocatedPortfolioAmount = input.PortfolioAmt*input.PortfolioAllocationPercentage/100;

            LoggingUtility.WriteInfo(logConfig, string.Format("Total capital to deploy for this portfolio is {0:c}",
                                                   allocatedPortfolioAmount));

            double maxPortfolioRiskAmount = allocatedPortfolioAmount * input.MaxPortfolioRisk / 100;
            double maxPortfolioRiskAmountPerPosition = maxPortfolioRiskAmount / input.NumberOfPositions;

            double maxPositionRiskAmount = allocatedPortfolioAmount * input.MaxPositionRisk / 100;

            double finalPositionRiskAmount = Math.Min(maxPortfolioRiskAmountPerPosition, maxPositionRiskAmount);
            
            double stopPrice = input.TargetPrice * input.StopPercentage / 100;
            double quantity = finalPositionRiskAmount / stopPrice;
            double maxPositionAmount = allocatedPortfolioAmount / input.NumberOfPositions;
            double calcPositionAmount = input.TargetPrice * quantity;

            if (calcPositionAmount > maxPositionAmount)
            {
                quantity = maxPositionAmount / input.TargetPrice;
            }

            LoggingUtility.WriteInfo(
                logConfig,
                string.Format(
                    "Qty calc: [Port Risk: {0:c}] [Pos Risk: {1:c}] [Stop Price: {2:c}] [Qty: {3:n2}]",
                    maxPortfolioRiskAmountPerPosition,
                    maxPositionRiskAmount,
                    stopPrice,
                    quantity));
            

            return CalculatePositionSizedQuantity(quantity, input);
        }

        public double CalculatePositionSizedQuantity( double quantity, QuantityCalculatorInput input)
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
                    input.PositionSizePercentage/100,
                    returnValue));


            return returnValue;
        }

    }


    public  class QuantityCalculatorInput
    {
        public double PortfolioAmt { get; set; }

        public double PortfolioAllocationPercentage { get; set; }

        public double MaxPortfolioRisk { get; set; }

        public double MaxPositionRisk { get; set; }

        public double NumberOfPositions { get; set; }

        public double StopPercentage { get; set; }

        public double TargetPrice { get; set; }

        public bool RoundLots { get; set; }

        public double PositionSizePercentage { get; set; }
    }
}
