using System;
using OpenQuant.API;
using Spider.Trading.OpenQuant3.Diagnostics;
using Spider.Trading.OpenQuant3.Entities;
using Spider.Trading.OpenQuant3.Enums;

namespace Spider.Trading.OpenQuant3.Calculations
{
    public class PriceCalculator
    {
        private LoggingConfig logConfig = null;
       

        public PriceCalculator(LoggingConfig logConfig)
        {
            this.logConfig = logConfig;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public double RoundPrice(double input, Instrument instrument)
        {
            double step = GetPriceStep(input, instrument);
            return Math.Round(input / step) * step;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private double GetPriceStep(double input, Instrument instrument)
        {
            double retValue = 0.01;
            if (instrument.Type == InstrumentType.Futures)
            {
                if (string.Compare(instrument.Symbol, "ES", true) == 0)
                {
                    retValue = 0.25;
                }
            }
            else if (instrument.Type == InstrumentType.Stock)
            {
                if (input < 1)
                    retValue = 0.001;
            }

            return retValue;
        }


        public double CalculateSlippageAdjustedPrice(PriceCalculatorInput input)
        {
            double effectiveSlippage = input.Strategy.GetEffectiveAllowedSlippage(input.CurrentBar);
            double slippageAmount = input.Atr * effectiveSlippage;
            double returnValue = input.CurrentBar.Open;

            if (input.OrderSide == OrderSide.Buy)
            {
                returnValue = returnValue + slippageAmount;
            }
            else
            {
                returnValue = returnValue - slippageAmount;
            }

            LoggingUtility.WriteInfo(
                logConfig,
                string.Format(
                    "Allowed slippage is {0}x ATR {1:c} ({2:p}) = {3:c} ({4:p}) on price of {5:c}, so slippage adjusted price is {6:c}",
                    effectiveSlippage,
                    input.Atr,
                    input.Atr/input.CurrentBar.Open,
                    slippageAmount,
                    slippageAmount/input.CurrentBar.Open,
                    input.CurrentBar.Open,
                    returnValue));

            return RoundPrice(returnValue, input.Instrument);
        }

    }
}