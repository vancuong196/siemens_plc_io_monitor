using PLCDiagnostic.Data;
using PLCDiagnostic.Event;
using PLCDiagnostic.Models;
using PLCDiagnostic.Services;
using S7.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PLCDiagnostic.PLC
{
    public class PLCMachine
    {
        public string Address { set; get; }
        private Plc plc;
        public bool IsConnected {set; get;}

        public CpuType PlcCpuType {set; get;}

        public PLCMachine(String address, CpuType type)
        {
            ListIO = new List<PlcIO>();
            Address = address;
            PlcCpuType = type;
        }
        public List<PlcIO> ListIO
        {
            set; get;
        }
        
        private Thread ReadIOThread;
        public bool Start()
        {
            if (PlcCpuType==null|| String.IsNullOrEmpty(Address))
            {
                throw new ArgumentNullException();
            }
            plc = new Plc(PlcCpuType, Address, 0, 0);
            try
            {
                plc.Open();
            } catch
            {
                return false;
            }
            
            
            if (plc.IsConnected)
            {
                ReadIOThread = new Thread(Run);
                ReadIOThread.Start();
                return true;
            } else
            {
                return false;
            }
            
        }
        public void Stop()
        {
            if (ReadIOThread!=null)
            {
                try
                {
                    ReadIOThread.Abort();
                }catch
                {

                }
            }
        }
        private void Run()
        {
            try
            {
                while (true)
                {

                    lock (ListIO)
                    {

                        foreach (var io in ListIO)
                        {
                            switch (io.Type)
                            {
                                case IOTypeConstants.BOOL_TYPE:
                                    string boolValue = "N/A";
                                    try
                                    {
                                         boolValue = ((bool)plc.Read(io.Address)).ToString();

                                    } catch
                                    {
                                        boolValue = "ERR";
                                    }
                                    Console.WriteLine(io.Address + " " + boolValue);
                                    if (string.IsNullOrEmpty(io.Value))
                                    {
                                        var model = new ValueChangedModel();
                                        model.Address = io.Address;
                                        model.Name = io.Name;
                                        model.NewValue = boolValue;
                                        model.OldValue = "N/A";
                                        ApplicationService.Instance.EventAggregatorService.GetEvent<ValueChangedEvent>().Publish(model);
                                    }
                                    else if (!boolValue.Equals(io.Value))
                                    {
                                        var model = new ValueChangedModel();
                                        model.Address = io.Address;
                                        model.Name = io.Name;
                                        model.NewValue = boolValue;
                                        model.OldValue = io.Value;
                                        ApplicationService.Instance.EventAggregatorService.GetEvent<ValueChangedEvent>().Publish(model);
                                    }
                                    io.Value = boolValue;
                                    break;
                                case IOTypeConstants.INT_TYPE:
                                    string intValue = "N/A";
                                    try
                                    {
                                        intValue = ((uint)plc.Read(io.Address)).ConvertToInt().ToString();

                                    }
                                    catch
                                    {
                                        intValue = "ERR";
                                    }
                                   // var intValue = ((uint)plc.Read(io.Address)).ConvertToInt().ToString();
                                   // Console.WriteLine(io.Name + " " + intValue);

                                    if (string.IsNullOrEmpty(io.Value))
                                    {
                                        var model = new ValueChangedModel();
                                        model.Address = io.Address;
                                        model.Name = io.Name;
                                        model.NewValue = intValue;
                                        model.OldValue = "N/A";
                                        ApplicationService.Instance.EventAggregatorService.GetEvent<ValueChangedEvent>().Publish(model);
                                    }
                                    else if (!intValue.Equals(io.Value))
                                    {
                                        var model = new ValueChangedModel();
                                        model.Address = io.Address;
                                        model.Name = io.Name;
                                        model.NewValue = intValue;
                                        model.OldValue = io.Value;
                                        ApplicationService.Instance.EventAggregatorService.GetEvent<ValueChangedEvent>().Publish(model);
                                    }
                                    io.Value = intValue;
                                    Console.WriteLine(io.Address + " " + intValue);
                                    break;
                                case IOTypeConstants.FLOAT_TYPE:
                                  //  var loatValue = ((uint)plc.Read(io.Address)).ConvertToFloat().ToString("0.000");
                                   // Console.WriteLine(io.Name + " " + loatValue);
                                    string loatValue = "N/A";
                                    try
                                    {
                                        loatValue = ((uint)plc.Read(io.Address)).ConvertToFloat().ToString("0.000");

                                    }
                                    catch
                                    {
                                        loatValue = "ERR";
                                    }
                                    if (string.IsNullOrEmpty(io.Value))
                                    {
                                        var model = new ValueChangedModel();
                                        model.Address = io.Address;
                                        model.Name = io.Name;
                                        model.NewValue = loatValue;
                                        model.OldValue = "N/A";
                                        ApplicationService.Instance.EventAggregatorService.GetEvent<ValueChangedEvent>().Publish(model);
                                    }
                                    else if (!loatValue.Equals(io.Value))
                                    {
                                        var model = new ValueChangedModel();
                                        model.Address = io.Address;
                                        model.Name = io.Name;
                                        model.NewValue = loatValue;
                                        model.OldValue = io.Value;
                                        ApplicationService.Instance.EventAggregatorService.GetEvent<ValueChangedEvent>().Publish(model);
                                    }
                                    io.Value = loatValue;
                                    Console.WriteLine(io.Address + " " + loatValue);
                                    break;
                                case IOTypeConstants.BYTE_TYPE:
                                    //  var loatValue = ((uint)plc.Read(io.Address)).ConvertToFloat().ToString("0.000");
                                    // Console.WriteLine(io.Name + " " + loatValue);
                                    string byteValue = "N/A";
                                    try
                                    {
                                        byteValue = ((byte)plc.Read(io.Address)).ToString();

                                    }
                                    catch
                                    {
                                        byteValue = "ERR";
                                    }
                                    if (string.IsNullOrEmpty(io.Value))
                                    {
                                        var model = new ValueChangedModel();
                                        model.Address = io.Address;
                                        model.Name = io.Name;
                                        model.NewValue = byteValue;
                                        model.OldValue = "N/A";
                                        ApplicationService.Instance.EventAggregatorService.GetEvent<ValueChangedEvent>().Publish(model);
                                    }
                                    else if (!byteValue.Equals(io.Value))
                                    {
                                        var model = new ValueChangedModel();
                                        model.Address = io.Address;
                                        model.Name = io.Name;
                                        model.NewValue = byteValue;
                                        model.OldValue = io.Value;
                                        ApplicationService.Instance.EventAggregatorService.GetEvent<ValueChangedEvent>().Publish(model);
                                    }
                                    io.Value = byteValue;
                                    Console.WriteLine(io.Address + " " + byteValue);
                                    break;
                            }
                        }
                    }
                    Thread.Sleep(10);
                }
            } catch
            {

            }
            
        }
    }
}
