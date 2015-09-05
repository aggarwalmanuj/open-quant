using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQuant.API;
using Spider.OpenQuant3_2.Base;
using Spider.OpenQuant3_2.Util;

namespace Spider.OpenQuant3_2.Closing
{
    public abstract class BaseReverseClosingStrategy : BaseQuantityFocusedStrategy
    {
        public override OpenQuant.API.OrderSide GetOrderSide()
        {
            if (InitialOpenQuantity == null || OpenPositionSide == null)
            {
                throw new InvalidOperationException(
                    string.Format("Could not retrieve an open position for {0} in account {1}", this.Instrument.Symbol,
                        this.IbAccountNumber));
            }

            if (OpenPositionSide.Value == PositionSide.Long)
            {
                return OrderSide.Buy;
            }
            else if (OpenPositionSide.Value == PositionSide.Short)
            {
                return OrderSide.Sell;
            }
            else
            {
                throw new InvalidOperationException(
                    string.Format("Could not retrieve an open position for {0} in account {1}", this.Instrument.Symbol,
                        this.IbAccountNumber));
            }
        }

        protected override bool IsParentOrderFilled()
        {
            bool isParentOrderFilled = false;

            if (ReversalOpeningOrderStateDictionary.TryGetValue(this.Instrument.Symbol, out isParentOrderFilled))
            {
                LoggingUtility.WriteInfoFormat(this, "Parent order status: {0}", isParentOrderFilled.ToString().ToUpper());
            }
            else
            {
                LoggingUtility.WriteInfo(this, "*****  COULD NOT RETRIEVE THE STATUS OF THE PARENT ORDER  *****");
            }
            return isParentOrderFilled;
        }

        protected override void RunPostOrderStatusChangedImpl()
        {
            if (IsStrategyOrderFilled)
            {
                ReversalOpeningOrderStateDictionary.AddOrUpdate(this.Instrument.Symbol, IsStrategyOrderFilled,
                    (s, b) => false);
            }
        }
    }
}
