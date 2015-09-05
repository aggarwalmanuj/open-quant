using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenQuant.API;
using OpenQuant.API.Plugins;

using Spider.OpenQuant.Indicators.Extensions;

namespace Spider.OpenQuant.Indicators
{
    public class SpiderVwapBand : SpiderBaseIndicator
    {
        private readonly double _factor;
        private readonly InstrumentType _instrumentType;

        private double _volumeSum;
        private double _volumeVwapSum;
        private double _volumeVwap2Sum;

        private double _avgCumPrice = 0.0;
        private double _cumVolume = 0.0;


        public SpiderVwapBand(BarSeries input, double factor, InstrumentType instrumentType)
            : base(input)
        {
            _factor = factor;
            _instrumentType = instrumentType;
            this.Name = string.Format("SpiderVwapBand ({0:N4})", factor);
        }



        public override double Calculate(int index)
        {


            DateTime thisDateTime = Input.GetDateTime(index);

            if (index <= 0 || thisDateTime.IsSessionOpeningBar(GetBarSize(), _instrumentType))
            {
                _avgCumPrice = 0;
                _cumVolume = 0;
            }


            double volume = Input[index, BarData.Volume]*100;
            double avgPrice = Input[index, BarData.Close];

            _avgCumPrice += (avgPrice*volume);
            _cumVolume += volume;

            double vwap = (_cumVolume > 0) ? _avgCumPrice/_cumVolume : double.NaN;
            if (double.IsNaN(vwap))
                return double.NaN;

            double vwapSquared = vwap*vwap;

            //Console.WriteLine(string.Format("VWAP: {0}, Volumen: {1}, LastClose: {2}", vwap, volume, lastClose));

            if (index <= 0 || thisDateTime.IsSessionOpeningBar(GetBarSize(), _instrumentType))
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
            double devCalc = Math.Sqrt(Math.Max(_volumeVwap2Sum/_volumeSum - priceSquared, 0));

            return priceCalc + (_factor*devCalc);
        }
    }
}
