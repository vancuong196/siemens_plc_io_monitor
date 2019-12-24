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
    public class ControllerInfomationHolder
    {
        private static volatile ControllerInfomationHolder _instance;
        private static object syncRoot = new Object();
        private ObservableCollection<PLCModel> _listPLC;
        public ObservableCollection<PLCModel> ListController
        {
            set
            {
                _listPLC = value;
            }
            get
            {
                return _listPLC;
            }
        }
        public static ControllerInfomationHolder Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new ControllerInfomationHolder();
                        }
                    }
                }
                return _instance;
            }
        }
        private ControllerInfomationHolder()
        {

            ListController = new ObservableCollection<PLCModel>()
            {
                new PLCModel()
                {
                    Id = 1,
                    Name = "1500Main",
                    Address = "192.168.8.2",
                    Type = "1500",
                    CpuType = S7.Net.CpuType.S71500
                },
                   new PLCModel()
                {
                    Id = 2,
                    Name = "1200Auto",
                    Address = "192.168.8.3",
                    Type = "1200"
                    ,
                    CpuType = S7.Net.CpuType.S71200
                },
                      new PLCModel()
                {
                          Id = 3,
                    Name = "1200MES",
                    Address = "192.168.8.4",
                    Type = "1200",
                    CpuType = S7.Net.CpuType.S71200
                },
                         new PLCModel()
                {
                             Id = 4,
                    Name = "1200PB1",
                    Address = "192.168.8.5",
                    Type = "1200",
                    CpuType = S7.Net.CpuType.S71200
                },
                            new PLCModel()
                {
                                Id = 5,
                    Name = "1200PB2",
                    Address = "192.168.8.6",
                    Type = "1200",
                    CpuType = S7.Net.CpuType.S71200
                },
                                         new PLCModel()
                {
                                             Id = 6,
                    Name = "1200PB3",
                    Address = "192.168.8.7",
                    Type = "1200",
                    CpuType = S7.Net.CpuType.S71200
                }
                                         ,
                new PLCModel()
                {
                    Id = 7,
                    Name = "1200PB4",
                    Address = "192.168.8.8",
                    Type = "1200",
                    CpuType = S7.Net.CpuType.S71200
                },
                                                                   new PLCModel()
                {
                                                                       Id = 8,
                    Name = "1200PB5",
                    Address = "192.168.8.9",
                    Type = "1200",
                    CpuType = S7.Net.CpuType.S71200
                }
                                                                   ,
                                                                                new PLCModel()
                {
                                                                                    Id = 9,
                    Name = "1200PB6",
                    Address = "192.168.8.10",
                    Type = "1200",
                    CpuType = S7.Net.CpuType.S71200
                } };

        }

    }
}
