using OpenQuant.API;

namespace Spider.OpenQuant.TradeClient5.Entities
{
    public class SpiderInstrument
    {
        public string Symbol { get; set; }

        public InstrumentType InstrumentType { get; set; }
    }
}