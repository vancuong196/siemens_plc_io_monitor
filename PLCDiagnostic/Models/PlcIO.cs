using Mvvm;
using PLCDiagnostic.Data;
using PLCDiagnostic.PLC;
using S7.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLCDiagnostic.Models
{
    public class PlcIO : BindableBase
    {
        private string _value;
        public string Name { set; get; }
        public CpuTypeModel CPUType { set; get; } = new CpuTypeModel()
        {
            Type = CpuType.S71500
        };
        public string Address { set; get; }
        public string Controller { set; get; } = "127.0.0.1";
        public string Value {
            set
            {
                SetProperty(ref _value, value);
            }
            get
            {
                return _value;
            }
        }
        public string ValueToDisplay
        {
            get
            {
                return Value;
            }
        }
        public bool IsEnable { set; get; }
        public int Type { set; get; }
        public string TypeName
        {
            get
            {
                if (Type==IOTypeConstants.BOOL_TYPE)
                {
                    return "Boolean";
                } else if (Type == IOTypeConstants.FLOAT_TYPE)
                {
                    return "Float";
                } else if (Type == IOTypeConstants.INT_TYPE)
                {
                    return "Int";
                } else
                {
                    return "Byte";
                }
            }
            set
            {
                if ("Boolean"==value)
                {
                    Type = IOTypeConstants.BOOL_TYPE;
                }else
                if ("Float" == value)
                {
                    Type = IOTypeConstants.FLOAT_TYPE;
                } else
                if ("Int" == value)
                {
                    Type = IOTypeConstants.INT_TYPE;
                } else
                
                {
                    Type = IOTypeConstants.BYTE_TYPE;
                }
               
            }
        }
    }
}
