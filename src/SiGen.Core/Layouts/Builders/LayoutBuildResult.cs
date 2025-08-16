using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Layouts.Builders
{
    public class LayoutBuildResult
    {
        public LayoutBuildResult(bool success, StringedInstrumentLayout? layout, List<ValidationMessage> messages)
        {
            Success = success;
            Layout = layout;
            Messages = messages;
        }

        public bool Success { get; private set; }
        public List<ValidationMessage> Messages { get; }
        public bool HasErrors => Messages.Any(x => x.Type == ValidationMessageType.Error);
        public bool HasWarnings => Messages.Any(x => x.Type == ValidationMessageType.Warning);
        public StringedInstrumentLayout? Layout { get; private set; }
    }
}
