using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider.Trading.OpenQuant3.Common
{
    public class PeriodConstants
    {
        public const int PERIOD_MINUTE = 60;
        public const int PERIOD_DAILY = 86400;
    }

    public class PstSessionTimeConstants
    {
        public const int StockExchangeStartTimeSeconds = 23400;
        public const int StockExchangeEndTimeSeconds = 46800;
    }


    public class MinNumberOfBars
    {
        public const int MinuteBars = 1600;
        public const int DailyBars = 55;
    }

    public class IwmStopTriggerConstants
    {
        public const string IwmSymbol = "IWM";
        public const string IwmStopPriceMetForBuySide = "Iwm.StopPrice.BUY";
        public const string IwmStopPriceMetForSellSide = "Iwm.StopPrice.SELL";
    }

    public class GlobalStorageKeys
    {
        public const string GLOBAL_POS_SIZE_HOLDER = "Global.GLOBAL_POS_SIZE_HOLDER";
        public const string GLOBAL_STOP_HOLDER = "Global.GLOBAL_STOP_HOLDER";
    }

    public class ClosingPositionConstants
    {
        public const string CLOSING_SYMBOL_KEY = "CLOSING.{0}.{1}";
    }

    public class PortfolioManagementConstants
    {
        public const string PORTFOLIO_POSITION_TABLE = "PORTFOLIO_POSITION_TABLE";
        public const string PORTFOLIO_POSITION_KEY = "PORTFOLIO_POSITION.{0}.{1}";
        public const string PORTFOLIO_POSITION_STOP_LOSS_MET_KEY = "PORTFOLIO_POSITION_STOP_LOSS_MET_KEY.{0}.{1}";
        public const string CURRENT_PORTFOLIO_PURCHASE_VALUE_KEY = "CURRENT_PORTFOLIO_PURCHASE_VALUE_KEY";
        public const string CURRENT_PORTFOLIO_CURRENT_VALUE_KEY = "CURRENT_PORTFOLIO_CURRENT_VALUE_KEY";
        public const string CURRENT_PORTFOLIO_PROFIT_LOSS_AMOUNT_KEY = "CURRENT_PORTFOLIO_PROFIT_LOSS_AMOUNT_KEY";
        public const string CURRENT_PORTFOLIO_PROFIT_LOSS_PERC_KEY = "CURRENT_PORTFOLIO_PROFIT_LOSS_PERC_KEY";
        public const string CURRENT_PORTFOLIO_STOP_LOSS_MET_KEY = "CURRENT_PORTFOLIO_STOP_LOSS_MET_KEY";
        
    }
}
