using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using SiGen.Data.Common;
using SiGen.Layouts.Data;

namespace SiGen.Layouts.Configuration
{
    public class InstrumentLayoutConfiguration
    {
        /// <summary>
        /// The number of strings on the instrument.
        /// </summary>
        public int NumberOfStrings { get; set; }

        /// <summary>
        /// Gets or sets the type of the instrument.
        /// Used to provide default values for the layout.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public InstrumentType InstrumentType { get; set; }

        //public string? InstrumentVariant { get; set; }

        /// <summary>
        /// Gets or sets the collection of string configurations.
        /// </summary>
        /// <remarks>This property holds the configurations for strings, which may include details such as
        /// tuning, gauge, or material. Ensure that the list is properly initialized before accessing or modifying its
        /// contents.</remarks>
        public List<BaseStringConfiguration> StringConfigurations { get; set; } = null!;

        /// <summary>
        /// Gets or sets the configuration settings for margin values.
        /// </summary>
        public MarginConfiguration Margin { get; set; } = null!;

        /// <summary>
        /// Gets or sets the string spacing configuration for the nut of the instrument.
        /// </summary>
        public StringSpacingConfiguration NutSpacing { get; set; } = null!;

        /// <summary>
        /// Gets or sets the configuration for string spacing on the bridge.
        /// </summary>
        public StringSpacingConfiguration BridgeSpacing { get; set; } = null!;

        /// <summary>
        /// Gets or sets the configuration for the scale length of the instrument.
        /// </summary>
        public ScaleLengthConfiguration ScaleLength { get; set; } = null!;

        /// <summary>
        /// Gets or sets the number of frets on the instrument.
        /// Can also be set for each string individually.
        /// Put zero for no frets.
        /// </summary>
        public int? NumberOfFrets { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the layout is left-handed.
        /// </summary>
        public bool LeftHanded { get; set; }

        // not used at the moment, but may be useful in the future
        public int? StringSetId { get; set; }

        public InstrumentLayoutConfiguration()
        {
            StringConfigurations = new List<BaseStringConfiguration>();
            Margin = new MarginConfiguration();
            NutSpacing = new StringSpacingConfiguration();
            BridgeSpacing = new StringSpacingConfiguration();
            ScaleLength = new ScaleLengthConfiguration();
            InstrumentType = SiGen.Data.Common.InstrumentType.ElectricGuitar;
        }

        public StringSpacingConfiguration GetStringSpacing(FingerboardEnd end)
        {
            return end == FingerboardEnd.Nut ? NutSpacing : BridgeSpacing;
        }

        public BaseStringConfiguration? GetString(int index)
        {
            if (index >= 0 && index < StringConfigurations.Count)
                return StringConfigurations[index];
            return null;
        }

        public BaseStringConfiguration? GetString(int index, FingerboardSide side)
        {
            return GetString(side == FingerboardSide.Bass ? index : NumberOfStrings - 1 - index);
        }

        public void InitializeStringConfigs()
        {
            if (StringConfigurations.Count != NumberOfStrings)
            {
                while (StringConfigurations.Count < NumberOfStrings)
                    StringConfigurations.Add(new SingleStringConfiguration());

            }
        }

        public int GetMaxFrets()
        {
            int numberOfFrets = NumberOfFrets ?? 0;

            foreach (var @string in StringConfigurations)
            {
                if (@string.Frets?.NumberOfFrets != null)
                    numberOfFrets = Math.Max(numberOfFrets, @string.Frets.NumberOfFrets.Value);
            }
            if (NumberOfFrets.HasValue)
                return NumberOfFrets.Value;

            return numberOfFrets; 
        }
    }

    public enum ScaleLengthMode
    {
        /// <summary>
        /// A single scale length for all strings.
        /// </summary>
        Single,
        /// <summary>
        /// A separate scale length for treble and bass strings.
        /// </summary>
        Multiscale,
        /// <summary>
        /// A separate scale length for each string.
        /// </summary>
        PerString
    }
}
