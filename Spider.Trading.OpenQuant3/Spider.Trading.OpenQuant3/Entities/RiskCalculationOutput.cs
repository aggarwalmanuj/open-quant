namespace Spider.Trading.OpenQuant3.Entities
{
    public class RiskCalculationOutput
    {
        public double AllocatedPortfolioAmount { get; set; }

        public double TotalPortfolioRiskAmount { get; set; }

        //public double PortfolioRiskAmountPerPosition { get; set; }

        //public double RiskAmountPerPosition { get; set; }

        public double MaximumAllocatedPositionAmount { get; set; }

        public double MaximumAllocatedPortfolioRiskAmount { get; set; }

        public double MaximumAllocatedPositionRiskAmountByPortfolioRisk { get; set; }

        public double MaximumAllocatedPositionRiskAmountByPositionRisk { get; set; }
    }
}