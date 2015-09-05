using NLog;
using NLog.Config;
using NLog.Targets;

using OpenQuant.API.Indicators;

namespace Spider.OpenQuant.TradeClient5.Solution
{
    public class LogConfigurator
    {
        public static void ConfigureViaCode(LogLevel consoleLogLevel, LogLevel fileLogLevel)
        {
            // Step 1. Create configuration object 
            var config = new LoggingConfiguration();

            // Step 2. Create targets and add them to the configuration 
            var consoleTarget = new ConsoleTarget();
            config.AddTarget("console", consoleTarget);

            var fileTarget = new FileTarget();
            config.AddTarget("file", fileTarget);

            // Step 3. Set target properties 
            consoleTarget.Layout = @"${message} ${onexception:EXCEPTION\:${exception:format=tostring}}";


            fileTarget.Layout = @"${date:format=yyyy-MM-dd HH\:mm\:ss.fff} [${level:uppercase=true}] ${message}  ${onexception:EXCEPTION\:${exception:format=tostring}}";
            fileTarget.FileName = "C:\\temp\\logs\\SpiderOpenQuant.${date:format=yyyy-MM-dd hh}.log";
            fileTarget.ConcurrentWrites = false;
            fileTarget.KeepFileOpen = true;
            fileTarget.OpenFileCacheTimeout = 60;


            // Step 4. Define rules
            var rule1 = new LoggingRule("*", consoleLogLevel, consoleTarget);
            config.LoggingRules.Add(rule1);

            var rule2 = new LoggingRule("*", fileLogLevel, fileTarget);
            config.LoggingRules.Add(rule2);

            // Step 5. Activate the configuration
            LogManager.Configuration = config;
        }


        public static void ConfigureViaCode()
        {
            ConfigureViaCode(LogLevel.Debug, LogLevel.Trace);

        }
    }
}