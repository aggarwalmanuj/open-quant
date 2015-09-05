using Spider.OpenQuant4.Base;

namespace Spider.OpenQuant4.Opening
{
    public abstract class BaseReverseOpeningStrategy : BaseQuantityFocusedStrategy
    {
        protected override void RunPostOrderStatusChangedImpl()
        {
            ReversalOpeningOrderStateDictionary.AddOrUpdate(this.Instrument.Symbol, IsStrategyOrderFilled, (s, b) => IsStrategyOrderFilled);
        }
    }
}