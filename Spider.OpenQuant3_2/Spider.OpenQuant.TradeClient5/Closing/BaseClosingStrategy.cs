using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenQuant.API;

using Spider.OpenQuant.TradeClient5.Base;
using Spider.OpenQuant.TradeClient5.Entities;
using Spider.OpenQuant.TradeClient5.Scaling;
using Spider.OpenQuant.TradeClient5.Util;

namespace Spider.OpenQuant.TradeClient5.Closing
{
    public abstract class BaseClosingStrategy : BaseQuantityFocusedStrategy
    {
        protected SpiderPosition SpiderPosition { get; set; }

        public override void SetupTradeLegs()
        {
            LookupOpenPosition();

            AddFirstClosingLeg();
        }

        protected void AddFirstClosingLeg()
        {
            var tradeLeg = new QuantityTradeLeg(
                1,
                this.Instrument,
                GetOrderSide(),
                this.SpiderPosition.Quantity
                );

            AddLeg(tradeLeg);
        }

        protected void LookupOpenPosition()
        {
            this.SpiderPosition = base.GetOpenPositionFromBroker();
        }


        protected OrderSide GetOrderSide()
        {
            if (SpiderPosition == null)
            {
                throw new InvalidOperationException(
                    string.Format("Could not retrieve an open position for {0} in account {1}", this.Instrument.Symbol,
                        this.IbAccountNumber));
            }

            if (SpiderPosition.PositionSide == PositionSide.Long)
            {
                return OrderSide.Sell;
            }
            else if (SpiderPosition.PositionSide == PositionSide.Short)
            {
                return OrderSide.Buy;
            }
            else
            {
                throw new InvalidOperationException(
                    string.Format("Could not retrieve an open position for {0} in account {1}", this.Instrument.Symbol,
                        this.IbAccountNumber));
            }

        }
    }
}
