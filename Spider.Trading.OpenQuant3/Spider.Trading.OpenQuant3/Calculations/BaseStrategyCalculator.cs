using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spider.Trading.OpenQuant3.Diagnostics;
using Spider.Trading.OpenQuant3.Enums;

namespace Spider.Trading.OpenQuant3.Calculations
{
    

    public abstract class BaseStrategyCalculator : IStrategyCalculator
    {
        private BaseStrategy baseStrat = null;

        public BaseStrategyCalculator(BaseStrategy strategy)
        {
            this.baseStrat = strategy;
        }

        protected BaseStrategy Strategy
        {
            get { return baseStrat; }
        }

        public abstract object Calculate(object param);
    }
}
