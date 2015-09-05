using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQuant.API;
using Spider.OpenQuant3_2.Base;

namespace Spider.OpenQuant3_2.Opening
{
    public abstract class BaseReverseOpeningStrategy  : BaseQuantityFocusedStrategy
    {
        protected override void RunPostOrderStatusChangedImpl()
        {
            ReversalOpeningOrderStateDictionary.AddOrUpdate(this.Instrument.Symbol, IsStrategyOrderFilled, (s, b) => IsStrategyOrderFilled);
        }
    }
}
