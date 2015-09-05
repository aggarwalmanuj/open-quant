using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spider.Trading.OpenQuant3.Exceptions;
using Spider.Trading.OpenQuant3.Util;

namespace Spider.Trading.OpenQuant3.Calculations.StopPriceCalc
{
    public class FixedAmountStopCalculator : BaseStopPriceCalculator
    {
        public FixedAmountStopCalculator(BaseStrategy strategy)
            : base(strategy)
        {
        }

        public override object Calculate(object param)
        {
            ValidateStrategyType(Enums.StopPriceCalculationStrategy.FixedAmount);

            double? stopPrice = SolutionUtility.StopPriceHolder.GetStopPrice(Strategy.Instrument.Symbol);
            if (null == stopPrice)
            {
                throw new StrategyIncorrectInputException("StopPrice",
                                                          "Stop price was not supplied for the instrument");
            }
            else
            {

                SetIntradayStopPrice(stopPrice.Value,
                                     GetEffectiveHiPrice(),
                                     GetEffectiveLoPrice(), 0, 0);
            }

            return null;
        }

        public override bool NeedToUpdateCurrentDayHiLoPrice()
        {
            return false;
        }

        
    }
}
