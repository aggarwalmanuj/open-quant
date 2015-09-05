using System;

using EasyNetQ.Management.Client;

using Spider.OpenQuant.TradeClient5.Util;

namespace Spider.OpenQuant.TradeClient5.QuoteServer
{
    public class QueueInitializer
    {
        private static readonly object lockObject = new object();
        private static readonly QueueInitializer singleton = new QueueInitializer();


        private QueueInitializer()
        {

        }

        public static QueueInitializer Instance
        {
            get
            {
                return singleton;
            }
        }


        public void InitializeQueues(string hostUrl, string username, string password, string queueNamePattern)
        {

            lock (lockObject)
            {

                var management = new ManagementClient(hostUrl, username, password);
                var queues = management.GetQueues();

                foreach (var queue in queues)
                {
                    if (queue.Name.IndexOf(queueNamePattern, StringComparison.InvariantCultureIgnoreCase) > -1)
                    {
                        management.Purge(queue);
                        management.DeleteQueue(queue);
                    }
                }


            }
        }

        public void InitializeQueues(string hostUrl, string username, string password)
        {
            InitializeQueues(hostUrl, username, password, EnvironmentManager.GetNormalizedMachineName());
        }


        public void PurgeQueues(string hostUrl, string username, string password)
        {

            lock (lockObject)
            {

                string lookupString = "QuoteServer.On";
                var management = new ManagementClient(hostUrl, username, password);
                var queues = management.GetQueues();

                foreach (var queue in queues)
                {
                    if (queue.Name.StartsWith(lookupString, StringComparison.InvariantCultureIgnoreCase))
                    {
                        management.Purge(queue);
                    }
                }


            }
        }
    }
}
