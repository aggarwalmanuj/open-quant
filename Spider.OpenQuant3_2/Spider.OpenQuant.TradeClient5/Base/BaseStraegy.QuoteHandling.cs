using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQuant.API;

using Spider.OpenQuant.TradeClient5.Entities;
using Spider.OpenQuant.TradeClient5.QuoteServer;

namespace Spider.OpenQuant.TradeClient5.Base
{
    public abstract partial class BaseStrategy
    {
        protected Quote ConvertAndFlushQuote(SpiderQuote originalQuote)
        {
            Quote convertedAndAddedQuote = null;

            lock (LockObject)
            {
                DataManager.Add(this.Instrument,
                    originalQuote.DateTime,
                    originalQuote.Bid,
                    originalQuote.BidSize,
                    originalQuote.Ask,
                    originalQuote.AskSize);

                DataManager.Flush();

                convertedAndAddedQuote = DataManager.GetHistoricalQuotes(this.Instrument,
                    originalQuote.DateTime.AddMinutes(-20*ExecutionTimePeriod),
                    originalQuote.DateTime).Last;
            }

            return convertedAndAddedQuote;
        }


        protected Bar ConvertAndFlushBar(SpiderBar originalBar)
        {

            Bar convertedAndAddedBar = null;

            lock (LockObject)
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


                convertedAndAddedBar = DataManager.GetHistoricalBars(this.Instrument,
                    originalBar.BeginTime.AddSeconds(-20*(originalBar.Size)),
                    originalBar.EndTime,
                    BarType.Time,
                    originalBar.Size).Last;
            }

            return convertedAndAddedBar;
        }
    }
}
