using System;

using NLog;

using OpenQuant.API;

namespace Spider.OpenQuant.TradeClient5.Util
{
    internal static class LoggerExtensions
    {
        

        internal static void WriteHorizontalBreak(this Logger log, string source, Instrument instrument)
        {
            WriteInfoLog(log, source, instrument, "----------------------------");
        }

        internal static void WriteInfoLog(this Logger log, string source, Instrument instrument, string message)
        {
            if (!log.IsInfoEnabled)
                return;

            log.Info(GetFormattedMessage(source, instrument, message));
        }

        internal static void WriteDebugLog(this Logger log, string source, Instrument instrument, string message)
        {
            if (!log.IsDebugEnabled)
                return;

            log.Debug(GetFormattedMessage(source, instrument, message));
        }


        internal static void WriteTraceLog(this Logger log, string source, Instrument instrument, string message)
        {
            if (!log.IsTraceEnabled)
                return;

            log.Trace(GetFormattedMessage(source, instrument, message));
        }
        private static string GetFormattedMessage(string source, Instrument instrument, string message)
        {
            string prefix = string.Format("[{0}  |  {1}]", instrument.Symbol.ToUpper(), source.ToUpper());
            string datePart = string.Format("[{0:dd-MMM-yyyy hh:mm:ss.fff tt}]", DateTime.Now);
            const string formattedString = "{0} -- {1} -- {2}";
            return string.Format(formattedString, prefix, datePart, message);
        }
    }
}