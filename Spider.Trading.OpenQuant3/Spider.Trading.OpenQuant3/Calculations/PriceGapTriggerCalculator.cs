using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQuant.API;
using Spider.Trading.OpenQuant3.Diagnostics;
using Spider.Trading.OpenQuant3.Entities;
using Spider.Trading.OpenQuant3.Enums;

namespace Spider.Trading.OpenQuant3.Calculations
{


    public class PriceGapTriggerCalculator
    {
        private LoggingConfig logConfig = null;


        public PriceGapTriggerCalculator(LoggingConfig logConfig)
        {
            this.logConfig = logConfig;
        }

        



        public GapType GetGapType(PriceGapCalculatorInput input)
        {
            double gap = input.CurrentBar.Open - input.PreviousBar.Close;


            GapType returnValue = GapType.Allowed;

            double fraction = 0;
            double effectiveFraction = 0;

            if (gap != 0 && input.Atr > 0)
                fraction = gap / input.Atr;
            double absFraction = Math.Abs(fraction);


            if (input.OrderSide == OrderSide.Buy)
            {

                if (fraction > 0)
                {
                    effectiveFraction = absFraction * -1;
                    if (absFraction >= input.UnfavorableGap)
                        returnValue = GapType.Unfavorable;
                }
                else if (fraction < 0)
                {
                    effectiveFraction = absFraction;
                    if (absFraction >= input.FavorableGap)
                        returnValue = GapType.Favorable;
                }
                else
                    effectiveFraction = 0;

            }
            else
            {
                if (fraction < 0 && absFraction >= input.UnfavorableGap)
                    returnValue = GapType.Unfavorable;
                else if (fraction > 0 && absFraction >= input.FavorableGap)
                    returnValue = GapType.Favorable;

                if (fraction < 0)
                {
                    effectiveFraction = absFraction * -1;
                    if (absFraction >= input.UnfavorableGap)
                        returnValue = GapType.Unfavorable;
                }
                else if (fraction > 0)
                {
                    effectiveFraction = absFraction;
                    if (absFraction >= input.FavorableGap)
                        returnValue = GapType.Favorable;
                }
                else
                    effectiveFraction = 0;
            }



            LoggingUtility.WriteInfo(
                logConfig,
                string.Format(
                    "For a {0} order, prev close was {1:c}, new open is {2:c}, a gap of {3:c} ({4:p}) ({5:n5}x ATR value {6:c} ({7:p})). [{8}]",
                    input.OrderSide,
                    input.PreviousBar.Close,
                    input.CurrentBar.Open,
                    gap,
                    gap / input.PreviousBar.Close,
                    effectiveFraction,
                    input.Atr,
                    input.Atr / input.PreviousBar.Close,
                    returnValue));

            return returnValue;
        }



    }
}
