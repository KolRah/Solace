using System.Text;
using SolaceSystems.Solclient.Messaging;
using ISession = SolaceSystems.Solclient.Messaging.ISession;

namespace QueueBrowserExample.Solace
{
    public class QueueBrowser
    {
        public string VPNName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        const int DefaultReconnectRetries = 3;

        public List<string> Run(string queuename,IContext context, string host)
        {
            // Validate parameters
            if (context == null)
            {
                throw new ArgumentException("Solace Systems API context Router must be not null.", "context");
            }
            if (string.IsNullOrWhiteSpace(host))
            {
                throw new ArgumentException("Solace Messaging Router host name must be non-empty.", "host");
            }
            if (string.IsNullOrWhiteSpace(VPNName))
            {
                throw new InvalidOperationException("VPN name must be non-empty.");
            }
            if (string.IsNullOrWhiteSpace(UserName))
            {
                throw new InvalidOperationException("Client username must be non-empty.");
            }

            // Create session properties
            SessionProperties sessionProps = new SessionProperties()
            {
                Host = host,
                VPNName = VPNName,
                UserName = UserName,
                Password = Password,
                ReconnectRetries = DefaultReconnectRetries
            };

            Console.WriteLine("Connecting as {0}@{1} on {2}...", UserName, VPNName, host);
            try
            { 
                using (ISession session = context.CreateSession(sessionProps, null, null))
                {
                    ReturnCode returnCode = session.Connect();
                    if (returnCode == ReturnCode.SOLCLIENT_OK)
                    {
                        Console.WriteLine("Session successfully connected.");
                        return BrowseQueue(queuename,session);
                    }
                    else
                    {
                        throw new SolaceConnectionException($"Error connecting, return code: {returnCode}");
                        Console.WriteLine($"Error connecting, return code: {returnCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw ;
            }
        }
        private List<string> BrowseQueue(string queuename,ISession session)
        {
            try
            {
                List<string> RESULT = new List<string>();
                BrowserProperties browserProperties = new BrowserProperties
                {
                    TransportWindowSize = 255,
                    WaitTimeout = 1000,
                };
                IQueue q = ContextFactory.Instance.CreateQueue(queuename);
                IBrowser browser = session.CreateBrowser(q,browserProperties);
                IMessage message;
                do
                {
                    message = browser.GetNext();
                    try { 
                        var bin = message.BinaryAttachment;
                        if (bin !=null)
                        { 
                            string messageContent = Encoding.UTF8.GetString(bin);
                            RESULT.Add(messageContent);
                        }
                    } 
                    catch (Exception)
                    { 
                        // sometimes the BinaryAttachment property fails with an Exception
                        // add your personal extra handling here
                    }
                } while (message != null);
                return RESULT;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
