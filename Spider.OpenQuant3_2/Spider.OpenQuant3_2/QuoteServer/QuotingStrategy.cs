using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EasyNetQ;
using OpenQuant.API;
using Spider.OpenQuant3_2.Common;
using Spider.OpenQuant3_2.Extensions;

namespace Spider.OpenQuant3_2.QuoteServer
{
    public abstract class QuotingStrategy : Strategy
    {
        #region Quote Server Management

        [Parameter("Quote Server Connection String", "Quote Server Management")] public string
            QuoteServerConnectionString = "host=miami.spider.local;username=manuj;password=manuj";


        #endregion

        protected IBus Bus = null; 

        public override void OnStrategyStart()
        {

            Bus = RabbitHutch.CreateBus(QuoteServerConnectionString);
           
            WriteHorizontalBreak();
            WriteInfoFormat("ENABLING quote server with conn string {0}", QuoteServerConnectionString);
            WriteHorizontalBreak();

            base.OnStrategyStart();
        }

        public override void OnStrategyStop()
        {

            Bus.Dispose();

            Bus = null;

            WriteHorizontalBreak();
            WriteInfoFormat("TEARING DOWN quote server with conn string {0}", QuoteServerConnectionString);
            WriteHorizontalBreak();

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

            WriteInfoFormat("Publishing bar: {0}", bar);

            Bus.Publish(message, topic);
        }

        private void PublishQuote(Quote quote)
        {
            string topic = Instrument.Symbol.ToUpperInvariant();
            OnQuoteMessage message = new OnQuoteMessage {Quote = ConvertQuote(quote), Instrument = ConvertInstrument()};

            
            Bus.Publish(message, topic);
        }


        private SpiderQuote ConvertQuote(OpenQuant.API.Quote originalQuote)
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

        private SpiderBar ConvertBar(OpenQuant.API.Bar originalBar)
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

        private void WriteHorizontalBreak()
        {
            WriteInfo("----------------------------");
        }

        private void WriteInfo(string message)
        {
            string prefix = string.Format("[{0}  |  QUOTE-SERVER]", Instrument.Symbol);
            string datePart = string.Format("[{0:dd-MMM-yyyy hh:mm:ss.fff tt}]", DateTime.Now);
            const string formattedString = "{0} -- {1} -- {2}";
            Console.WriteLine(string.Format(formattedString, prefix, datePart, message));
        }

        private void WriteInfoFormat(string formattedString, params object[] args)
        {
            WriteInfo(string.Format(formattedString, args));
        }
    }
}
