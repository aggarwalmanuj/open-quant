using Spider.Trading.OpenQuant3.Entities;
using Spider.Trading.OpenQuant3.Enums;

namespace Spider.Trading.OpenQuant3.Calculations.QuantityCalc
{
    public class CalculatedBasedOnRiskAmountQuantityCalculator : BaseQuantityCalculator
    {
        public CalculatedBasedOnRiskAmountQuantityCalculator(BaseStrategy strategy)
            : base(strategy)
        {
        }


        protected override object CalculateImpl(OpeningQuantityCalculatorInput input)
        {
            double quantity = 0;

            ValidateStrategyType(input, PositionSizingCalculationStrategy.CalculatedBasedOnRiskAmount);

            // calculate the size based on risk
            double riskPerUnit = input.TargetPrice * input.StopPercentage / 100d;
            quantity = input.MaxAmountToRisk / riskPerUnit;


            double totalAmount = quantity * input.TargetPrice;
            if (totalAmount > input.MaxAmountToInvest)
            {
                quantity = input.MaxAmountToInvest / input.TargetPrice;
            }

            quantity = GetFinalQuantity(quantity,
                                        input.RoundLots,
                                        input.PositionSizePercentage,
                                        input.MinimumPosition);


            return quantity;
        }
    }
}