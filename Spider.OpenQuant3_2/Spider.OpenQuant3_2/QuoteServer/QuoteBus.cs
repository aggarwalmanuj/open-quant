using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EasyNetQ;

namespace Spider.OpenQuant3_2.QuoteServer
{
    public class QuoteBus2
    {
        private static string connString = "host=miami.spider.local;username=manuj;password=manuj";
        private static bool isDisposed = false;

        static Lazy<IBus> _bus = new Lazy<IBus>(() => RabbitHutch.CreateBus(connString));


        private QuoteBus2()
        {
            
        }

        public static void SetConnString(string conn)
        {
            if (string.Compare(conn, connString, StringComparison.InvariantCultureIgnoreCase) == 0)
                return;

            connString = conn;

            if (_bus.IsValueCreated)
                Dispose();

            _bus = new Lazy<IBus>(() => RabbitHutch.CreateBus(connString));
        }

        public static IBus Bus
        {
            get
            {
                if (!_bus.IsValueCreated)
                    isDisposed = false;

                return _bus.Value;
            }
        }

        public static void Dispose()
        {
            if (!isDisposed)
            {

                try
                {
                    Bus.Dispose();
                    isDisposed = true;
                }
                catch
                {
                }
                finally
                {
                    _bus = new Lazy<IBus>(() => RabbitHutch.CreateBus(connString));
                }
            }
        }
    }
}
