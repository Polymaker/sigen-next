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

        protected abstract void ExecuteFirstPass();

        protected virtual void ExecuteSecondPass() { }

        public bool ExecuteBuilder(int pass)
        {
            if (pass == 1)
                ExecuteFirstPass();
            else if (pass == 2)
                ExecuteSecondPass();
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
