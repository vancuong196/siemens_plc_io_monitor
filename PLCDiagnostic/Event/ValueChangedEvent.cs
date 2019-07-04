using PLCDiagnostic.Models;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLCDiagnostic.Event
{
    class ValueChangedEvent: PubSubEvent<ValueChangedModel>
    {
    }
}
