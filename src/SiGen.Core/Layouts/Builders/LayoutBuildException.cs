using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Layouts.Builders
{
    public class LayoutBuildException : Exception
    {
        public LayoutBuildException()
        {
        }

        public LayoutBuildException(string? message) : base(message)
        {
        }

        public LayoutBuildException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
