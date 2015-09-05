using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spider.Trading.OpenQuant3.Calculations;
using Spider.Trading.OpenQuant3.Calculations.StopPriceCalc;
using Spider.Trading.OpenQuant3.Enums;

namespace Spider.Trading.OpenQuant3.Factories
{
    public static class StopPriceCalculatorFactory
    {
        public static BaseStopPriceCalculator GetCalculator(BaseStrategy strategy)
        {
            BaseStopPriceCalculator returnVal = null;


            switch (strategy.StopCalculationStrategy)
            {
                case StopPriceCalculationStrategy.FixedAmount:
                    returnVal = new FixedAmountStopCalculator(strategy);
                    break;
                case StopPriceCalculationStrategy.ProtectiveStopBasedOnAtr:
                    returnVal = new ProtectiveStopBasedOnAtrCalculator(strategy);
                    break;
                case StopPriceCalculationStrategy.RetracementEntryBasedOnAtr:
                    returnVal = new RetracementEntryBasedOnAtrCalculator(strategy);
                    break;
                case StopPriceCalculationStrategy.OpeningGap:
                    returnVal = new OpeningGapStopCalculator(strategy);
                    break;
                case StopPriceCalculationStrategy.OpeningGapOrProtectiveStop:
                    returnVal = new OpeningGapOrProtectiveStopCalculator(strategy);
                    break;
                case StopPriceCalculationStrategy.OpeningGapOrRetracementEntry:
                    returnVal = new OpeningGapOrRetracementEntryStopCalculator(strategy);
                    break;
                case StopPriceCalculationStrategy.OpeningGapAndProtectiveStop:
                    returnVal = new OpeningGapOrProtectiveStopCalculator(strategy);
                    break;
                case StopPriceCalculationStrategy.OpeningGapAndRetracementEntry:
                    returnVal = new OpeningGapOrRetracementEntryStopCalculator(strategy);
                    break;
                case StopPriceCalculationStrategy.AbIfStopBasedOnAtr:
                    returnVal = new OpeningGapOrProtectiveStopCalculator(strategy);
                    break;
                default:
                    throw new NotImplementedException();
            }



            return returnVal;
        }
    }
}
