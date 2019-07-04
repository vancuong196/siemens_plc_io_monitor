using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLCDiagnostic.Services
{
    class ApplicationService
    {
        private static volatile ApplicationService _instance;
        private static object syncRoot = new Object();
        private EventAggregator eventAggregator;
        public static ApplicationService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new ApplicationService();
                        }
                    }
                }
                return _instance;
            }
        }
        private ApplicationService()
        {
            eventAggregator = new EventAggregator();
        }
        public EventAggregator EventAggregatorService
        {
            get
            {
                return eventAggregator;
            }
        }
    }
}
