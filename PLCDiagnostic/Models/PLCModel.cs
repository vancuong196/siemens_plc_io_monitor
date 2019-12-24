using S7.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLCDiagnostic.Models
{
    public class PLCModel
    {
        public int Id { set; get; }
        public String Name { get; set; }
        public String Type { get; set; }
        public String Address { get; set; }
        public String IsEnable { get; set; }
        public CpuType CpuType { set; get; }
        public String DisplayName
        {
            get
            {
                return "Name: "+Name + " - IP: " + Address;
            }
        }
    }
}
