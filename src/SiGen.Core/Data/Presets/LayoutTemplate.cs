using SiGen.Data.Common;
using SiGen.Layouts.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Data.Presets
{
    public class LayoutTemplate
    {
        public string Name { get; }
        public InstrumentLayoutConfiguration Configuration { get; set; }
        public int NumberOfStrings => Configuration.NumberOfStrings;
        public InstrumentType InstrumentType  => Configuration.InstrumentType;

        public LayoutTemplate(string name, InstrumentLayoutConfiguration configuration)
        {
            Name = name;
            Configuration = configuration;
        }
    }
}
