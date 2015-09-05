using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spider.Trading.OpenQuant3.Entities;
using Spider.Trading.OpenQuant3.Enums;

namespace Spider.Trading.OpenQuant3.Calculations.QuantityCalc
{
    public class FixedAmountQuantityCalculator : BaseQuantityCalculator
    {
        public FixedAmountQuantityCalculator(BaseStrategy strategy)
            : base(strategy)
        {
        }


        protected override object CalculateImpl(OpeningQuantityCalculatorInput input)
        {
            double quantity = 0;

            ValidateStrategyType(input, PositionSizingCalculationStrategy.FixedAmount);

            // calculate the size based on fixed size
            quantity = input.MaxAmountToInvest/input.TargetPrice;

            quantity = GetFinalQuantity(quantity,
                                        input.RoundLots,
                                        input.PositionSizePercentage,
                                        input.MinimumPosition);


            return quantity;
        }


        
    }
}
