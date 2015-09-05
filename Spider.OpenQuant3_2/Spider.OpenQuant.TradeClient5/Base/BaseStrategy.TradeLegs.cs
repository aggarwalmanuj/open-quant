using System.Collections.Generic;
using System.Linq;

using OpenQuant.API;

using Spider.OpenQuant.TradeClient5.Scaling;
using Spider.OpenQuant.TradeClient5.Util;

namespace Spider.OpenQuant.TradeClient5.Base
{
    public abstract partial class BaseStrategy
    {
        protected List<BaseTradeLeg> TradeLegs = new List<BaseTradeLeg>();

        protected bool AreAllLegsCompleted = true;

        public abstract void SetupTradeLegs();

        protected BaseTradeLeg CurrentTradeLeg { get; set; }

        protected void AddLeg(BaseTradeLeg tradeLeg)
        {
            TradeLegs.Add(tradeLeg);

            LoggingUtility.WriteInfoFormat(this, "Added leg: {0}", tradeLeg.LegName);
        }

        protected void UpdateTradeLegsCompletedFlag()
        {

            if (TradeLegs.Count <= 0)
            {
                LoggingUtility.WriteDebug(this, "No legs to process");
                AreAllLegsCompleted = true;
            }
            else
            {
                AreAllLegsCompleted = TradeLegs.All(a => a.IsLegComplete);

                if (AreAllLegsCompleted)
                {
                    LoggingUtility.WriteDebug(this, "All legs are complete");
                }
            }
        }

        protected void UpdateNextTradeLeg()
        {
            CurrentTradeLeg = TradeLegs.FirstOrDefault(a => !a.IsLegComplete);

            if (CurrentTradeLeg != null)
            {
                LoggingUtility.WriteInfoFormat(this, "Set CURRENT LEG: {0}", CurrentTradeLeg.LegName);
            }
            else if (!AreAllLegsCompleted)
            {
                LoggingUtility.WriteInfo(this, "CURRENT LEG IS NULL");
            }
        }
    }
}
