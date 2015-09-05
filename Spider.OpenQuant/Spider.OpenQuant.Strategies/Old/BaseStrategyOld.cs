using System;
using System.Collections.Generic;
using OpenQuant.API;

namespace Spider.OpenQuant.Strategies
{
    public abstract class BaseStrategyOld : Strategy
    {
        [Parameter("Previous Bar Slippage", "Other Options")]
        public double PreviousBarSlippagePercentage = 0.5;

        [Parameter("Next Bar Slippage", "Other Options")]
        public double NextBarSlippagePercentage = 0;

        [Parameter("Log output", "Other Options")]
        public bool LogOutput = false;

        [Parameter("Auto Submit", "Other Options")]
        public bool AutoSubmit = true;


        [Parameter("Max Gap Percent", "Money Management")]
        public double MaxGapPercentage = 5;


        [Parameter("Portfolio Amount", "Money Management")]
        public double PortfolioAmount = 10000;

        [Parameter("Portfolio Positions", "Money Management")]
        public int NumberOfPositions = 5;

        [Parameter("Portfolio Risk", "Money Management")]
        public double PortfolioRisk = 6;

        [Parameter("Position Risk", "Money Management")]
        public double PositionRisk = 2;

        [Parameter("Stop", "Money Management")]
        public double StopPercentage = 5;


        [Parameter("Time", "Trigger Time")]
        public DateTime TiggerOrderDate = DateTime.Today;

        [Parameter("Trigger Hour", "Trigger Time")]
        public int TriggerOrderHour = 6;

        [Parameter("Trigger Minute", "Trigger Time")]
        public int TriggerOrderMinute = 30;

        private Dictionary<string, double> StopPrices = new Dictionary<string, double>(StringComparer.InvariantCultureIgnoreCase);

        private static readonly object LockObject = new object();

        protected void AddStopPrice(string symbol, double price)
        {
            lock (LockObject)
            {
                StopPrices[symbol] = price;
            }
        }


        protected void ClearStopPrices()
        {
            lock (LockObject)
            {
                StopPrices.Clear();
            }
        }

        protected double? GetStopPrice(string symbol)
        {
            if (StopPrices.ContainsKey(symbol))
                return StopPrices[symbol];
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instrument"></param>
        protected void PreLoadBarData(Instrument instrument)
        {
            BarSeries historicalBars = GetHistoricalBars("IB", instrument, DateTime.Now.AddDays(-5), DateTime.Now, 60);
            foreach (Bar currentBar in historicalBars)
            {
                Bars.Add(currentBar);
                DataManager.Add(Instrument, currentBar);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="bar"></param>
        /// <param name="orderType"></param>
        /// <returns></returns>
        protected double GetTargetPrice(Bar bar, OrderSide orderType)
        {
            double openPrice = 0;
            double? previousClosePrice = null;

            openPrice = bar.Open;

            Bar previousBar = GetPreviousBar(bar);

            if (null != previousBar)
            {
                previousClosePrice = previousBar.Close;
                if (LogOutput && previousClosePrice.HasValue)
                    Console.WriteLine(string.Format("Previous close for {0} at {1} was @ {2}", Instrument.Symbol, previousBar.BeginTime, previousBar.Close));
            }

            double targetPrice = openPrice;
            bool didPriceGapMoreThanAllowedThreshold = false;
            if (previousClosePrice.HasValue && null != previousBar)
            {
                // First test whether the current open price is within slippage allowance
                bool isWithinSlippageAllowance = IsOpeningPriceWithinSlippageAllowance(openPrice,
                                                                                       previousClosePrice.Value,
                                                                                       orderType);

                if (isWithinSlippageAllowance)
                    targetPrice = openPrice;
                else if (previousBar.BeginTime != bar.BeginTime)
                    targetPrice = GetGapAdjustedPrice(openPrice, previousClosePrice.Value, orderType, ref didPriceGapMoreThanAllowedThreshold);

            }

            if (!didPriceGapMoreThanAllowedThreshold)
                targetPrice = GetSlippageAdjustedPrice(targetPrice, orderType);

            return targetPrice;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetPrice"></param>
        /// <returns></returns>
        protected double GetTargetQuantity(double targetPrice, bool roundLots)
        {
            double maxPortfolioRiskAmount = PortfolioAmount * PortfolioRisk / 100;
            double maxPortfolioRiskAmountPerPosition = maxPortfolioRiskAmount / NumberOfPositions;

            double maxPositionRiskAmount = PortfolioAmount * PositionRisk / 100;

            maxPositionRiskAmount = Math.Min(maxPortfolioRiskAmountPerPosition, maxPositionRiskAmount);

            if (LogOutput)
                Console.WriteLine(string.Format("Max risk amount per position: {0:C}", maxPositionRiskAmount));

            double stopPrice = targetPrice * StopPercentage / 100;
            double quantity = maxPositionRiskAmount / stopPrice;
            double maxPositionAmount = PortfolioAmount / NumberOfPositions;
            double calcPositionAmount = targetPrice * quantity;

            if (calcPositionAmount > maxPositionAmount)
            {
                quantity = maxPositionAmount / targetPrice;
            }

            quantity = Math.Round(quantity, 0);
            if (roundLots)
                quantity = quantity - (quantity % 100);

            return quantity;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="price"></param>
        /// <param name="orderType"></param>
        /// <returns></returns>
        protected double GetSlippageAdjustedPrice(double price, OrderSide orderType)
        {
            double slippageAmount = price * NextBarSlippagePercentage / 100;
            double targetPrice = price;

            if (orderType == OrderSide.Buy)
            {
                targetPrice = targetPrice + slippageAmount;
            }
            else
            {
                targetPrice = targetPrice - slippageAmount;
            }

            return targetPrice;
        }

        protected bool IsExitStopPriceMet(double lastPrice, double stopPrice, PositionSide positionSide)
        {
            bool stopMet = false;
            if (positionSide == PositionSide.Short)
                stopMet = lastPrice >= stopPrice;
            else
                stopMet = lastPrice <= stopPrice;
            return stopMet;
        }

        protected bool IsEntryStopPriceMet(double lastPrice, double stopPrice, OrderSide OrderType)
        {
            bool stopMet = false;
            if (OrderType == OrderSide.Buy)
                stopMet = lastPrice >= stopPrice;
            else
                stopMet = lastPrice <= stopPrice;
            return stopMet;
        }


        protected void LogOrder(string orderStyle, string symbol, OrderSide orderType, double qty, double price)
        {
            if (LogOutput)
            {
                Console.WriteLine();
                Console.WriteLine("---------------------------");
                Console.WriteLine(string.Format("ORDER [{0}]: {1} order for {2}: {3} {4} @ {5} for {6:C}", 
                                                DateTime.Now,
                                                orderStyle, symbol,
                                                orderType, qty, price, qty*price));
                Console.WriteLine("---------------------------");
            }
        }


        protected void OutputLog(string message)
        {
            if (LogOutput)
            {
                Console.WriteLine(string.Format("INFO [{0}]: {1}", DateTime.Now, message));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="openPrice"></param>
        /// <param name="previousClose"></param>
        /// <param name="orderType"></param>
        /// <returns></returns>
        private double GetGapAdjustedPrice(double openPrice, double previousClose, OrderSide orderType, ref bool didPriceGapMoreThanAllowedThreshold)
        {
            bool needToCheckForGap = false;
            double gapAmount = 0;
            double gapPercent = 0;
            double midPrice = openPrice;
            double targetPrice = openPrice;

            if (orderType == OrderSide.Buy)
            {
                needToCheckForGap = previousClose < openPrice;
                gapAmount = openPrice - previousClose;
                gapPercent = gapAmount/previousClose*100;
                midPrice = previousClose + (gapAmount/2);
            }
            else
            {
                needToCheckForGap = previousClose > openPrice;
                gapAmount = previousClose - openPrice;
                gapPercent = gapAmount / previousClose * 100;
                midPrice = previousClose - (gapAmount / 2);
            }

            if (LogOutput && needToCheckForGap)
                Console.WriteLine(string.Format("Previous close for {0} gapped {1:C} ({2:P})", Instrument.Symbol, gapAmount, gapPercent / 100));

            didPriceGapMoreThanAllowedThreshold = false;
            if (needToCheckForGap)
            {
                if (gapPercent > MaxGapPercentage)
                {
                    targetPrice = midPrice;
                    didPriceGapMoreThanAllowedThreshold = true;
                }
            }

            return targetPrice;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="openPrice"></param>
        /// <param name="previousClose"></param>
        /// <param name="orderType"></param>
        /// <returns></returns>
        private bool IsOpeningPriceWithinSlippageAllowance(double openPrice, double previousClose, OrderSide orderType)
        {
            double slippageAmount = previousClose * PreviousBarSlippagePercentage / 100;
            double targetPrice = previousClose;

            if (orderType == OrderSide.Buy)
            {
                targetPrice = targetPrice + slippageAmount;
                return openPrice <= targetPrice;
            }
            else
            {
                targetPrice = targetPrice - slippageAmount;
                return openPrice >= targetPrice;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="bar"></param>
        /// <returns></returns>
        private Bar GetPreviousBar(Bar bar)
        {
            Bar retVal = null;
            if (Bars.Count > 1)
            {
                int idx = -1;
                try
                {
                    idx = Bars.GetIndex(bar.BeginTime);
                }
                catch { }
                idx--;
                if (idx >= 0 && idx <= Bars.Count - 1)
                {
                    retVal = Bars[idx];
                }
            }
            return retVal;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        protected double RoundPrice(double input)
        {
            double step = GetStep(input);
            return Math.Round(input / step) * step;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private double GetStep(double input)
        {
            double retValue = 0.01;
            if (Instrument.Type == InstrumentType.Futures)
            {
                if (string.Compare(Instrument.Symbol, "ES", true) == 0)
                {
                    retValue = 0.25;
                }
            }
            else if (Instrument.Type == InstrumentType.Stock)
            {
                if (input < 1)
                    retValue = 0.001;
            }

            return retValue;
        }



    }
}