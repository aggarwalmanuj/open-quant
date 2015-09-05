using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenQuant.API;

using Spider.OpenQuant.TradeClient5.Base;
using Spider.OpenQuant.TradeClient5.Scaling;
using Spider.OpenQuant.TradeClient5.Util;

namespace Spider.OpenQuant.TradeClient5.Opening
{
    public abstract partial class BaseOpeningStrategy : BaseStrategy
    {
        public override void SetupTradeLegs()
        {
            var totalAmount = ((TotalPortfolioAmount/NumberOfPortfolioPositions)*PositionSizePercentage/100d);

            LoggingUtility.WriteDebug(this,
                string.Format("*** Calculated OPENING position size amount={0:c} to {1} for {2} in account {3} ***",
                    totalAmount,
                    OpeningOrderSide.ToString().ToUpper(),
                    this.Instrument.Symbol.ToUpper(),
                    this.IbAccountNumber));

            var tradeLeg = new AmountTradeLeg(
                1,
                this.Instrument,
                OpeningOrderSide,
                totalAmount
                );

            AddLeg(tradeLeg);
        }


    }
}
