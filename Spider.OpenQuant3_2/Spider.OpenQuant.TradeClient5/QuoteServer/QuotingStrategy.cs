using System;

using EasyNetQ;

using NLog;

using OpenQuant.API;

using Spider.OpenQuant.TradeClient5.Common;
using Spider.OpenQuant.TradeClient5.Entities;
using Spider.OpenQuant.TradeClient5.Entities.Messages;
using Spider.OpenQuant.TradeClient5.Extensions;
using Spider.OpenQuant.TradeClient5.Util;

namespace Spider.OpenQuant.TradeClient5.QuoteServer
{
    public abstract class QuotingStrategy : Strategy
    {
        protected static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private const string LOG_SOURCE = "QUOTE-SERVER";

        #region Quote Server Management

        [Parameter("Quote Server Connection String", "Quote Server Management")] public string
            QuoteServerConnectionString = "host=miami.spider.local;username=manuj;password=manuj";


        #endregion

        protected IBus Bus = null; 

        public override void OnStrategyStart()
        {

            Bus = RabbitHutch.CreateBus(QuoteServerConnectionString);
           
            Log.WriteHorizontalBreak(LOG_SOURCE, this.Instrument);
            Log.WriteInfoLog(LOG_SOURCE, this.Instrument,
                string.Format("ENABLING quote server with conn string {0}", QuoteServerConnectionString));
            Log.WriteHorizontalBreak(LOG_SOURCE, this.Instrument);

            base.OnStrategyStart();
        }

        public override void OnStrategyStop()
        {

            Bus.Dispose();

            Bus = null;

            Log.WriteHorizontalBreak(LOG_SOURCE, this.Instrument);
            Log.WriteInfoLog(LOG_SOURCE, this.Instrument,
                string.Format("TEARING DOWN quote server with conn string {0}", QuoteServerConnectionString));
            Log.WriteHorizontalBreak(LOG_SOURCE, this.Instrument);

            LogManager.Flush();

            LoggingUtility.FlushLog();

            base.OnStrategyStop();
        }

        public override void OnBar(Bar bar)
        {
            if (!bar.IsWithinRegularTradingHours(Instrument.Type))
            {
                return;
            }

            if (bar.Size == PeriodConstants.PERIOD_DAILY)
            {
                return;
            }

           
            PublishBar(bar);

            base.OnBar(bar);
        }



        public override void OnBarOpen(Bar bar)
        {

            if (!bar.IsWithinRegularTradingHours(Instrument.Type))
            {
                return;
            }

            if (bar.Size == PeriodConstants.PERIOD_DAILY)
            {
                return;
            }


            PublishOpenBar(bar);

            base.OnBarOpen(bar);
        }

        public override void OnQuote(Quote quote)
        {

            PublishQuote(quote);

            base.OnQuote(quote);
        }

        private void PublishOpenBar(Bar bar)
        {
            string topic = Instrument.Symbol.ToUpperInvariant();
            OnBarOpenMessage message = new OnBarOpenMessage {Bar = ConvertBar(bar), Instrument = ConvertInstrument()};


            Bus.Publish(message, topic);
        }

        private void PublishBar(Bar bar)
        {
            string topic = Instrument.Symbol.ToUpperInvariant();
            OnBarMessage message = new OnBarMessage {Bar = ConvertBar(bar), Instrument = ConvertInstrument()};

            Log.WriteDebugLog(LOG_SOURCE, this.Instrument, string.Format("Publishing bar: {0}", bar));

            Bus.Publish(message, topic);
        }

        private void PublishQuote(Quote quote)
        {
            string topic = Instrument.Symbol.ToUpperInvariant();
            OnQuoteMessage message = new OnQuoteMessage {Quote = ConvertQuote(quote), Instrument = ConvertInstrument()};

            
            Bus.Publish(message, topic);
        }


        private SpiderQuote ConvertQuote(Quote originalQuote)
        {
            SpiderQuote spiderQuote = new SpiderQuote()
            {
                DateTime = originalQuote.DateTime,
                Ask = originalQuote.Ask,
                Bid = originalQuote.Bid,
                AskSize = originalQuote.AskSize,
                BidSize = originalQuote.BidSize
            };

            return spiderQuote;
        }

        private SpiderBar ConvertBar(Bar originalBar)
        {
            SpiderBar spiderBar = new SpiderBar()
            {
                IsComplete = originalBar.IsComplete,
                Open = originalBar.Open,
                High = originalBar.High,
                Low = originalBar.Low,
                Close = originalBar.Close,
                BeginTime = originalBar.BeginTime,
                EndTime = originalBar.EndTime,
                Size = originalBar.Size,
                Volume = originalBar.Volume
            };
            
            return spiderBar;
        }

        private SpiderInstrument ConvertInstrument()
        {
            return new SpiderInstrument()
            {
                InstrumentType = this.Instrument.Type,
                Symbol = this.Instrument.Symbol
            };
        }

        
    }
}
