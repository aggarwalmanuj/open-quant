namespace Spider.Trading.OpenQuant3.Entities
{
    public class ClosingQuantityCalculatorInput
    {
        public double OpenQuantity { get; set; }

        public bool RoundLots { get; set; }

        public double PositionSizePercentage { get; set; }

        public double MinimumPosition { get; set; }
    }
}