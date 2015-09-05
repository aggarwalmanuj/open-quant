using OpenQuant.API;

namespace Spider.Trading.OpenQuant3.Entities
{
    public class PriceGapCalculatorInput
    {

        public Bar CurrentBar { get; set; }

        public Bar PreviousBar { get; set; }

        public double Atr { get; set; }

        public OrderSide OrderSide { get; set; }

        public double UnfavorableGapAllowedSlippage { get; set; }

        public double UnfavorableGap { get; set; }

        public double FavorableGapAllowedSlippage { get; set; }

        public double FavorableGap { get; set; }


    }
}