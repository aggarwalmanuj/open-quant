using System;
using System.Linq;

using OpenQuant.API;

using Spider.OpenQuant.TradeClient5.Common;
using Spider.OpenQuant.TradeClient5.Extensions;
using Spider.OpenQuant.TradeClient5.Util;

namespace Spider.OpenQuant.TradeClient5.Base
{
    public abstract partial class BaseStrategy
    {
        protected Bar CurrentBarRef { get; set; }

        protected Quote CurrentQuoteRef { get; set; }


        protected double CurrentAtrPrice { get; set; }

        protected double CurrentClosePrice { get; set; }

        protected double CurrentAskPrice { get; set; }

        protected double CurrentBidPrice { get; set; }

        protected double CurrentDailyAvgPrice { get; set; }

        protected double CurrentDailyAvgVolume { get; set; }
        
        protected double CurrentDayOperningPrice { get; set; }

        protected double CurrentLastPrice { get; set; }


        public void ProcessDailyBar(Bar bar)
        {
            try
            {
                if (!IsItOkToHandleBar(bar))
                {
                    return;
                }

                LoggingUtility.WriteTraceFormat(this, "Processing daily bar: {0}", bar);

                BasicSaveBar(bar);

                CurrentBarRef = bar;

                SetCurrentDayOpeningPrice(bar);

               
                if (CurrentDailyBarSeries.Count > DailySmaPeriod)
                {
                    try
                    {
                        CurrentDailyAvgPrice = DailyPriceSmaIndicator.Last;
                    }
                    catch(Exception exSma)
                    {
                        LoggingUtility.WriteError(this, exSma, "Exception while trying to get daily average price");
                    }
                    try
                    {
                        CurrentDailyAvgVolume = DailyVolumeSmaIndicator.Last*100;

                        if (MaxPercentageOfAvgVolumeOrderSize <= 0)
                            throw new ArgumentOutOfRangeException("MaxPercentageOfAvgVolumeOrderSize", MaxPercentageOfAvgVolumeOrderSize, "MaxPercentageOfAvgVolumeOrderSize must be more than 0");

                        MaximumOrderSize = CurrentDailyAvgVolume * MaxPercentageOfAvgVolumeOrderSize / 100;

                    }
                    catch (Exception exSma)
                    {
                        LoggingUtility.WriteError(this, exSma, "Exception while trying to get daily average volume");
                    }

                    if (CurrentDailyAvgPrice > 0 && IsBarCloseEnoughForLogging(bar))
                    {
                        LoggingUtility.WriteInfoFormat(this,
                            "Setting SMA AVG ({0}) Price to {1:c}",
                            DailySmaPeriod,
                            CurrentDailyAvgPrice);
                    }
                    else
                    {
                        LoggingUtility.WriteTraceFormat(this,
                           "Trace: Setting SMA AVG ({0}) Price to {1:c}",
                           DailySmaPeriod,
                           CurrentDailyAvgPrice);
                    }

                    if (CurrentDailyAvgVolume > 0 && IsBarCloseEnoughForLogging(bar))
                    {
                      
                        LoggingUtility.WriteInfoFormat(this,
                            "Setting SMA AVG ({0}) Volume to {1:N4}. Max order size to {2:N4}",
                            DailySmaPeriod,
                            CurrentDailyAvgVolume,
                            MaximumOrderSize);
                    }
                    else
                    {
                        LoggingUtility.WriteTraceFormat(this,
                            "Trace: Setting SMA AVG ({0}) Volume to {1:N4}. Max order size to {2:N4}",
                            DailySmaPeriod,
                            CurrentDailyAvgVolume,
                            MaximumOrderSize);
                    }
                }
                else
                {
                    LoggingUtility.WriteTraceFormat(this, "Daily bars not enough for SMA calculation. DailySmaPeriod: {0}, Bar count: {1}", DailySmaPeriod, CurrentDailyBarSeries.Count);
                }

                if (CurrentDailyBarSeries.Count > AtrPeriod)
                {
                    try
                    {
                        CurrentAtrPrice = DailyAtrIndicator.Last;
                    }
                    catch (Exception exAtr)
                    {
                        LoggingUtility.WriteError(this, exAtr, "Exception while trying to get daily ATR");
                    }
                    if (CurrentClosePrice > 0 && IsBarCloseEnoughForLogging(bar))
                    {
                        LoggingUtility.WriteInfoFormat(this,
                            "Setting ATR to {0:c} which is {1:p} of the last close price {2:c}",
                            CurrentAtrPrice,
                            CurrentAtrPrice/CurrentClosePrice,
                            CurrentClosePrice);
                    }
                    else
                    {
                        LoggingUtility.WriteTraceFormat(this,
                            "Trace: Setting ATR to {0:c} which is {1:p} of the last close price {2:c}",
                            CurrentAtrPrice,
                            CurrentAtrPrice / CurrentClosePrice,
                            CurrentClosePrice);
                    }
                }
                else
                {
                    LoggingUtility.WriteTraceFormat(this, "Daily bars not enough for ATR calculation. AtrPeriod: {0}, Bar count: {1}", AtrPeriod, CurrentDailyBarSeries.Count);
                }
            }
            catch (Exception ex)
            {
                LoggingUtility.WriteError(this, ex, "Error in ProcessDailyBar");
            }
            finally
            {
                CurrentBarRef = null;
            }
        }


        public void ProcessBar(Bar bar)
        {
            try
            {
                if (!IsItOkToHandleBar(bar))
                {
                    return;
                }

                if (bar.BeginTime >= CurrentValidityDateTime && !AreAllLegsCompleted)
                {
                    LoggingUtility.WriteTraceFormat(this, "Processing bar: {0}", bar);
                }

                BasicSaveBar(bar);

                CurrentBarRef = bar;

                SetCurrentDayOpeningPrice(bar);

                SetCurrentLastPrice(bar);

                if (bar.Size == CurrentExecutionTimePeriodInSeconds)
                {
                    int[] periods = new[]
                    {
                        SlowMaPeriod,
                        FastMaPeriod,
                        StochasticsDPeriod,
                        StochasticsKPeriod,
                        StochasticsSmoothPeriod
                    };

                    int maxPeriod = periods.Max();

                    if (CurrentExecutionBarSeries.Count <= maxPeriod)
                    {
                        return;
                    }

                    // Ensure that intraday indicators are evaluated
                    GetIsStochInBullishMode(bar);
                    GetIsStochInBearishMode(bar);
                    GetIsEmaInBearishMode(bar);
                    GetIsEmaInBullishMode(bar);
                    GetIsEmaAlmostInBearishMode(bar);
                    GetIsEmaAlmostInBullishMode(bar);

                    GetIsStochInObMode(bar);
                    GetIsStochInOsMode(bar);

                    
                    AttempOrder();
                }

            }
            catch (Exception ex)
            {
                LoggingUtility.WriteError(this, ex, "Error in ProcessBar");
            }
            finally
            {
                CurrentBarRef = null;
            }
        }

        private void SetCurrentLastPrice(Bar bar)
        {
            if (bar.BeginTime >= CurrentStartOfSessionTime && !AreAllLegsCompleted)
            {
                if ( bar.Close > 0)
                {
                    CurrentLastPrice = bar.Close;

                    WriteInfrequentDebugMessage(string.Format("Setting last price for {0:c2}", CurrentLastPrice));
                }
            }
        }

        private void SetCurrentDayOpeningPrice(Bar bar)
        {
            if (bar.BeginTime >= CurrentStartOfSessionTime)
            {
                if (CurrentDayOperningPrice <= 0 && bar.Open > 0)
                {
                    CurrentDayOperningPrice = bar.Open;

                    LoggingUtility.WriteInfoFormat(this, "Setting opening price for today {0:c2}", CurrentDayOperningPrice);
                }
            }
        }

        public void ProcessQuote(Quote quote)
        {
            try
            {

                
                if (!IsItOkToHandleQuote(quote))
                {
                    return;
                }

                if (GetCurrentDateTime() >= CurrentValidityDateTime && !AreAllLegsCompleted)
                {
                    LoggingUtility.WriteTraceFormat(this, "Processing quote: {0}", quote);
                }

                CurrentQuoteRef = quote;

                SaveData(Instrument, quote);

                CurrentAskPrice = quote.Ask;
                CurrentBidPrice = quote.Bid;

                AttempOrder();
            }
            catch (Exception ex)
            {
                LoggingUtility.WriteError(this, ex, "Error in ProcessQuote");
            }
            finally
            {
                CurrentQuoteRef = null;
            }
        }

        

        private void BasicSaveBar(Bar bar)
        {
            SaveData(bar);
            CurrentClosePrice = bar.Close;
        }



        private bool IsBarCloseEnoughForLogging(Bar bar)
        {

            long barSize = bar.Size;
            if (barSize == PeriodConstants.PERIOD_DAILY)
            {
                return (CurrentValidityDateTime.Subtract(bar.BeginTime.Date).TotalDays <= 7);
            }

            if (!IsCurrentOrderActive())
                return false;

            if (bar.BeginTime >= CurrentValidityDateTime)
                return true;

            return (bar.BeginTime >= CurrentLastSessionDate.Date.AddHours(12));


        }


        protected bool IsItOkToHandleBar(Bar bar)
        {
            if (!bar.IsWithinRegularTradingHours(Instrument.Type))
            {
                LoggingUtility.WriteTraceFormat(this, "Bar is not within regular trading hours: {0}", bar);
                return false;
            }

            return true;
        }

        protected bool IsItOkToHandleQuote(Quote quote)
        {
            if (!quote.IsWithinRegularTradingHours(Instrument.Type))
            {
                LoggingUtility.WriteTraceFormat(this, "Quote is not within regular trading hours: {0}", quote);
                return false;
            }

            return true;
        }
    }
}
