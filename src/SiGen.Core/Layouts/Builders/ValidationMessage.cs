using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Layouts.Builders
{
    public class ValidationMessage
    {
        public ValidationMessageType Type { get; set; }
        public string Message { get; set; }
        public object[]? FormatArgs { get; set; }

        public ValidationMessage(ValidationMessageType type, string message, object[]? formatArgs)
        {
            Type = type;
            Message = message;
            FormatArgs = formatArgs;
        }

        public ValidationMessage(ValidationMessageType type, string message)
        {
            Type = type;
            Message = message;
        }

        public override string ToString()
        {
            string message = FormatArgs != null ? string.Format(Message, FormatArgs) : Message;
            return $"[{Type}] {message}";
        }
    }
    public enum ValidationMessageType
    {
        Info,
        Warning,
        Error,
    }
}
