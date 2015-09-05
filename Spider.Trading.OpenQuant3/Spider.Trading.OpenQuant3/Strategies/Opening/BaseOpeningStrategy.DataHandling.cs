using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQuant.API;
using Spider.Trading.OpenQuant3.Calculations;
using Spider.Trading.OpenQuant3.Diagnostics;
using Spider.Trading.OpenQuant3.Entities;
using Spider.Trading.OpenQuant3.Enums;
using Spider.Trading.OpenQuant3.Validators.Opening;

namespace Spider.Trading.OpenQuant3.Strategies.Opening
{
    public partial class BaseOpeningStrategy : BaseStrategy
    {
        
        private double GetTargetQuantity(double targetPrice)
        {
            OpeningQuantityCalculatorInput input = new OpeningQuantityCalculatorInput()
            {
                MaxAmountToInvest = EffectiveAmountToInvest,
                MaxAmountToRisk = EffectiveAmountToRisk,
                PositionSizePercentage = PositionSizePercentage,
                PositionSizingCalculationStrategy = PositionSizingCalculationStrategy,
                RoundLots = RoundLots,
                TargetPrice = targetPrice,
                StopPercentage = StopPercentage,
                MinimumPosition = MinimumOrderSize
            };

            QuantityCalculator qtyCalc = new QuantityCalculator(LoggingConfig);
            return qtyCalc.CalculateOpeningQuantity(input);
        }

        
    }
}
