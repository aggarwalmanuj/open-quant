using Spider.Trading.OpenQuant3.Entities;

namespace Spider.Trading.OpenQuant3.Calculations
{
    public class RiskCalculator
    {
        public RiskCalculationOutput Calculate(RiskCalculationInput input)
        {
            RiskCalculationOutput returnVal = new RiskCalculationOutput();


            returnVal.AllocatedPortfolioAmount = input.TotalPortfolioAmount * input.PortfolioAllocationPercentage / 100;
            returnVal.MaximumAllocatedPositionAmount = returnVal.AllocatedPortfolioAmount / input.NumberOfPositions;
            returnVal.MaximumAllocatedPortfolioRiskAmount = returnVal.AllocatedPortfolioAmount *
                                                            input.MaxPortfolioRisk /
                                                            100;
            returnVal.MaximumAllocatedPositionRiskAmountByPortfolioRisk = returnVal.MaximumAllocatedPortfolioRiskAmount /
                                                                          input.NumberOfPositions;
            returnVal.MaximumAllocatedPositionRiskAmountByPositionRisk = returnVal.AllocatedPortfolioAmount *
                                                                         input.MaxPositionRisk /
                                                                         100;
            return returnVal;
        }
    }
}