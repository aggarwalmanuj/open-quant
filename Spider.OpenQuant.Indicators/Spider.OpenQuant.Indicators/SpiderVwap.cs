using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenQuant.API;
using OpenQuant.API.Plugins;

using Spider.OpenQuant.Indicators.Extensions;

namespace Spider.OpenQuant.Indicators
{
    public class SpiderVwap : SpiderBaseIndicator
    {
        private readonly InstrumentType _instrumentType;
        private double _avgCumPrice = 0.0;
        private double _cumVolume = 0.0;

        public SpiderVwap(BarSeries input, InstrumentType instrumentType)
            : base(input)
        {
            _instrumentType = instrumentType;
            this.Name = "SpiderVwap";
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
            //double avgPrice = Input[index, BarData.Close];
            //double avgPrice = Input[index, BarData.Close];


            _avgCumPrice += (avgPrice*volume);
            _cumVolume += volume;

            return (_cumVolume > 0) ? Math.Round(_avgCumPrice/_cumVolume, 2) : double.NaN;
        }
    }
}
