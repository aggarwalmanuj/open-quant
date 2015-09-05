using System;
using OpenQuant.API;

namespace Spider.OpenQuant.Strategies.Util
{
    internal static class LoggingUtility
    {

        private const string DEBUG_STRING = "{0:dd-MMM-yyyy h:mm:ss.FFFF tt}, (DEBUG) [{1}] - {2}";
        private const string WARN_STRING = "{0:dd-MMM-yyyy h:mm:ss.FFFF tt}, (WARN) [{1}] - {2}";
        private const string INFO_STRING = "{0:dd-MMM-yyyy h:mm:ss.FFFF tt}, (INFO) [{1}] - {2}";
        private const string ORDER_STRING = "{0:dd-MMM-yyyy h:mm:ss.FFFF tt}, (ORDER - Retry #{3}) [{1}] - {2}";
        private const string ERR_STRING = "{0:dd-MMM-yyyy h:mm:ss.FFFF tt}, (ERROR) [{1}] - {2}";

        internal static void LogOrderQueued(LoggingConfig config, string orderStyle, string symbol, DateTime time)
        {
            if (config.IsInfoEnabled)
            {
                string message = string.Format("{0} order queued for {1} @ {2}", orderStyle, symbol, time);
                WriteInfo(config, message);
            }
        }

        internal static void LogStopOrderQueued(LoggingConfig config, string orderStyle, string symbol, DateTime time, double stopPrice)
        {
            if (config.IsInfoEnabled)
            {
                string message = string.Format("{0} stop order queued for {1} @ {2} at a stop price of {3:c}", orderStyle, symbol, time, stopPrice);
                WriteInfo(config, message);
            }
        }

        internal static void LogOkToTriggerOrder(LoggingConfig config, Bar bar)
        {
            if (config.IsInfoEnabled)
            {
                string message = string.Format("It is OK to trigger the trade @ bar: {0}", bar);
                Console.WriteLine();
                WriteInfo(config, message);
            }
        }



        internal static void LogCurrentBarArrival(LoggingConfig config, Bar bar)
        {
            if (config.IsDebugEnabled)
            {
                string message = string.Format("Bar with begin time {0} arrived: {1}", bar.BeginTime, bar);
                WriteDebug(config, message);
            }
        }

        internal static void LogNoStopOrderFound(LoggingConfig config)
        {
            if (config.IsWarnEnabled)
            {
                WriteWarn(config, "No STOP price has been defined");
            }
        }

        internal static void LogRetryOrder(LoggingConfig config, Bar bar, int retryNumber)
        {
            Console.WriteLine("---------------------------");
            string message = string.Format("RETRYING THE ORDER. RETRY #{0} @ Bar: {1}", retryNumber, bar);
            WriteWarn(config, message);
            Console.WriteLine("---------------------------");
        }

        internal static void LogOrder(LoggingConfig config, string orderName, OrderSide orderSide, double qty, double price, int retryCount)
        {
            Console.WriteLine("---------------------------");
            string message = string.Format("{0} - {1} {2} @ {3} for {4:C}", orderName, orderSide, qty, price, qty * price);
            Console.WriteLine(string.Format(ORDER_STRING, DateTime.Now, config.Instrument.Symbol, message, retryCount));
            Console.WriteLine("---------------------------");

        }

        internal static void WriteDebug(LoggingConfig config, string message)
        {
            if (config.IsDebugEnabled)
                Console.WriteLine(string.Format(DEBUG_STRING, DateTime.Now, config.Instrument.Symbol, message));
        }

        internal static void WriteInfo(LoggingConfig config, string message)
        {
            if (config.IsInfoEnabled)
                Console.WriteLine(string.Format(INFO_STRING, DateTime.Now, config.Instrument.Symbol, message));
        }

        internal static void WriteWarn(LoggingConfig config, string message)
        {
            if (config.IsWarnEnabled)
                Console.WriteLine(string.Format(WARN_STRING, DateTime.Now, config.Instrument.Symbol, message));
        }

        internal static void WriteError(LoggingConfig config, string message)
        {
            Console.WriteLine(string.Format(ERR_STRING, DateTime.Now, config.Instrument.Symbol, message));
        }


    }


    public class LoggingConfig
    {
        public bool IsDebugEnabled { get; set; }

        public bool IsWarnEnabled { get; set; }

        public bool IsInfoEnabled { get; set; }

        public Instrument Instrument { get; set; }
    }
}
