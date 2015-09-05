using OpenQuant.API;

namespace Spider.Trading.OpenQuant3.Entities
{
    public class PriceCalculatorInput
    {

        public Bar CurrentBar { get; set; }

        public double Atr { get; set; }

        public OrderSide OrderSide { get; set; }

        public BaseStrategy Strategy { get; set; }

        public Instrument Instrument { get; set; }
    }
}
