﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spider.OpenQuant4.Common;

namespace Spider.OpenQuant4.Util
{
    public static class BarNameManager
    {
        public static string GetBarName(long barSize)
        {
            if (barSize == PeriodConstants.PERIOD_DAILY)
                return "01D";
            if (barSize == PeriodConstants.PERIOD_MINUTE)
                return "01M";
            if (barSize == PeriodConstants.PERIOD_2_MINUTE)
                return "02M";
            return "01D";

        }
    }
}
