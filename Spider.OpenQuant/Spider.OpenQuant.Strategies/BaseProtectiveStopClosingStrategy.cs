using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQuant.API;
using Spider.OpenQuant.Strategies.Util;

namespace Spider.OpenQuant.Strategies
{
    public class BaseProtectiveStopClosingStrategy : BaseStopClosingStrategy
    {
        #region Money Management

        [Parameter("Stop Slippage", "Money Management")]
        public double StopSlippage = 0.2;

        #endregion


        protected override bool IsItOkToQueueOrder()
        {
            Bar bar = GetPreviousDayBar(Instrument);

            PositionSide side = GetOpenPositionSide();

            double stopThresholdAmount = 0;
            if (side == PositionSide.Long)
                stopThresholdAmount = bar.Low;
            else
                stopThresholdAmount = bar.High;


            stopPrice = new PriceCalculator(LoggingConfig).CalculateStopSlippageAdjustedPrice(stopThresholdAmount,
                                                                                              GetAtrValue(Instrument, AtrPeriod, triggerTime.Value),
                                                                                              StopSlippage, side);
            bool stopPriceOk = stopPrice.HasValue;

            if (!stopPriceOk)
                LoggingUtility.LogNoStopOrderFound(LoggingConfig);

            return stopPriceOk && IsClosingQuantityValid();
        }
    }
}
