using System;
using OpenQuant.API;

namespace Spider.Trading.OpenQuant3.Diagnostics
{
    internal static class LoggingUtility
    {

        private const string DEBUG_STRING = "{0} -- {1:dd-MMM-yyyy h:mm:ss.fff tt}, (DEBUG) [{2}] - {3}";
        private const string VERBOSE_STRING = "{0} -- {1:dd-MMM-yyyy h:mm:ss.fff tt}, (VERBOSE) [{2}] - {3}";
        private const string WARN_STRING = "{0} -- {1:dd-MMM-yyyy h:mm:ss.fff tt}, (WARN) [{2}] - {3}";
        private const string INFO_STRING = "{0} -- {1:dd-MMM-yyyy h:mm:ss.fff tt}, (INFO) [{2}] - {3}";
        private const string ORDER_STRING = "{0} -- {1:dd-MMM-yyyy h:mm:ss.fff tt}, (ORDER - Retry #{4}) [{2}] - {3}";
        private const string ERR_STRING = "{0} -- {1:dd-MMM-yyyy h:mm:ss.fff tt}, (ERROR) [{2}] - {3}";


        internal static void LogOkToTriggerOrder(LoggingConfig config, Bar bar)
        {
            if (config.IsInfoEnabled)
            {
                string message = string.Format("The bar arrived within the trade trigger validity time @ bar: {0}", bar);
                Console.WriteLine();
                WriteInfo(config, message);
            }
        }



        internal static void LogCurrentBarArrival(LoggingConfig config, Bar bar)
        {
            if (config.IsVerboseEnabled)
            {
                string message = string.Format("Bar with begin time {0} arrived: {1}", bar.BeginTime, bar);
                WriteVerbose(config, message);
                WriteVerbose(config, "---------------------------");
            }
        }


        internal static void LogOrder(LoggingConfig config, string orderName, OrderSide orderSide, double qty, double price, int retryCount)
        {
            Console.WriteLine("---------------------------");
            string message = string.Format("{0} - {1} {2} @ {3} for {4:C}", orderName, orderSide, qty, price, qty * price);
            Console.WriteLine(string.Format(ORDER_STRING, config.IbAccountNumber, DateTime.Now, config.Instrument.Symbol, message, retryCount));
            Console.WriteLine("---------------------------");

        }

        internal static void WriteDebug(LoggingConfig config, string message)
        {
            if (config.IsDebugEnabled)
                Console.WriteLine(string.Format(DEBUG_STRING, config.IbAccountNumber, DateTime.Now, config.Instrument.Symbol, message));
        }

        internal static void WriteInfo(LoggingConfig config, string message)
        {
            if (config.IsInfoEnabled)
                Console.WriteLine(string.Format(INFO_STRING, config.IbAccountNumber, DateTime.Now, config.Instrument.Symbol, message));
        }

        internal static void WriteWarn(LoggingConfig config, string message)
        {
            if (config.IsWarnEnabled)
                Console.WriteLine(string.Format(WARN_STRING, config.IbAccountNumber, DateTime.Now, config.Instrument.Symbol, message));
        }

        internal static void WriteError(LoggingConfig config, string message)
        {
            Console.WriteLine(string.Format(ERR_STRING, config.IbAccountNumber, DateTime.Now, config.Instrument.Symbol, message));
        }




        internal static void WriteVerbose(LoggingConfig config, string message)
        {
            if (config.IsVerboseEnabled)
                Console.WriteLine(string.Format(VERBOSE_STRING, config.IbAccountNumber, DateTime.Now, config.Instrument.Symbol, message));
        }
    }
}