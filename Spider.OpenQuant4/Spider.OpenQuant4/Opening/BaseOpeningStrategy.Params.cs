using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQuant.API;
using Spider.OpenQuant4.Base;

namespace Spider.OpenQuant4.Opening
{
    public partial class BaseOpeningStrategy : BaseStrategy
    {

       


        #region Portfolio Management

        [Parameter("Grand Total Portfolio Amount", "Portfolio Management")] public double TotalPortfolioAmount = 10000;

      
       [Parameter("Number Of Portfolio Positions In Each Sub Portfolio", "Portfolio Management")]
        public int NumberOfPortfolioPositions = 5;

 

        #endregion





        #region Order Management

        [Parameter("Opening Order Side", "Order Management")]
        public OrderSide OpeningOrderSide = OrderSide.Buy;


        #endregion

    }


}
