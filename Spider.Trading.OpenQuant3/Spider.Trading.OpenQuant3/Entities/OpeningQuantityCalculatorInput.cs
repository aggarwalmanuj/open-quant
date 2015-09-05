using Spider.Trading.OpenQuant3.Enums;

namespace Spider.Trading.OpenQuant3.Entities
{
    public class OpeningQuantityCalculatorInput
    {
        public PositionSizingCalculationStrategy PositionSizingCalculationStrategy { get; set; }

        public double MaxAmountToInvest { get; set; }

        public double MaxAmountToRisk { get; set; }

        public double TargetPrice { get; set; }

        public double StopPercentage { get; set; }

        public bool RoundLots { get; set; }

        public double PositionSizePercentage { get; set; }

        public double MinimumPosition { get; set; }
    }
}