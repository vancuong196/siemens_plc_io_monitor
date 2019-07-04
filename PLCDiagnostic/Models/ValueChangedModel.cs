using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLCDiagnostic.Models
{
    public class ValueChangedModel
    {
        public string OldValue { set; get; }
        public string NewValue { set; get; }
        public string Name { set; get; }
        public string Address { set; get; }
    }
}
