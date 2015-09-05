using System;

using OpenQuant.API;

namespace Spider.OpenQuant.TradeClient5.Extensions
{
    public static class QuoteExtenstions
    {
        public static bool IsWithinRegularTradingHours(this Quote quote, InstrumentType insType)
        {
            if (insType != InstrumentType.Stock)
                throw new NotImplementedException("Not implemented for anything other than stocks");



            return quote.DateTime.IsWithinRegularTradingHours(insType);
        }
    }
}