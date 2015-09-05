using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQuant.API;

namespace Spider.Trading.OpenQuant3.Diagnostics
{
    public class LoggingConfig
    {
        public bool IsDebugEnabled { get; set; }

        public bool IsWarnEnabled { get; set; }

        public bool IsInfoEnabled { get; set; }

        public bool IsVerboseEnabled { get; set; }

        public Instrument Instrument { get; set; }


        public string IbAccountNumber { get; set; }

    }
}
