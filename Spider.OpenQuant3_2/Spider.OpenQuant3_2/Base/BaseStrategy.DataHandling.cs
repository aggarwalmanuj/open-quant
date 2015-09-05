using System;
using System.Diagnostics;
using OpenQuant.API;
using Spider.OpenQuant3_2.Common;
using Spider.OpenQuant3_2.QuoteServer;
using Spider.OpenQuant3_2.Util;

namespace Spider.OpenQuant3_2.Base
{
    public abstract partial class BaseStrategy
    {
        protected void SetupData()
        {
            ResetData();

            PreLoadHistoricalData(CurrentValidityDateTime.AddDays(-50), PeriodConstants.PERIOD_DAILY, CurrentDailyBarSeries);

            CurrentLastSessionDate = CurrentDailyBarSeries.Last.BeginTime.Date;

            PreLoadHistoricalData(GetStartDateForIntradayHistoricalData(), CurrentExecutionTimePeriodInSeconds, CurrentExecutionBarSeries);
        }



        private void ResetData()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            try { DataManager.DeleteBarSeries(Instrument, BarType.Time, PeriodConstants.PERIOD_MINUTE); }
            catch { }
            try { DataManager.DeleteBarSeries(Instrument, BarType.Time, CurrentExecutionTimePeriodInSeconds); }
            catch { }
            try { DataManager.DeleteDailySeries(Instrument); }
            catch { }
            try { DataManager.DeleteQuoteSeries(Instrument); }
            catch { }
            /*
            try { DataManager.DeleteTradeSeries(Instrument); }
            catch { }
             */ 
            try { DataManager.Flush(); }
            catch { }

            watch.Stop();
            LoggingUtility.WriteInfo(this, string.Format("Reset instrument data in {0}ms", watch.ElapsedMilliseconds));
        }


        private void PreLoadHistoricalData(DateTime startDate, int barSize, BarSeries barsToAppendTo)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            string barName = BarNameManager.GetBarName(barSize);
            LoggingUtility.WriteInfo(this, string.Format("Trying to retrieve {0} data starting from {1} from IB", barName, startDate.ToShortDateString()));

            BarSeries historicalData = GetHistoricalBars("IB", Instrument,
                startDate, 
                Clock.Now,
                barSize);

            watch.Stop();

            int retrievedBarCount = historicalData.Count;

            LoggingUtility.WriteInfo(this,
                string.Format("Took {0}ms to retrieve data from IB for {1} period data. Total bars retrieved: {2}",
                    watch.ElapsedMilliseconds, barName, retrievedBarCount));


            watch.Reset();
            watch.Start();

            foreach (Bar currentBar in historicalData)
            {
                barsToAppendTo.Add(currentBar);

                SaveData(currentBar);

                OnBar(currentBar);
            }

            watch.Stop();

            LoggingUtility.WriteInfo(this,
                string.Format("Took {0}ms to load data into memory for {1} data", watch.ElapsedMilliseconds, barName));

        }

        protected Quote ConvertAndFlushQuote(SpiderQuote originalQuote)
        {

            DataManager.Add(this.Instrument,
                originalQuote.DateTime,
                originalQuote.Bid,
                originalQuote.BidSize,
                originalQuote.Ask,
                originalQuote.AskSize);

            DataManager.Flush();

            var convertedAndAddedQuote = DataManager.GetHistoricalQuotes(this.Instrument,
                originalQuote.DateTime.AddMinutes(-20 * ExecutionTimePeriod),
                originalQuote.DateTime).Last;

            return convertedAndAddedQuote;
        }

        protected void SaveData(Bar bar)
        {
            DataManager.Add(this.Instrument, bar);

            DataManager.Flush();

            if (bar.Size == CurrentExecutionTimePeriodInSeconds)
            {
                CurrentExecutionBarSeries.Add(bar);
            }

            if (bar.Size == PeriodConstants.PERIOD_DAILY)
            {
                CurrentDailyBarSeries.Add(bar);
            }
        }

        protected void SaveData(Instrument instrument, Quote quote)
        {
            DataManager.Add(instrument, quote);

            DataManager.Flush();
        }



        protected Bar ConvertAndFlushBar(SpiderBar originalBar)
        {
            DataManager.Add(this.Instrument,
                originalBar.BeginTime,
                originalBar.Open,
                originalBar.High,
                originalBar.Low,
                originalBar.Close,
                originalBar.Volume,
                originalBar.Size);

            DataManager.Flush();

            if (originalBar.Size == CurrentExecutionTimePeriodInSeconds)
            {
                CurrentExecutionBarSeries.Add(originalBar.BeginTime,
                    originalBar.Open,
                    originalBar.High,
                    originalBar.Low,
                    originalBar.Close,
                    originalBar.Volume,
                    originalBar.Size);
            }


            var convertedAndAddedBar = DataManager.GetHistoricalBars(this.Instrument,
                originalBar.BeginTime.AddSeconds(-20 * (originalBar.Size)),
                originalBar.EndTime,
                BarType.Time,
                originalBar.Size).Last;


            return convertedAndAddedBar;
        }

        private DateTime GetStartDateForIntradayHistoricalData()
        {
            int dayToGet = -2;
            DateTime startDateForIntradayyData = CurrentValidityDateTime.Date.AddDays(dayToGet);
            bool isHoliday = startDateForIntradayyData.Date.DayOfWeek == DayOfWeek.Saturday ||
                             startDateForIntradayyData.Date.DayOfWeek == DayOfWeek.Sunday;

            while (isHoliday)
            {
                startDateForIntradayyData = startDateForIntradayyData.AddDays(-1);
                isHoliday = startDateForIntradayyData.Date.DayOfWeek == DayOfWeek.Saturday ||
                            startDateForIntradayyData.Date.DayOfWeek == DayOfWeek.Sunday;
            }

            return startDateForIntradayyData;
        }
    }
}
