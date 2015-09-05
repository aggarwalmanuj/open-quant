namespace Spider.Trading.OpenQuant3.Entities
{
    public class RiskCalculationInput
    {
        public double TotalPortfolioAmount { get; set; }

        public int PortfolioAllocationPercentage { get; set; }

        public int NumberOfPositions { get; set; }

        public double MaxPortfolioRisk { get; set; }

        public double MaxPositionRisk { get; set; }

    }
}
