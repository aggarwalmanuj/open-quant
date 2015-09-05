using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQuant.API;

namespace Spider.Trading.OpenQuant3.Events
{
    public delegate void StrategyEventHandler(Bar bar);

    public delegate void StrategyRetryOrderAttemptTriggered(Bar bar, int tryCount, bool trialExhausted);
}
