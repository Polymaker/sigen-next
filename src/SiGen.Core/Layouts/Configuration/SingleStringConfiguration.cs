using SiGen.Measuring;
using SiGen.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SiGen.Layouts.Configuration
{

    /// <summary>
    /// The configuration for a single string.
    /// </summary>
    public class SingleStringConfiguration : BaseStringConfiguration
    {

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public StringProperties? Properties { get; set; }

        [JsonIgnore]
        public Measure? Gauge
        {
            get => Properties?.Gauge;
            set
            {
                Properties ??= new StringProperties();
                Properties.Gauge = value;
            }
        }

        [JsonIgnore]
        public PitchInterval? Tuning
        {
            get => Properties?.Tuning;
            set
            {
                Properties ??= new StringProperties();
                Properties.Tuning = value;
            }
        }

        public override Measure? GetTotalWidth()
        {
            return Gauge;
        }

    }
}
