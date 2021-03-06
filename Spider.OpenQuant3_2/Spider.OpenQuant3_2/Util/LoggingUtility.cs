﻿using System;
using Spider.OpenQuant3_2.Base;

namespace Spider.OpenQuant3_2.Util
{
    public static  class LoggingUtility
    {
       

        internal static void WriteHorizontalBreak(BaseStrategy strategy)
        {
            WriteInfo(strategy, "----------------------------");
        }

        internal static void WriteInfo(BaseStrategy strategy, string message)
        {
            string prefix = string.Format("[{0} | {1}]", strategy.IbAccountNumber, strategy.Instrument.Symbol);
            string datePart = string.Format("[{0:dd-MMM-yyyy hh:mm:ss.fff tt}]", strategy.GetCurrentDateTime());
            const string formattedString = "{0} -- {1} -- {2}";
            Console.WriteLine(string.Format(formattedString, prefix, datePart, message));
        }

        internal static void WriteInfoFormat(BaseStrategy strategy, string formattedString, params object[] args)
        {
            WriteInfo(strategy, string.Format(formattedString, args));
        }

    }
}
