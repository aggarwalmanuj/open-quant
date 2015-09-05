using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using OpenQuant.API;
using Spider.OpenQuant.TradeClient5.Util;

namespace Spider.OpenQuant.TradeClient5.Base
{
    public abstract partial class BaseStrategy : Strategy
    {
        #region Properties

        #region Private/Protected Properties

        protected static readonly Logger Log = LogManager.GetCurrentClassLogger();

        protected DateTime StrategyBeginTime { get; set; }

        public DateTime CurrentValidityDateTime { get; set; }

        public DateTime CurrentStartOfSessionTime = DateTime.MaxValue;

        public DateTime CurrentEndOfSessionTime = DateTime.MaxValue;

        public DateTime CurrentLastSessionDate = DateTime.MaxValue;

        public int CurrentExecutionTimePeriodInSeconds = 120;

        protected bool IsStrategyInitialized = false;

        #endregion

        #endregion

        public override void OnStrategyStart()
        {
            try
            {
                LoggingUtility.WriteDebug(this, "Strategy initializing");

                StrategyBeginTime = DateTime.Now;

                // First setup the main objects
                SetAndValidateValues();

                SetupIndicators();

                SetupData();

                SetupQuoteClient();

                SetupTradeLegs();

                UpdateTradeLegsCompletedFlag();

                UpdateNextTradeLeg();

                IsStrategyInitialized = true;

                LoggingUtility.WriteHorizontalBreak(this);
                LoggingUtility.WriteInfo(this, "*** INITIALIZATION DONE ***");
                LoggingUtility.WriteHorizontalBreak(this, 2);
            }
            catch (Exception ex)
            {
                IsStrategyInitialized = false;
                LoggingUtility.WriteError(this, ex, "*** FAILED TO INITIALIZE STRATEGY ***");
            }
        }


        public override void OnStrategyStop()
        {
            LogManager.Flush();

            LoggingUtility.FlushLog();

            TearDownQuoteClient();

            base.OnStrategyStop();
        }

    }
}
