using S7.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLCDiagnostic.PLC
{
    public class CpuTypeModel
    {
        public CpuType Type { set; get; }
        public string DisplayName
        {
            get
            {
                return Type.ToString();
            }
        }
    }
}
