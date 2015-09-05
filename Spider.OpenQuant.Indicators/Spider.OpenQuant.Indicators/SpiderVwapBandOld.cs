using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenQuant.API;
using OpenQuant.API.Indicators;
using OpenQuant.API.Plugins;

using Spider.OpenQuant.Indicators.Extensions;

namespace Spider.OpenQuant.Indicators
{
    [Obsolete]
    public class SpiderVwapBandOld : UserIndicator
    {
        private readonly double _factor;
        private readonly BarSeries _priceBarSeries;
        private readonly int _barSize;
        private readonly InstrumentType _instrumentType;

        private double _volumeSum;
        private double _volumeVwapSum;
        private double _volumeVwap2Sum;


        public SpiderVwapBandOld(ISeries input, double factor, BarSeries priceBarSeries, int barSize, InstrumentType instrumentType)
            : base(input)
        {
            _factor = factor;
            _priceBarSeries = priceBarSeries;
            _barSize = barSize;
            _instrumentType = instrumentType;
            this.Name = string.Format("SpiderVwapBand ({0:N4})", factor);
        }


        public override double Calculate(int index)
        {
            double vwap = this.Input[index, BarData.Close];
            double vwapSquared = vwap*vwap;
            double volume = 0;
            double lastClose = 0;

            if (null != this._priceBarSeries && this._priceBarSeries.Count > 0)
            {
                volume = this._priceBarSeries.Last.Volume*100;
                lastClose = this._priceBarSeries.Last.Close;
            }
            else
            {
                return double.NaN;
            }

            //Console.WriteLine(string.Format("VWAP: {0}, Volumen: {1}, LastClose: {2}", vwap, volume, lastClose));

            DateTime thisDateTime = Input.GetDateTime(index);

            if (index <= 0 || thisDateTime.IsSessionOpeningBar(_barSize, _instrumentType))
            {
                _volumeSum = volume;
                _volumeVwapSum = volume*vwap;
                _volumeVwap2Sum = volume*vwapSquared;
            }
            else
            {
                _volumeSum = _volumeSum + volume;
                _volumeVwapSum = _volumeVwapSum + (volume*vwap);
                _volumeVwap2Sum = _volumeVwap2Sum + (volume*vwapSquared);
            }


            double priceCalc = (_volumeSum > 0) ? (_volumeVwapSum/_volumeSum) : double.NaN;

            if (double.IsNaN(priceCalc))
                return double.NaN;

            double priceSquared = priceCalc*priceCalc;
            double devCalc = Math.Sqrt(Math.Max(_volumeVwap2Sum / _volumeSum - priceSquared, 0));

            return priceCalc + (_factor*devCalc);
        }
    }
}
