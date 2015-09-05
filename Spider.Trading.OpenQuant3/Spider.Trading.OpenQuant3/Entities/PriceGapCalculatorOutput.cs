using Spider.Trading.OpenQuant3.Enums;

namespace Spider.Trading.OpenQuant3.Entities
{
    public class PriceGapCalculatorOutput
    {
        public GapType GapType { get; set; }

        public double PriceGapAmount { get; set; }

        public double PriceGapAtr { get; set; }
    }
}