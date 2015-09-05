using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using OpenQuant.API;

using Spider.OpenQuant.TradeClient5.Util;

namespace Spider.OpenQuant.TradeClient5.Base
{

    public abstract partial class BaseStrategy
    {
        protected DateTime LastSpreadAdjustedAt = DateTime.MinValue;


        protected const double MinSpreadInCents = 0.01;

        protected double OriginalMaxSpreadFractionToAllocateForPricing { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isBullishEvent"></param>
        /// <param name="isBearishEvent"></param>
        /// <param name="factor"></param>
        private void AdjustSpreadSteppingBasedOnIndicatorEvent(bool isBullishEvent, bool isBearishEvent, int factor, string eventName)
        {
            if (IsCurrentOrderActive())
            {
                OrderSide orderSide = CurrentTradeLeg.OrderSide;
                factor = Math.Abs(factor);

                if (isBullishEvent)
                {
                    if (orderSide == OrderSide.Sell)
                    {
                        LoggingUtility.WriteDebugFormat(this, "Increasing the spread to recover by factor of {0} due to a {1} event on a {2} order. {3}", factor, "Bullish", orderSide, eventName.ToUpper());

                        AdjustSpreadStepping(factor);
                    }

                    if (orderSide == OrderSide.Buy)
                    {
                        LoggingUtility.WriteDebugFormat(this, "Decreasing the spread to recover by factor of {0} due to a {1} event on a {2} order. {3}", factor, "Bullish", orderSide, eventName.ToUpper());

                        AdjustSpreadStepping(factor * -1);
                    }
                }

                if (isBearishEvent)
                {
                    if (orderSide == OrderSide.Sell)
                    {
                        LoggingUtility.WriteDebugFormat(this, "Decreasing the spread to recover by factor of {0} due to a {1} event on a {2} order. {3}", factor, "Bearish", orderSide, eventName.ToUpper());

                        AdjustSpreadStepping(factor * -1);
                    }

                    if (orderSide == OrderSide.Buy)
                    {
                        LoggingUtility.WriteDebugFormat(this, "Increasing the spread to recover by factor of {0} due to a {1} event on a {2} order. {3}", factor, "Bearish", orderSide, eventName.ToUpper());

                        AdjustSpreadStepping(factor);
                    }
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool IsSpreadTolerable()
        {

            if (MinAllowedSpreadToAtrFraction > MaxAllowedSpreadToAtrFraction)
                throw new ArithmeticException(
                    string.Format("Min Allowed spread {0} cannot be more than Max Allowed spread {1}",
                        MinAllowedSpreadToAtrFraction, MaxAllowedSpreadToAtrFraction));

            bool returnValue = false;
            string logMessage = "";
            if (CurrentAtrPrice > 0 &&
                CurrentAskPrice > 0 &&
                CurrentBidPrice > 0)
            {
                double currentSpread = Math.Abs(CurrentBidPrice - CurrentAskPrice);
                if (currentSpread <= MinSpreadInCents)
                {
                    logMessage = string.Format(
                        "Spread is {0:c} which is minimum possible. Spread eval=TRUE.",
                        currentSpread);
                    returnValue = true;
                }
                else
                {
                    double remainingSessionPart = (1 - GetRemainderSessionTimeFraction());
                    double differenceInAllowedSpread = MaxAllowedSpreadToAtrFraction - MinAllowedSpreadToAtrFraction;
                    double maxtolerableSpreadInAtr = MinAllowedSpreadToAtrFraction +
                                                     (differenceInAllowedSpread*remainingSessionPart);

                    double currentSpreadInAtr = currentSpread/CurrentAtrPrice;

                    bool isCurrentSpreadOk = currentSpreadInAtr <= maxtolerableSpreadInAtr;

                    string currentBidAskMessage = string.Format("Bid={0:c},Ask={1:c},Atr={2:c}",
                        CurrentBidPrice, CurrentAskPrice, CurrentAtrPrice);

                    string currentSpreadMessage = string.Format("Spread is {0:c} ({1:p} of ATR {2:c})",
                        currentSpread, currentSpreadInAtr, CurrentAtrPrice);
                    string allowedSpreadMessage = string.Format("Allowed is {0:c} ({1:p} of ATR {2:c})",
                        maxtolerableSpreadInAtr*CurrentAtrPrice, maxtolerableSpreadInAtr, CurrentAtrPrice);

                    logMessage = string.Format(
                        "{0}. {1}. {2}. Session Passed: {3:F4}. Spread eval={4}.",
                        currentBidAskMessage,
                        currentSpreadMessage,
                        allowedSpreadMessage,
                        remainingSessionPart,
                        isCurrentSpreadOk.ToString().ToUpper());

                    returnValue = isCurrentSpreadOk;
                }
            }
            else
            {
                logMessage = "Not enough data to evaluate spread suitability";
            }


            WriteInfrequentDebugMessage(logMessage);

            return returnValue;
        }

        private double GetAllowededSlippageAmount()
        {
            if (MinAllowedSlippage > MaxAllowedSlippage)
                throw new ArithmeticException(
                    string.Format("Min Allowed Slippage {0} cannot be more than Max Allowed Slippage {1}",
                        MinAllowedSlippage, MaxAllowedSlippage));

            if (CurrentAtrPrice <= 0)
                throw new ArgumentOutOfRangeException("CurrentAtrPrice", "Atr price must be a positive number");
            double remainingSessionPart = (1 - GetRemainderSessionTimeFraction());
            double totalSlippageGapAllowedToBeConsumedInTheSession =
                GetTotalSlippageGapAllowedToBeConsumedInTheSession();
            double slippageLeftToBeConsumed = totalSlippageGapAllowedToBeConsumedInTheSession *
                                              GetRemainderSessionTimeFraction();
            double allowedSlippageFraction = MaxAllowedSlippage - slippageLeftToBeConsumed;
            double slippageAllowedAmount = allowedSlippageFraction * CurrentAtrPrice;

            string allowedSlippageMessage =
                string.Format("Slippage Fraction: Max={0:F4}, Min={1:F4}, Diff={2:F4}, Remaining={3:F4}",
                    MaxAllowedSlippage,
                    MinAllowedSlippage,
                    totalSlippageGapAllowedToBeConsumedInTheSession,
                    allowedSlippageFraction);

            LoggingUtility.WriteTraceFormat(this,
                "{0}. Session Passed: {1:F4}. So slippage amount to recover {2:c5}.",
                allowedSlippageMessage,
                remainingSessionPart,
                slippageAllowedAmount);



            return slippageAllowedAmount;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private double GetAllowededSpreadAmount()
        {
            double allowedSpreadAmount = 0;

            string logMessage = string.Empty;

            if (CurrentAtrPrice > 0 &&
                CurrentAskPrice > 0 &&
                CurrentBidPrice > 0)
            {
                double currentSpread = Math.Abs(CurrentBidPrice - CurrentAskPrice);
                if (currentSpread > MinSpreadInCents)
                {
                    double remainingSessionPart = (1 - GetRemainderSessionTimeFraction());
                    if (MaxSpreadFractionToAllocateForPricing > MinSpreadFractionToAllocateForPricing)
                    {
                        double differenceInAllowedSpread = MaxSpreadFractionToAllocateForPricing -
                                                           MinSpreadFractionToAllocateForPricing;
                        double maxtolerableSpreadFraction = MaxSpreadFractionToAllocateForPricing -
                                                            (differenceInAllowedSpread*remainingSessionPart);
                        allowedSpreadAmount = maxtolerableSpreadFraction*currentSpread;

                        string allowedSpreadMessage =
                            string.Format("Spread Fraction: Max={0:F4}, Min={1:F4}, Diff={2:F4}, Remaining={3:F4}",
                                MaxSpreadFractionToAllocateForPricing,
                                MinSpreadFractionToAllocateForPricing,
                                differenceInAllowedSpread,
                                maxtolerableSpreadFraction);

                        logMessage = string.Format(
                            "{0}. Session Passed: {1:F4}. So Spread amount to recover {2:c5}.",
                            allowedSpreadMessage,
                            remainingSessionPart,
                            allowedSpreadAmount);

                    }
                    else
                    {
                        allowedSpreadAmount = MinSpreadFractionToAllocateForPricing*currentSpread;
                        string allowedSpreadMessage =
                            string.Format("Spread Fraction: Max={0:F4}, Min={1:F4}. Using min allowed",
                                MaxSpreadFractionToAllocateForPricing,
                                MinSpreadFractionToAllocateForPricing);

                        logMessage = string.Format(
                            "{0}. Session Passed: {1:F4}. So Spread amount to recover {2:c5}.",
                            allowedSpreadMessage,
                            remainingSessionPart,
                            allowedSpreadAmount);
                    }
                }
                else
                {
                    allowedSpreadAmount = currentSpread;
                }
            }
            else
            {
                throw new  ArgumentException("Cannot calculate spread because bid, ask or atr is missing");
            }

            WriteInfrequentDebugMessage(logMessage);

            return allowedSpreadAmount;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private double GetTotalSlippageGapAllowedToBeConsumedInTheSession()
        {
            return MaxAllowedSlippage - MinAllowedSlippage;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fractionMultiplier"></param>
        private void AdjustSpreadStepping(int fractionMultiplier)
        {
            if (GetCurrentDateTime() < CurrentValidityDateTime)
                return;

            if (!IsCurrentOrderActive())
                return;

            if (GetMinutesElapsedSinceBeginningOfSession() < EarlyMorningTradingGradePeriodMinutes)
                return;

            double timePassedInSectonds = Math.Abs(GetCurrentDateTime().Subtract(LastSpreadAdjustedAt).TotalSeconds);
            if (timePassedInSectonds <= RandomDelayInSecondsBetweenSlippageAdjustment)
                return;

            double temp = MaxSpreadFractionToAllocateForPricing +
                          (fractionMultiplier * SpreadFractionSteppingForAllocationOfPricing);

            if (temp > MinSpreadFractionToAllocateForPricing)
            {
                MaxSpreadFractionToAllocateForPricing = temp;
            }
            else
            {
                MaxSpreadFractionToAllocateForPricing = MinSpreadFractionToAllocateForPricing;
            }

            string message =
                string.Format(
                    "Old max spread fraction: {0:N4}, New max spread fraction: {1:N4}, Min spread fraction: {2:N4}",
                    MaxSpreadFractionToAllocateForPricing,
                    temp,
                    MinSpreadFractionToAllocateForPricing);

            LoggingUtility.WriteDebug(this, message);

            LastSpreadAdjustedAt = GetCurrentDateTime();

        }

        /// <summary>
        /// 
        /// </summary>
        private void ResetMaxAllowedSpread()
        {
            var temp = MaxSpreadFractionToAllocateForPricing;

            MaxSpreadFractionToAllocateForPricing = OriginalMaxSpreadFractionToAllocateForPricing;

            string message =
               string.Format(
                   "Old max spread fraction: {0:N4}, New max spread fraction: {1:N4}. Reset the max spread to original value",
                   temp,
                   MaxSpreadFractionToAllocateForPricing);

            LoggingUtility.WriteDebug(this, message);
        }

    }
}
