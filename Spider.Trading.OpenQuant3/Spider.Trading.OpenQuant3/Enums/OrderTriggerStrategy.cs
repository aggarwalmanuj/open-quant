using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider.Trading.OpenQuant3.Enums
{
    public enum OrderTriggerStrategy
    {
        TimerBasedTrigger,
        StopPriceBasedTrigger,
        DailyLossPercentageTrigger,
        PortfolioLossPercentageTrigger,
        IwmStopPriceBasedTrigger
    }
}
