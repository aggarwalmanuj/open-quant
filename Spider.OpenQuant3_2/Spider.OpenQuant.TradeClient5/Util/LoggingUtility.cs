using System;
using System.Linq;

using NLog;
using NLog.Targets.Wrappers;

using Spider.OpenQuant.TradeClient5.Base;

namespace Spider.OpenQuant.TradeClient5.Util
{
    public static  class LoggingUtility
    {
        private const string FormattedMessageString = "{0} -- {1} -- {2}";

        internal static void WriteHorizontalBreak(BaseStrategy strategy)
        {
            WriteHorizontalBreak(strategy, 0);
        }

        internal static void WriteHorizontalBreak(BaseStrategy strategy, int numberOfLineBreaks)
        {
            var prefix = GetLogMessagePrefix(strategy);
            var datePart = GetLogMessageDatePart(strategy);

            Console.WriteLine(FormattedMessageString, prefix, datePart, "----------------------------");
            for (int i = 0; i < numberOfLineBreaks; i++)
            {
                Console.WriteLine();
            }
        }

        internal static void WriteInfo(BaseStrategy strategy, string message)
        {
            var logger = strategy.GetLogger();
            if (!logger.IsInfoEnabled)
                return;

            var prefix = GetLogMessagePrefix(strategy);
            var datePart = GetLogMessageDatePart(strategy);

            logger.Info(FormattedMessageString, prefix, datePart, message);
        }

        internal static void WriteInfoFormat(BaseStrategy strategy, string formattedString, params object[] args)
        {
            WriteInfo(strategy, string.Format(formattedString, args));
        }

        internal static void WriteTrace(BaseStrategy strategy, string message)
        {
            var logger = strategy.GetLogger();
            if (!logger.IsTraceEnabled)
                return;

            var prefix = GetLogMessagePrefix(strategy);
            var datePart = GetLogMessageDatePart(strategy);

            logger.Trace(FormattedMessageString, prefix, datePart, message);
        }

        internal static void WriteTraceFormat(BaseStrategy strategy, string formattedString, params object[] args)
        {
            WriteTrace(strategy, string.Format(formattedString, args));
        }

        internal static void WriteDebug(BaseStrategy strategy, string message)
        {
            var logger = strategy.GetLogger();
            if (!logger.IsDebugEnabled)
                return;

            var prefix = GetLogMessagePrefix(strategy);
            var datePart = GetLogMessageDatePart(strategy);

            logger.Debug(FormattedMessageString, prefix, datePart, message);
        }

        internal static void WriteDebugFormat(BaseStrategy strategy, string formattedString, params object[] args)
        {
            WriteDebug(strategy, string.Format(formattedString, args));
        }


        internal static void WriteError(BaseStrategy strategy, Exception ex, string message)
        {
            var logger = strategy.GetLogger();
            if (!logger.IsErrorEnabled)
                return;

            var prefix = GetLogMessagePrefix(strategy);
            var datePart = GetLogMessageDatePart(strategy);

            logger.Error(ex, FormattedMessageString, prefix, datePart, message);
        }

        internal static void WriteErrorFormat(BaseStrategy strategy, Exception ex, string formattedString, params object[] args)
        {
            WriteError(strategy, ex, string.Format(formattedString, args));
        }

        private static string GetLogMessageDatePart(BaseStrategy strategy)
        {
            string datePart = string.Format("[{0:dd-MMM-yyyy hh:mm:ss.fff tt}]", strategy.GetCurrentDateTime());
            return datePart;
        }

        private static string GetLogMessagePrefix(BaseStrategy strategy)
        {
            string prefix = string.Format("[{0} | {1}]", strategy.IbAccountNumber, strategy.Instrument.Symbol);
            return prefix;
        }

        public static void FlushLog()
        {
            try
            {
                LogManager.Configuration.AllTargets
                    .OfType<BufferingTargetWrapper>()
                    .ToList()
                    .ForEach(b => b.Flush(e =>
                    {
                        //do nothing here
                    }));
            }
            catch
            {
                LogManager.Configuration.AllTargets
                    .ToList()
                    .ForEach(b => b.Flush(e =>
                    {
                        //do nothing here
                    }));
            }

            try
            {

            }
            catch
            {

            }
        }
        
    }
}
