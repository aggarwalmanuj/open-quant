using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQuant.API;
using Spider.Trading.OpenQuant3.Calculations;
using Spider.Trading.OpenQuant3.Diagnostics;
using Spider.Trading.OpenQuant3.Util;
using Spider.Trading.OpenQuant3.Validators;

namespace Spider.Trading.OpenQuant3
{
    public abstract partial class BaseStrategy
    {
        
        public bool IsInitialized
        {
            get;
            protected set;
        }

        public bool IsReInitialized
        {
            get;
            protected set;
        }

        public bool IsPortfolioStopLossEnabled
        {
            get { return OrderTriggerStrategy == Enums.OrderTriggerStrategy.PortfolioLossPercentageTrigger; }
        }

        public bool IsPositionStopLossEnabled
        {
            get { return OrderTriggerStrategy == Enums.OrderTriggerStrategy.DailyLossPercentageTrigger; }
        }

        public bool IsPortfolioOrPositionStopLossEnabled
        {
            get {return  IsPortfolioStopLossEnabled || IsPositionStopLossEnabled; }
        }

        public double EffectiveAtrAsPercentageOfLastClosePrice
        {
            get
            {
                if (EffectivePreviousClosePrice != 0)
                    return (EffectiveAtrPrice/EffectivePreviousClosePrice);
                return 0;
            }
        }

        

        #region Util Object Accessors

        




        public LoggingConfig LoggingConfig
        {
            get
            {
                if (null == logConf)
                {
                    lock (LockObjectManager.LockObject)
                    {
                        if (null == logConf)
                        {
                            logConf = new LoggingConfig()
                            {
                                IsDebugEnabled = IsLogDebugEnabled,
                                IsWarnEnabled = IsLogWarnEnabled,
                                IsInfoEnabled = IsLogInfoEnabled,
                                IsVerboseEnabled = IsLogVerboseEnabled,
                                Instrument = Instrument,
                                IbAccountNumber =  IbBrokerAccountNumber
                            };
                        }
                    }
                }
                return logConf;
            }
        }


        public StopPriceTriggerCalculator StopPriceTriggerCalculator
        {
            get
            {
                if (null == stopPriceTriggerCalc)
                {
                    lock (LockObjectManager.LockObject)
                    {
                        if (null == stopPriceTriggerCalc)
                        {
                            stopPriceTriggerCalc = new StopPriceTriggerCalculator(LoggingConfig);
                        }
                    }
                }

                return stopPriceTriggerCalc;
            }
        }
        #endregion
    }
}
