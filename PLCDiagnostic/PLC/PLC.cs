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
        public bool WriteIOToPLC(PlcIO io)
        {
            try
            {
                object valueToWrite = null;
                switch (io.Type)
                {
                    case IOTypeConstants.BOOL_TYPE:
                        if (io.WriteValue == "True")
                        {
                            valueToWrite = true;
                        }
                        else
                        {
                            valueToWrite = false;
                        }
                        break;
                    case IOTypeConstants.BYTE_TYPE:
                        valueToWrite = byte.Parse(io.WriteValue);
                        break;
                    case IOTypeConstants.INT_TYPE:
                        valueToWrite = int.Parse(io.WriteValue);
                        break;
                    case IOTypeConstants.FLOAT_TYPE:
                        valueToWrite = float.Parse(io.WriteValue);

                        break;
                    case IOTypeConstants.WORD_TYPE:
                        valueToWrite = short.Parse(io.WriteValue);

                        break;
                }
                plc.Write(io.Address, valueToWrite);
                return true;
            } catch
            {
                return false;
            }
        }
        private Thread ReadIOThread;
        public async Task<bool> Start()
        {
            if (String.IsNullOrEmpty(Address))
            {
                throw new ArgumentNullException();
            }
            plc = new Plc(PlcCpuType, Address, 0, 0);
           
            try
            {
                 await plc.OpenAsync();
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

                   // lock (ListIO)
                   // {

                        foreach (var io in ListIO)
                        {
                           
                            switch (io.Type)
                            {
                                case IOTypeConstants.WORD_TYPE:
                                    string wordValue = "N/A";
                                    try
                                    {
                                        if (io.Address.StartsWith("QW"))
                                        {
                                            var add = io.Address.Substring(2);
                                            var startByte = int.Parse(add);
                                            wordValue = (plc.Read(DataType.Output, 0, startByte, VarType.Word, 1)).ToString();

                                        }
                                        else
                                        {
                                            wordValue = ((bool)plc.Read(io.Address)).ToString();
                                        }
                                    }
                                    catch
                                    {
                                        wordValue = "Parsing ERR";
                                    }

                                    Console.WriteLine(io.Address + " " + wordValue);
                                    if (string.IsNullOrEmpty(io.Value))
                                    {
                                        var model = new ValueChangedModel();
                                        model.Address = io.Address;
                                        model.Name = io.Name;
                                        model.NewValue = wordValue;
                                        model.OldValue = "N/A";
                                        ApplicationService.Instance.EventAggregatorService.GetEvent<ValueChangedEvent>().Publish(model);
                                    }
                                    else if (!wordValue.Equals(io.Value))
                                    {
                                        var model = new ValueChangedModel();
                                        model.Address = io.Address;
                                        model.Name = io.Name;
                                        model.NewValue = wordValue;
                                        model.OldValue = io.Value;
                                        ApplicationService.Instance.EventAggregatorService.GetEvent<ValueChangedEvent>().Publish(model);
                                    }
                                    io.Value = wordValue;
                                    break;
                                case IOTypeConstants.BOOL_TYPE:
                                    string boolValue = "N/A";
                                    try
                                    {
                                        if (io.Address.StartsWith("M"))
                                        {
                                            var add = io.Address.Substring(1, io.Address.Length - 1);
                                            var parts = add.Split('.');
                                            var startByte = int.Parse(parts[0]);
                                            var startBit = byte.Parse(parts[1]);
                                            boolValue = ((bool) plc.Read(DataType.Memory, 0, startByte, VarType.Bit, 1, startBit)).ToString() ;
                                           
                                        } else
                                        {
                                            boolValue = ((bool)plc.Read(io.Address)).ToString();
                                        }
                                    } catch
                                    {
                                        boolValue = "Parsing ERR";
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
                                        if (io.Address.StartsWith("MD"))
                                        {
                                            var add = io.Address.Substring(2, io.Address.Length - 2);
                                            var startByte = int.Parse(add);
                                            var x = plc.Read(DataType.Memory, 0, startByte, VarType.DInt, 1);
                                            intValue = (plc.Read(DataType.Memory, 0, startByte, VarType.DInt, 1)).ToString();
                                         
                                        } else
                                        {
                                            intValue = ((uint) plc.Read(io.Address)).ConvertToInt().ToString();
                                        }
                                    }
                                    catch
                                    {
                                        intValue = "Parsing ERR";
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
                                    string loatValue = "N/A";
                                    try
                                    {
                                        if (io.Address.StartsWith("M"))
                                        {
                                            var add = io.Address.Substring(2, io.Address.Length - 1);
                                            var startByte = int.Parse(add);
                                            loatValue = (plc.Read(DataType.Memory, 0, startByte, VarType.Real, 1)).ToString();
                                           
                                        } else
                                        {
                                        loatValue = ((uint)plc.Read(io.Address)).ConvertToFloat().ToString();
                                        }
                                    }
                                    catch
                                    {
                                        loatValue = "Parsing ERR";
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
                                        if (io.Address.StartsWith("MB"))
                                        {
                                            var add = io.Address.Substring(2, io.Address.Length - 2);
                                            var startByte = int.Parse(add);
                                            byteValue = ((byte)plc.Read(DataType.Memory, 0, startByte, VarType.Byte, 1)).ToString();
                                      
                                        } else
                                        {
                                            byteValue = ((byte)plc.Read(io.Address)).ToString();
                                        }
                                    }
                                    catch
                                    {
                                        byteValue = "Parsing ERR";
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
                   // }
                    Thread.Sleep(20);
                }
            } catch (ThreadAbortException e)
            {

            } catch (Exception e)
            {

            }
            
        }
    }
}
