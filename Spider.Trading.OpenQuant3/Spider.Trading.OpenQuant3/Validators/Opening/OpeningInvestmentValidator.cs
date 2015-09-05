using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spider.Trading.OpenQuant3.Calculations;
using Spider.Trading.OpenQuant3.Diagnostics;
using Spider.Trading.OpenQuant3.Entities;
using Spider.Trading.OpenQuant3.Enums;
using Spider.Trading.OpenQuant3.Exceptions;
using Spider.Trading.OpenQuant3.Strategies.Opening;

namespace Spider.Trading.OpenQuant3.Validators.Opening
{
    internal static class OpeningInvestmentValidator
    {
        internal static void SetAndValidateValue(BaseOpeningStrategy strategy)
        {
            ValidateInput(strategy);

            SetInput(strategy);
        }

        private static void SetInput(BaseOpeningStrategy strategy)
        {
            LoggingUtility.WriteInfo(strategy.LoggingConfig,
                                     string.Format(
                                         "Position size allocation parameters: Portfolio Amt: {0:c}, No Of Positions {1}, Portfolio Allocation: {2}",
                                         strategy.GrandTotalPortfolioAmount,
                                         strategy.NumberOfPortfolioPositions,
                                         strategy.PortfolioAllocationPercentage));

            switch (strategy.PositionSizingCalculationStrategy)
            {
                case PositionSizingCalculationStrategy.FixedAmount:
                    strategy.EffectiveAmountToInvest = strategy.FixedAmountToInvestPerPosition;
                    break;

                case PositionSizingCalculationStrategy.CalculatedBasedOnRiskAmount:
                    SetCalculatedRiskAmountValue(strategy);
                    break;

                case PositionSizingCalculationStrategy.CalculateBasedOnPortfolioValue:
                    SetCalculatedPositionSizeValue(strategy);
                    break;
                default:

                    throw new NotImplementedException("PositionSizingCalculationStrategy not implemented");

            }

            LoggingUtility.WriteInfo(strategy.LoggingConfig,
                                     string.Format(
                                         "Position size allocation per position {0:c}",
                                         strategy.EffectiveAmountToInvest));
        }

        private static void SetCalculatedPositionSizeValue(BaseOpeningStrategy strategy)
        {
            RiskCalculator calculator = new RiskCalculator();
            RiskCalculationOutput output = calculator.Calculate(
                new RiskCalculationInput()
                    {
                        TotalPortfolioAmount = strategy.GrandTotalPortfolioAmount,
                        PortfolioAllocationPercentage = strategy.PortfolioAllocationPercentage,
                        NumberOfPositions = strategy.NumberOfPortfolioPositions,
                        MaxPortfolioRisk = strategy.MaxPortfolioRisk,
                        MaxPositionRisk = strategy.MaxPositionRisk
                    });


            strategy.EffectiveAmountToInvest = output.MaximumAllocatedPositionAmount;
        }

        private static void ValidateInput(BaseOpeningStrategy strategy)
        {
            if (strategy.PositionSizingCalculationStrategy == PositionSizingCalculationStrategy.FixedAmount &&
                !IsFixedAmountToInvestValid(strategy))
                throw new StrategyIncorrectInputException(
                    "Fixed amount to invest value is not valid for PositionAmountCalculationStrategy.FixedAmount");

            string incorrectParameter = string.Empty;
            if (strategy.PositionSizingCalculationStrategy ==
                PositionSizingCalculationStrategy.CalculateBasedOnPortfolioValue &&
                !IsPortfolioValueInputValid(strategy, ref incorrectParameter))
                throw new StrategyIncorrectInputException(incorrectParameter,
                                                          "Portfolio calculation parameters amount is not valid for RiskAmountCalculationStrategy.CalculateBasedOnPortfolioValue");

            if (strategy.PositionSizingCalculationStrategy ==
                PositionSizingCalculationStrategy.CalculatedBasedOnRiskAmount)
            {
                if (strategy.RiskAmountCalculationStrategy == RiskAmountCalculationStrategy.FixedAmount &&
                    !IsFixedAmountToRiskValid(strategy))
                    throw new StrategyIncorrectInputException(
                        "Fixed amount to risk value is not valid for RiskAmountCalculationStrategy.FixedAmount");


                if (strategy.RiskAmountCalculationStrategy ==
                    RiskAmountCalculationStrategy.CalculatedBasedOnPortfolioAmount &&
                    !IsRiskCalculationInputValid(strategy, ref incorrectParameter))
                    throw new StrategyIncorrectInputException(incorrectParameter,
                                                              "Risk calculation parameters amount is not valid for RiskAmountCalculationStrategy.CalculatedBasedOnPortfolioAmount");

            }
        }


        private static void SetCalculatedRiskAmountValue(BaseOpeningStrategy strategy)
        {

            if (strategy.RiskAmountCalculationStrategy == RiskAmountCalculationStrategy.FixedAmount)
            {
                strategy.EffectiveAmountToRisk = strategy.FixedAmountToRiskPerPosition;
            }
            else if (strategy.RiskAmountCalculationStrategy == RiskAmountCalculationStrategy.CalculatedBasedOnPortfolioAmount)
            {
                RiskCalculator calculator = new RiskCalculator();
                RiskCalculationOutput output = calculator.Calculate(
                    new RiskCalculationInput()
                        {
                            TotalPortfolioAmount = strategy.GrandTotalPortfolioAmount,
                            PortfolioAllocationPercentage = strategy.PortfolioAllocationPercentage,
                            NumberOfPositions = strategy.NumberOfPortfolioPositions,
                            MaxPortfolioRisk = strategy.MaxPortfolioRisk,
                            MaxPositionRisk = strategy.MaxPositionRisk
                        });


                strategy.EffectiveAmountToInvest = output.MaximumAllocatedPositionAmount;

                if (strategy.RiskAppetiteStrategy == RiskAppetiteStrategy.MinRisk)

                    strategy.EffectiveAmountToRisk = new double[]
                                                         {
                                                             output.MaximumAllocatedPositionRiskAmountByPortfolioRisk,
                                                             output.MaximumAllocatedPositionRiskAmountByPositionRisk
                                                         }.Min();
                else
                    strategy.EffectiveAmountToRisk = new double[]
                                                         {
                                                             output.MaximumAllocatedPositionRiskAmountByPortfolioRisk,
                                                             output.MaximumAllocatedPositionRiskAmountByPositionRisk
                                                         }.Max();
            }
            else
            {
                throw new NotImplementedException("RiskAmountCalculationStrategy not implemented");
            }

            
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static bool IsFixedAmountToInvestValid(BaseOpeningStrategy strategy)
        {
            return strategy.FixedAmountToInvestPerPosition > 0;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static bool IsFixedAmountToRiskValid(BaseOpeningStrategy strategy)
        {
            return strategy.FixedAmountToRiskPerPosition > 0;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="incorrectParameter"></param>
        /// <returns></returns>
        private static bool IsRiskCalculationInputValid(BaseOpeningStrategy strategy, ref string incorrectParameter)
        {

            if (strategy.GrandTotalPortfolioAmount <= 0)
                incorrectParameter = "GrandTotalPortfolioAmount";

            if (strategy.PortfolioAllocationPercentage <= 0)
                incorrectParameter = "PortfolioAllocationPercentage";

            if (strategy.NumberOfPortfolioPositions <= 0)
                incorrectParameter = "NumberOfPortfolioPositions";

            if (strategy.MaxPortfolioRisk <= 0)
                incorrectParameter = "MaxPortfolioRisk";

            if (strategy.MaxPositionRisk <= 0)
                incorrectParameter = "MaxPositionRisk";

            if (strategy.StopPercentage <= 0)
                incorrectParameter = "StopPercentage";

            return string.IsNullOrEmpty(incorrectParameter);
        }


        private static bool IsPortfolioValueInputValid(BaseOpeningStrategy strategy, ref string incorrectParameter)
        {

            if (strategy.GrandTotalPortfolioAmount <= 0)
                incorrectParameter = "GrandTotalPortfolioAmount";

            if (strategy.PortfolioAllocationPercentage <= 0)
                incorrectParameter = "PortfolioAllocationPercentage";

            if (strategy.NumberOfPortfolioPositions <= 0)
                incorrectParameter = "NumberOfPortfolioPositions";

            return string.IsNullOrEmpty(incorrectParameter);
        }
    }
}
