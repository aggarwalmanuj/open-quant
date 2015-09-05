using System;
using NLog;
using OpenQuant.API;
using OpenQuant.API.Indicators;
using Spider.OpenQuant.TradeClient5.Common;
using Spider.OpenQuant.TradeClient5.Util;

namespace Spider.OpenQuant.TradeClient5.Base
{
    public abstract partial class BaseStrategy
    {
        protected double TotalNumberOfSlicesAvailableInSession { get; set; }

        public Logger GetLogger()
        {
            return Log;
        }

        protected void WriteInfrequentDebugMessage(string logMessage)
        {
            if (GetCurrentDateTime() > CurrentValidityDateTime && !AreAllLegsCompleted)
            {
                if (OrderRetrialCheckCounter % 10 == 0)
                {
                    LoggingUtility.WriteDebug(this, logMessage);
                }
                else
                {
                    LoggingUtility.WriteTrace(this, logMessage);
                }
            }
        }

        public DateTime GetCurrentDateTime()
        {
            if (CurrentBarRef != null && CurrentBarRef.BeginTime.Year >= 2010)
            {
                return CurrentBarRef.BeginTime;
            }

            if (CurrentQuoteRef != null && CurrentQuoteRef.DateTime.Year >= 2010)
            {
                return CurrentQuoteRef.DateTime;
            }

            if (Bar != null && Bar.BeginTime.Year >= 2010)
            {
                return Bar.BeginTime;
            }

            return Clock.Now;
        }


        protected double GetRemainderSessionTimeFraction()
        {

            // Assuming we want to execute at least 30 minutes before the close 
            double totalNumberOfMinutes = Math.Abs(CurrentEndOfSessionTime.Subtract(CurrentStartOfSessionTime).TotalMinutes) - 30;
            TotalNumberOfSlicesAvailableInSession = totalNumberOfMinutes / TimeSliceIntervalInMinutes;

            var numberOfMinutesElapsedSinceBeginningOfSession = GetMinutesElapsedSinceBeginningOfSession();

            double remainder = totalNumberOfMinutes - numberOfMinutesElapsedSinceBeginningOfSession;
            double fractionRemainder = remainder / totalNumberOfMinutes;

            
            LoggingUtility.WriteTraceFormat(this, "{0:F2} minutes passed in the session with {1:F2} minutes ({2:p}) remaining.",
                numberOfMinutesElapsedSinceBeginningOfSession, remainder, fractionRemainder);
            
            return fractionRemainder;
        }

        protected double GetMinutesElapsedSinceBeginningOfSession()
        {
            DateTime sessionDate = GetCurrentDateTime().Date;
            DateTime startOfTodaySession = sessionDate.AddSeconds(PstSessionTimeConstants.StockExchangeStartTimeSeconds);

            double numberOfMinutesElapsedSinceBeginningOfSession =
                Math.Abs(GetCurrentDateTime().Subtract(startOfTodaySession).TotalMinutes);
            return numberOfMinutesElapsedSinceBeginningOfSession;
        }
    }
}
