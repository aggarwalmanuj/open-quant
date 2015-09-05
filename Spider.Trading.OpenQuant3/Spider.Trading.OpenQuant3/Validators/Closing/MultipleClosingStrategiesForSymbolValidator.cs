using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQuant.API;
using Spider.Trading.OpenQuant3.Common;
using Spider.Trading.OpenQuant3.Strategies.Closing;
using Spider.Trading.OpenQuant3.Util;

namespace Spider.Trading.OpenQuant3.Validators.Closing
{
    internal static class MultipleClosingStrategiesForSymbolValidator
    {
       internal static void ValidateClosingSymbol(BaseClosingStrategy strategy)
       {
           if (strategy.IsCurrentInstrumentIwmAndIgnorable())
               return;

           string closingSymbolKey = string.Format(ClosingPositionConstants.CLOSING_SYMBOL_KEY,
                                                   strategy.Instrument.Symbol, strategy.Instrument.Type);

           if (!Strategy.Global.ContainsKey(closingSymbolKey))
           {
               lock(LockObjectManager.LockObject)
               {
                   if (!Strategy.Global.ContainsKey(closingSymbolKey))
                   {
                       Strategy.Global.Add(closingSymbolKey, closingSymbolKey);
                   }
               }
           }
           else
           {
               throw new ApplicationException(
                   string.Format(
                       "The symbol '{0}' is already attached to another closing strategy. You cannot run multiple closing strategies for the same symbol.",
                       strategy.Instrument.Symbol));
           }
   
       }
    }
}
