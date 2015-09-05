using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQuant.API;
using Spider.Trading.OpenQuant3.Enums;
using Spider.Trading.OpenQuant3.Validators.Opening;

namespace Spider.Trading.OpenQuant3.Strategies.Opening
{
    public partial class BaseOpeningStrategy : BaseStrategy
    {

        #region Position Sizing Strategy

        [Parameter("Position Sizing Calculation Strategy", "Position Sizing Strategy")]
        public PositionSizingCalculationStrategy PositionSizingCalculationStrategy =
            PositionSizingCalculationStrategy.CalculatedBasedOnRiskAmount;

        [Parameter("Position Risk Amount Calculation Strategy", "Position Sizing Strategy")]
        public RiskAmountCalculationStrategy RiskAmountCalculationStrategy =
            RiskAmountCalculationStrategy.CalculatedBasedOnPortfolioAmount;

        [Parameter("Fixed Position Amount To Invest Per Position", "Position Sizing Strategy")]
        public double FixedAmountToInvestPerPosition;

        [Parameter("Fixed Position Amount To Risk Per Position", "Position Sizing Strategy")]
        public double FixedAmountToRiskPerPosition;

        #endregion


        #region Portfolio Management

        [Parameter("Grand Total Portfolio Amount", "Portfolio Management")]
        public double GrandTotalPortfolioAmount;

        [Parameter("Portfolio Allocation Percentage", "Portfolio Management")]
        public int PortfolioAllocationPercentage = 100;

        [Parameter("Number Of Portfolio Positions In Each Sub Portfolio", "Portfolio Management")]
        public int NumberOfPortfolioPositions = 5;

        [Parameter("Maximum Percentage Portfolio Risk In Each Sub Portfolio", "Portfolio Management")]
        public double MaxPortfolioRisk = 10;

        [Parameter("Maximum Position Risk", "Portfolio Management")]
        public double MaxPositionRisk = 2;

        [Parameter("Maximum Position Stop Percentage", "Portfolio Management")]
        public double StopPercentage = 10d;

        #endregion



        #region Position Sizing Management

        [Parameter("Risk Appetite Strategy", "Position Sizing Management")]
        public RiskAppetiteStrategy RiskAppetiteStrategy;


        #endregion


        #region Order Management

        [Parameter("Order Side", "Order Management")]
        public OrderSide OrderSide = OrderSide.Buy;


        #endregion
       
    }


}
