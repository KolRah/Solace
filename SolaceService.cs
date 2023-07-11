/*
     I'm not adding the full code here, the general part about context initialization is well documented in the Solace GitHub repos
     https://github.com/SolaceSamples/solace-samples-dotnet/tree/master
*/


public List<string> BrowseQueue(string queuename)
        {
            try
            {   
                List<string> RESULT = new List<string>();

                ContextFactoryProperties cfp = new ContextFactoryProperties()
                {
                    SolClientLogLevel = SolLogLevel.Warning
                };
                cfp.LogToConsoleError();
                _instance = ContextFactory.Instance;
                _instance.Init(cfp);

                using (IContext context = _instance.CreateContext(new ContextProperties(), null))
                {
                    QueueBrowser browser = new QueueBrowser()
                    {
                        VPNName = _parameters._VPN_NAME,
                        UserName = _parameters._USERNAME,
                        Password = _parameters._PASSWORD
                    };
                    RESULT = browser.Run(queuename,context, _parameters._HOST);
                }
                _instance.Cleanup();
                return RESULT;
            }
            catch (Exception ex)
            {
                _instance.Cleanup();
                Console.WriteLine("Exception thrown: {0}", ex.Message);
                throw new SolaceConnectionException();
            }
            Console.WriteLine("Finished.");
            
        }
    }
