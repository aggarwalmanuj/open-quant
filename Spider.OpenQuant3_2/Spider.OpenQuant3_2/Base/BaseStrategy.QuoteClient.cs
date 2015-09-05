using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using EasyNetQ;
using OpenQuant.API;
using Spider.OpenQuant3_2.QuoteServer;
using Spider.OpenQuant3_2.Util;

namespace Spider.OpenQuant3_2.Base
{
    public abstract partial class BaseStrategy
    {
        protected string CurrentQuoteClientSubscription = string.Empty;

        protected IBus Bus = null;

        protected int numberOfQuotes = 0;

        protected void SetupQuoteClient()
        {
            if (QuoteClientEnabled)
            {
                Bus = RabbitHutch.CreateBus(QuoteClientConnectionString);

                string instrumentSymbol = Instrument.Symbol.ToUpperInvariant();
                CurrentQuoteClientSubscription = string.Format(
                    "{0}.{1}.{2}",
                    EnvironmentManager.GetNormalizedMachineName(),
                    ProjectName,
                    instrumentSymbol)
                    .ToUpperInvariant()
                    .Replace("-", "")
                    .Replace("_", "");


                LoggingUtility.WriteInfoFormat(this, "ENABLING quote client with sub id: {0}",
                    CurrentQuoteClientSubscription);

                Bus.Subscribe<OnBarOpenMessage>(CurrentQuoteClientSubscription, HandleOnBarOpenFromBus,
                    x => x.WithTopic(instrumentSymbol));
                Bus.Subscribe<OnBarMessage>(CurrentQuoteClientSubscription, HandleOnBarFromBus,
                    x => x.WithTopic(instrumentSymbol));
                Bus.Subscribe<OnQuoteMessage>(CurrentQuoteClientSubscription, HandleOnQuoteFromBus,
                    x => x.WithTopic(instrumentSymbol));

                numberOfQuotes = 0;
            }
            else
            {
                LoggingUtility.WriteInfo(this, "Quote client is NOT ENABLED");
            }
        }



        private void HandleOnQuoteFromBus(OnQuoteMessage quoteMessage)
        {
            if (IsOrderInCompletedState())
            {
                return;
            }

            if (null != quoteMessage && null != quoteMessage.Quote && null != quoteMessage.Instrument)
            {
                if (ValidateInstrument(quoteMessage.Instrument) && ValidateQuote(quoteMessage.Quote))
                {
                    var convertedAndAddedQuote = ConvertAndFlushQuote(quoteMessage.Quote);

                    numberOfQuotes++;

                    OnQuote(convertedAndAddedQuote);

                    if (numberOfQuotes % 10 == 0 && !IsOrderInCompletedState())
                    {
                        LoggingUtility.WriteInfoFormat(this, "QUOTE CLIENT - Received Quote: {0}", convertedAndAddedQuote);
                    }

                }
            }
        }

      

        private void HandleOnBarOpenFromBus(OnBarOpenMessage barMessage)
        {
            if (IsOrderInCompletedState())
            {
                return;
            }

            if (null != barMessage && null != barMessage.Bar && null != barMessage.Instrument)
            {
                if (ValidateInstrument(barMessage.Instrument) && ValidateBar(barMessage.Bar))
                {
                    var convertedAndAddedBar = ConvertAndFlushBar(barMessage.Bar);

                    OnBarOpen(convertedAndAddedBar);
                }
            }
        }

        private void HandleOnBarFromBus(OnBarMessage barMessage)
        {
           

            if (null != barMessage && null != barMessage.Bar && null != barMessage.Instrument)
            {
                if (ValidateInstrument(barMessage.Instrument) && ValidateBar(barMessage.Bar))
                {
                    var convertedAndAddedBar = ConvertAndFlushBar(barMessage.Bar);

                    OnBar(convertedAndAddedBar);
                    
                    if (!IsOrderInCompletedState())
                    {
                        LoggingUtility.WriteInfoFormat(this, "QUOTE CLIENT - Received Bar: {0}", convertedAndAddedBar);
                    }
                }
            }
        }

        

        /*
        private OpenQuant.API.Bar ConvertBar(SpiderBar originalBar)
        {
            SmartQuant.Data.Bar smartBar = new SmartQuant.Data.Bar()
            {
                IsComplete = originalBar.IsComplete,
                Open = originalBar.Open,
                High = originalBar.High,
                Low = originalBar.Low,
                Close = originalBar.Close,
                DateTime = originalBar.BeginTime,
                EndTime = originalBar.EndTime,
                Size = originalBar.Size,
                Volume = originalBar.Volume
            };

            object[] parameter = new object[1];
            parameter[0] = smartBar;

            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
            CultureInfo culture = null; // use InvariantCulture or other if you prefer
            OpenQuant.API.Bar convertedBar =
                Activator.CreateInstance(typeof(OpenQuant.API.Bar), flags, null, parameter, culture) as
                    OpenQuant.API.Bar;

            return convertedBar;
        }


        private OpenQuant.API.Quote ConvertQuote(SpiderQuote originalQuote)
        {
            SmartQuant.Data.Quote smartQuote = new SmartQuant.Data.Quote()
            {
                DateTime = originalQuote.DateTime,
                Ask = originalQuote.Ask,
                AskSize = originalQuote.AskSize,
                Bid = originalQuote.Bid,
                BidSize = originalQuote.BidSize
            };

            object[] parameter = new object[1];
            parameter[0] = smartQuote;

            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
            CultureInfo culture = null; // use InvariantCulture or other if you prefer
            OpenQuant.API.Quote convertedQuote =
                Activator.CreateInstance(typeof(OpenQuant.API.Quote), flags, null, parameter, culture) as
                    OpenQuant.API.Quote;

            return convertedQuote;
        }
        */

        private bool ValidateInstrument(SpiderInstrument instrument)
        {
            return string.Compare(
                Instrument.Symbol,
                instrument.Symbol,
                StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        private bool ValidateBar(SpiderBar bar)
        {
            return (bar.Open > 0 &&
                    bar.High > 0 &&
                    bar.Low > 0 &&
                    bar.Close > 0 &&
                    bar.Volume > 0);

        }

        private bool ValidateQuote(SpiderQuote quote)
        {
            return (quote.Bid > 0 &&
                    quote.Ask > 0 &&
                    quote.AskSize > 0 &&
                    quote.BidSize > 0);

        }

        protected void TearDownQuoteClient()
        {
            if (QuoteClientEnabled)
            {
                LoggingUtility.WriteInfoFormat(this, "TEARING DOWN quote client with sub id: {0}",
                    CurrentQuoteClientSubscription);

                Bus.Dispose();

                Bus = null;
            }
        }
    }
}
