using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLCDiagnostic.Models
{
    public class PlcEventModel
    {
        public int Type { set; get; }
        public string Address { set; get; }

        public string Data { set; get; }
    }
}
