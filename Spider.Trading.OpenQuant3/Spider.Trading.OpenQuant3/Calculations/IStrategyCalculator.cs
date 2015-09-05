using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider.Trading.OpenQuant3.Calculations
{
    public interface IStrategyCalculator
    {
        object Calculate(object param);
    }
}
