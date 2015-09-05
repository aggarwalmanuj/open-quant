using OpenQuant.API;

using Spider.OpenQuant.TradeClient5.Base;


namespace Spider.OpenQuant.TradeClient5.Opening
{
    public partial class BaseOpeningStrategy : BaseStrategy
    {




        #region Portfolio Management

        [Parameter("Grand Total Portfolio Amount", "Portfolio Management")] public double TotalPortfolioAmount = 10000;


        [Parameter("Number Of Portfolio Positions In Each Sub Portfolio", "Portfolio Management")] 
        public int NumberOfPortfolioPositions = 5;



        #endregion





        #region Order Management

        [Parameter("Opening Order Side", "Order Management")] public OrderSide OpeningOrderSide = OrderSide.Buy;


        #endregion

    }


}
