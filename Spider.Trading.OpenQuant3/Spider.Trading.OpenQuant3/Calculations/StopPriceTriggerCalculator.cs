using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQuant.API;
using Spider.Trading.OpenQuant3.Diagnostics;
using Spider.Trading.OpenQuant3.Enums;

namespace Spider.Trading.OpenQuant3.Calculations
{
    public class StopPriceTriggerCalculator
    {
        private LoggingConfig logConfig = null;

        public StopPriceTriggerCalculator(LoggingConfig logConfig)
        {
            this.logConfig = logConfig;
        }



        internal bool IsStopPriceMet(double lastPrice, double stopPrice, OrderSide orderSide, StopPriceCalculationStrategy stopStrategy, string info)
        {
            bool stopMet = false;


            if (lastPrice <= 0)
            {
                LoggingUtility.WriteWarn(logConfig,
                                         string.Format("Cannot calculate stop price condition for LAST price {0:c}",
                                                       lastPrice));
                return stopMet;
            }

            if (stopPrice <= 0)
            {
                LoggingUtility.WriteVerbose(logConfig,
                                         string.Format("Cannot calculate stop price condition for STOP price {0:c}",
                                                       stopPrice));
                return stopMet;
            }



            if (orderSide == OrderSide.Buy)
                stopMet = lastPrice > stopPrice;
            else
                stopMet = lastPrice < stopPrice;


            if (stopMet)
                LoggingUtility.WriteInfo(
                    logConfig,
                    string.Format(
                        "[{4}] Stop price of {0:c} was met on {1} side by last close price of {2:c} based on {3} strategy",
                        stopPrice,
                        orderSide,
                        lastPrice,
                        stopStrategy,
                        info));

            return stopMet;
        }

    }
}
