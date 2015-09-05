using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spider.OpenQuant.Strategies.Util;

namespace Spider.OpenQuant.Strategies.Entities
{
    public enum FlipFlopPair
    {
        None,

        [FlipFlopPairDescription("TNA", "TZA")]
        Triple_RUT_TNA_TZA,

        [FlipFlopPairDescription("UDOW", "SDOW")]
        Triple_DOW_UDOW_SDOW,

        [FlipFlopPairDescription("UPRO", "SPXU")]
        Triple_SPX_UPRO_SPXU,

        [FlipFlopPairDescription("TQQQ", "SQQQ")]
        Triple_QQQQ_TQQQ_SQQQ,

        [FlipFlopPairDescription("QLD", "QID")]
        Double_QQQ_QLD_QID
    }
}
