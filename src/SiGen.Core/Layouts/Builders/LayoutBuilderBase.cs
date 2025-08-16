using SiGen.Layouts.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Layouts.Builders
{
    public abstract class LayoutBuilderBase
    {

        public List<ValidationMessage> Messages { get; }
        public StringedInstrumentLayout Layout { get; }
        public InstrumentLayoutConfiguration Configuration { get; private set; }

        protected int NumberOfStrings => Configuration.NumberOfStrings;

        protected LayoutBuilderBase(StringedInstrumentLayout layout, InstrumentLayoutConfiguration configuration)
        {
            Layout = layout;
            Configuration = configuration;
            Messages = new List<ValidationMessage>();
        }

        public abstract void BuildLayoutCore();

        public virtual bool BuildLayout()
        {
            BuildLayoutCore();
            return !Messages.Any(x => x.Type == ValidationMessageType.Error);
        }

        protected BaseStringConfiguration? GetStringConfig(int index)
        {
            if (index < Configuration.StringConfigurations.Count)
                return Configuration.StringConfigurations[index];
            return null;
        }

        protected void AddWarning(string message, params object[] arguments)
        {
            Messages.Add(new ValidationMessage(ValidationMessageType.Warning, message, arguments));
        }

        protected void AddError(string message, params object[] arguments)
        {
            Messages.Add(new ValidationMessage(ValidationMessageType.Error, message, arguments));
        }
    }
}
