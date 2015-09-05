using System;
using System.Linq;

namespace Spider.Trading.OpenQuant3.Validators
{
    internal static class SlippageValidator
    {
        internal static void SetAndValidateValue(BaseStrategy strategy)
        {
            if (strategy.MinAllowedSlippage > strategy.MaxAllowedSlippage)
                throw new ArithmeticException(
                    string.Format("Min Allowed Slippage {0} cannot be more than Max Allowed Slippage {1}",
                                  strategy.MinAllowedSlippage, strategy.MaxAllowedSlippage));

            double[] arr = new double[] { strategy.MaxAllowedSlippage, strategy.MinAllowedSlippage };
            double minSlip = arr.Min();
            double maxSlip = arr.Max();
            strategy.EffectiveMaxAllowedSlippage = maxSlip;
            strategy.EffectiveMinAllowedSlippage = minSlip;
        }
    }
}