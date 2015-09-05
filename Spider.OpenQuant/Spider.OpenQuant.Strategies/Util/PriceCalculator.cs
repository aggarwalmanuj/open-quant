using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQuant.API;

namespace Spider.OpenQuant.Strategies.Util
{
    class PriceCalculator
    {
        private LoggingConfig logConfig = null;
        private enum GapType
        {
            Favorable,
            Unfavorable,
            Allowed
        }

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


        public double Calculate(PriceCalculatorInput input)
        {
            double price = 0;

            GapType gap = GetGapType(input);
            if (gap == GapType.Favorable)
                price = CalculateFavorableGapPrice(input);
            else if (gap == GapType.Unfavorable)
                price = CalculateUnfavorableGapPrice(input);
            else
                price = CalculateSlippageAdjustedPrice(input, input.AllowedSlippage);

            return price;
        }

        private GapType GetGapType(PriceCalculatorInput input)
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
                     effectiveFraction = absFraction*-1;
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
                     effectiveFraction = absFraction*-1;
                     if (absFraction >= input.UnfavorableGap)
                         returnValue = GapType.Unfavorable;
                 }
                 else if (fraction > 0)
                 {
                     effectiveFraction = absFraction;
                     if ( absFraction >= input.FavorableGap)
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
                    gap/input.PreviousBar.Close,
                    effectiveFraction,
                    input.Atr,
                    input.Atr/input.PreviousBar.Close,
                    returnValue));

            return returnValue;
        }


        public double CalculateStopSlippageAdjustedPrice(double amount, double atr, double allowedSlippage, PositionSide side)
        {
            double slippageAmount = atr * allowedSlippage;
            double returnValue = amount;

            if (side == PositionSide.Long)
            {
                returnValue = returnValue - slippageAmount;
            }
            else
            {
                returnValue = returnValue + slippageAmount;
            }

            LoggingUtility.WriteInfo(
                logConfig,
                string.Format(
                    "Allowed STOP slippage is {0}x ATR {1:c} ({2:p}) = {3:c} ({4:p}) on price of {5:c}, so slippage adjusted price is {6:c}",
                    allowedSlippage,
                    atr,
                    atr/amount,
                    slippageAmount,
                    slippageAmount/amount,
                    amount,
                    returnValue));

            return returnValue;
        }

        public double CalculateSlippageAdjustedPrice(PriceCalculatorInput input, double allowedSlippage)
        {
            double slippageAmount = input.Atr * allowedSlippage;
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
                    allowedSlippage,
                    input.Atr,
                    input.Atr/input.CurrentBar.Open,
                    slippageAmount,
                    slippageAmount / input.CurrentBar.Open,
                    input.CurrentBar.Open,
                    returnValue));

            return returnValue;
        }

        public double CalculateSlippageAdjustedPrice(PriceCalculatorInput input)
        {
            return CalculateSlippageAdjustedPrice(input, input.AllowedSlippage);
        }

        private double CalculateUnfavorableGapPrice(PriceCalculatorInput input)
        {
            return CalculateSlippageAdjustedPrice(input, input.UnfavorableGapAllowedSlippage);
        }

        private double CalculateFavorableGapPrice(PriceCalculatorInput input)
        {
            return CalculateSlippageAdjustedPrice(input, input.FavorableGapAllowedSlippage);
        }

    }

    public class PriceCalculatorInput
    {

        public Bar CurrentBar { get; set; }

        public Bar PreviousBar { get; set; }

        public double Atr { get; set; }

        public OrderSide OrderSide { get; set; }

        public double UnfavorableGapAllowedSlippage { get; set; }

        public double UnfavorableGap { get; set; }

        public double FavorableGapAllowedSlippage { get; set; }

        public double FavorableGap { get; set; }

        public double AllowedSlippage { get; set; }
    }
}
