using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider.Trading.OpenQuant3.Calculations.SlippageCalc
{
    public class SlippageCalculator : BaseStrategyCalculator
    {
        public SlippageCalculator(BaseStrategy strategy)
            : base(strategy)
        {
        }

        public override object Calculate(object param)
        {
            return 0;
        }
    }
}
