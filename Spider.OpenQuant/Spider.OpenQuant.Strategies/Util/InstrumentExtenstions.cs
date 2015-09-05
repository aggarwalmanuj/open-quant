using OpenQuant.API;

namespace Spider.OpenQuant.Strategies.Util
{
    static class InstrumentExtenstions
    {
        public static string ToIdentifier(this Instrument ins)
        {
            return string.Format("{0}:{1}:{2}", ins.Symbol, ins.Exchange, ins.Currency);
        }
    }
}