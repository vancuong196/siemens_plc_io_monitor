using Newtonsoft.Json;
using PLCDiagnostic.Event;
using PLCDiagnostic.Models;
using PLCDiagnostic.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLCDiagnostic.Data
{
    class IODataInformationHolder
    {
        private static volatile IODataInformationHolder _instance;
        private static object syncRoot = new Object();
        private ObservableCollection<PlcIO> _listIO;
        public ObservableCollection<PlcIO> ListIO
        {
            set
            {
                _listIO = value;
            }
            get
            {
                return _listIO;
            }
        }
        public static IODataInformationHolder Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new IODataInformationHolder();
                        }
                    }
                }
                return _instance;
            }
        }
        private IODataInformationHolder()
        {
            
            ListIO = new ObservableCollection<PlcIO>();
        }
       private void subscribeDataFromPLC()
        {
            ApplicationService.Instance.EventAggregatorService.GetEvent<DataReadEvent>().Subscribe(handleDataFromEvent);
        }
        private void handleDataFromEvent(PlcEventModel plcEventModel)
        {
            if (plcEventModel == null)
            {
                return;
            }
            var currentIO = ListIO.FirstOrDefault(io => io.Address.Equals(plcEventModel.Address));
            if (currentIO!=null)
            {
                currentIO.Value = plcEventModel.Data;
            }

        }
    }
}
