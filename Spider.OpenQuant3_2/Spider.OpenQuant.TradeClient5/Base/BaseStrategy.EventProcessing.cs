using System;
using System.Diagnostics;

using NLog;
using OpenQuant.API;
using OpenQuant.API.Indicators;
using Spider.OpenQuant.TradeClient5.Common;
using Spider.OpenQuant.TradeClient5.Extensions;
using Spider.OpenQuant.TradeClient5.Util;

namespace Spider.OpenQuant.TradeClient5.Base
{
    public abstract partial class BaseStrategy
    {


        public override void OnBar(Bar bar)
        {
            try
            {
               
                LoggingUtility.WriteTraceFormat(this, "OnBar. {0}", bar);

                if (!IsItOkToHandleBar(bar))
                {
                    if (bar.BeginTime >= CurrentValidityDateTime && !AreAllLegsCompleted)
                    {
                        LoggingUtility.WriteDebug(this, "Strategy not in valid state");
                    }
                    return;
                }

                if (bar.Size == PeriodConstants.PERIOD_DAILY)
                {
                    LoggingUtility.WriteTraceFormat(this, "Processing daily bar. {0}", bar);

                    ProcessDailyBar(bar);
                }

                if (bar.Size == CurrentExecutionTimePeriodInSeconds)
                {
                    bool needToProcessBar = !QuoteClientEnabled;
                    if (QuoteClientEnabled)
                    {
                        // Need to process any bars before the strategy kicked off
                        // which were received via the historical data
                        // New bars for current day should be received by the quote client
                        needToProcessBar = bar.BeginTime <= QuoteClientStartAt;
                    }

                    if (needToProcessBar)
                    {
                        if (!AreAllLegsCompleted)
                        {
                            LoggingUtility.WriteTraceFormat(this,
                                "Processing native bar. {0}", bar);
                        }

                        ProcessBar(bar);
                    }
                    else
                    {
                        if (!AreAllLegsCompleted)
                        {
                            LoggingUtility.WriteTraceFormat(this, "Discarding native bar. {0}",
                                bar);
                        }
                    }
                }

                base.OnBar(bar);
            }
            catch (Exception ex)
            {
                LoggingUtility.WriteError(this, ex, "Error in OnBar");
            }
            finally
            {
            }
        }

        public override void OnBarOpen(Bar bar)
        {
            try
            {
                if (!IsItOkToHandleBar(bar))
                {
                    return;
                }

                LoggingUtility.WriteTraceFormat(this, "OnBarOpen. {0}", bar);

                if (bar.Size == PeriodConstants.PERIOD_DAILY)
                {
                    // Since we will like to capture any gaps in the current day
                    // open and corresponding effects of ATR calculations
                    OnBar(bar);
                }
                else if (bar.BeginTime >= CurrentStartOfSessionTime)
                {
                    //EvaluateIndicatorsAtBeginningOfSession();

                    OnBar(bar);

                }

                base.OnBarOpen(bar);


            }
            catch (Exception ex)
            {
                LoggingUtility.WriteError(this, ex, "Error in OnBarOpen");
            }
        }


        public override void OnOrderStatusChanged(Order order)
        {

            try
            {
                string message = string.Format("*** ORDER UPDATE: {0}, Status={1} ***", order.Text, order.Status);
                LoggingUtility.WriteInfo(this, message);
            }
            catch 
            {
                
            }

            try
            {
                

                if (order.IsDone || order.IsCancelled || order.IsRejected || order.IsFilled)
                {
                    LastOrderCompletedAt = GetCurrentDateTime();

                    ResetMaxAllowedSpread();

                    ResetLastOrderPrice();
                }

                if (null != CurrentTradeLeg)
                {
                    CurrentTradeLeg.OnOrderStatusChanged(order);

                    if (CurrentTradeLeg.IsLegComplete)
                    {
                        UpdateTradeLegsCompletedFlag();

                        LoggingUtility.WriteInfoFormat(this, "Completed trade leg: {0}", CurrentTradeLeg.LegName);

                        UpdateNextTradeLeg();
                    }
                }

              
                base.OnOrderStatusChanged(order);
            }
            catch (Exception ex)
            {
                LoggingUtility.WriteError(this, ex, "Error in OnOrderStatusChanged");

            }
        }

        private void ResetLastOrderPrice()
        {
            CurrentLastOrderPrice = 0;
        }


        public override void OnQuote(Quote quote)
        {
            try
            {
  
                if (!quote.IsWithinRegularTradingHours(Instrument.Type))
                {
                    LoggingUtility.WriteTraceFormat(this,
                                "Discarding quote outside of trading hours. {0}", quote);
                    return;
                }

                if (!QuoteClientEnabled)
                {
                    LoggingUtility.WriteTraceFormat(this,
                                "Processing native quote. {0}", quote);

                    ProcessQuote(quote);
                }
                else
                {
                    LoggingUtility.WriteTraceFormat(this,
                                "Discarding native quote. {0}", quote);
                }

                base.OnQuote(quote);
            }
            catch (Exception ex)
            {
                LoggingUtility.WriteError(this, ex, "Error in OnQuote");
            }
        }

        protected bool AreConditionsOkForOrderProcessing()
        {
            if (GetCurrentDateTime() < CurrentValidityDateTime)
            {
                return false;
            }

            if (AreAllLegsCompleted)
            {
                LoggingUtility.WriteTrace(this, "AreConditionsOkForOrderProcessing: all legs are completed");
                return false;
            }

            if (CurrentTradeLeg == null)
            {
                LoggingUtility.WriteDebug(this, "AreConditionsOkForOrderProcessing: CurrentTradeLeg is null");
                return false;
            }

            if (!GetCurrentDateTime().IsWithinRegularTradingHours(Instrument.Type))
            {
                return false;
            }

            return true;
        }
    }
}
