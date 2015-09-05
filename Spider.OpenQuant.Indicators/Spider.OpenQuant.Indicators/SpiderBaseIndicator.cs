using OpenQuant.API;
using OpenQuant.API.Plugins;

namespace Spider.OpenQuant.Indicators
{
    public class SpiderBaseIndicator : UserIndicator
    {
        private BarSeries _barSeriesInput;
        private long? _barSize;

        public SpiderBaseIndicator(BarSeries input) : base(input)
        {
            _barSeriesInput = input;
        }

        protected BarSeries BarSeriesInput
        {
            get
            {
                return _barSeriesInput;
            }
        }

        protected long GetBarSize()
        {
            if (null == BarSeriesInput || BarSeriesInput.Count <= 0)
                return 0;

            if (!_barSize.HasValue)
            {
                _barSize = BarSeriesInput.Last.Size;
            }

            return _barSize.Value;
        }

    }
}