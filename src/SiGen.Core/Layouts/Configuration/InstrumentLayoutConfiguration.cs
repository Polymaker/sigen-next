using SiGen.Data.Common;
using SiGen.Layouts.Data;
using SiGen.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SiGen.Layouts.Configuration
{
    public class InstrumentLayoutConfiguration
    {
        public int Version { get; set; } = 2;

        public string? Name { get; set; }

        /// <summary>
        /// The number of strings on the instrument.
        /// </summary>
        public int NumberOfStrings { get; set; }

        [JsonIgnore]
        public int TotalNumberOfStrings => StringConfigurations.Sum(x => x.NumberOfStrings);

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
        public StringConfigurationCollection StringConfigurations { get; set; } = null!;

        /// <summary>
        /// Configures the fingerboard margins and other related settings.
        /// </summary>
        public FingerboardConfiguration Fingerboard { get; set; } = null!;


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
        /// Global fret configuration.
        /// </summary>
        public GlobalFretConfiguration Frets { get; set; }

        /// <summary>
        /// Gets or sets the number of frets on the instrument.
        /// Can also be set for each string individually.
        /// Put zero for no frets.
        /// </summary>
        [JsonIgnore]
        public int? NumberOfFrets
        {
            get => Frets.NumberOfFrets;
            set => Frets.NumberOfFrets = value;
        }

        /// <summary>
        /// Temperament used for fret placement on this string.
        /// </summary>
        /// <remarks>When set to <see cref="Temperament.Custom"/>, specify the intervals for each strings</remarks>
        [JsonIgnore]
        public Temperament Temperament
        {
            get => Frets.Temperament ?? Physics.Temperament.Equal;
            set => Frets.Temperament = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the layout is left-handed.
        /// </summary>
        public bool LeftHanded { get; set; }

        // not used at the moment, but may be useful in the future
        [JsonIgnore]
        public int? StringSetId { get; set; }

        public InstrumentLayoutConfiguration()
        {
            StringConfigurations = new StringConfigurationCollection();
            Fingerboard = new FingerboardConfiguration();
            NutSpacing = new StringSpacingConfiguration();
            BridgeSpacing = new StringSpacingConfiguration();
            ScaleLength = new ScaleLengthConfiguration();
            Frets = new GlobalFretConfiguration()
            {
                NumberOfFrets = 24,
                Temperament = Temperament.Equal
            };
            InstrumentType = SiGen.Data.Common.InstrumentType.ElectricGuitar;
            Temperament = Physics.Temperament.Equal;
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

        public StringProperties? GetStringProperties(int courseIndex, int? subIndex)
        {
            if (courseIndex < 0 || courseIndex >= StringConfigurations.Count)
                return null;

            var stringConfig = StringConfigurations[courseIndex];

            if (stringConfig is SingleStringConfiguration singleString)
            {
                // Single string - subIndex should be null or 0
                if (subIndex == null || subIndex == 0)
                    return singleString.Properties;
                return null;
            }
            else if (stringConfig is StringGroupConfiguration stringGroup)
            {
                // String group - subIndex must be provided and valid
                if (subIndex == null || subIndex < 0 || subIndex >= stringGroup.Strings.Count)
                    return null;
                return stringGroup.Strings[subIndex.Value];
            }

            return null;
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
            int numberOfFrets = Frets.NumberOfFrets ?? 0;

            foreach (var @string in StringConfigurations)
            {
                if (@string.Frets?.NumberOfFrets != null)
                    numberOfFrets = Math.Max(numberOfFrets, @string.Frets.NumberOfFrets.Value);
            }

            return numberOfFrets; 
        }

        /// <summary>
        /// Enumerates all string properties across all string configurations (both single strings and string groups).
        /// </summary>
        /// <returns>An enumerable of all StringProperties in the layout.</returns>
        public IEnumerable<StringContext<StringProperties>> EnumerateStringProperties()
        {
            int totalIndex = 0;

            for (int i = 0; i < NumberOfStrings; i++)
            {
                var stringConfig = StringConfigurations[i];
                if (stringConfig is SingleStringConfiguration singleString && singleString.Properties != null)
                {
                    yield return new StringContext<StringProperties>(totalIndex, i, null, singleString.Properties);
                    totalIndex++;
                }
                else if (stringConfig is StringGroupConfiguration stringGroup)
                {
                    for (int j = 0; j < stringGroup.NumberOfStrings; j++)
                    {
                        yield return new StringContext<StringProperties>(totalIndex, i, j, stringGroup.Strings[j]);
                        totalIndex++;
                    }
                }
            }
        }

        public static InstrumentLayoutConfiguration Duplicate(InstrumentLayoutConfiguration source)
        {
            var options = SiGen.Serialization.SiGenJsonOptions.Default;
            var json = System.Text.Json.JsonSerializer.Serialize(source, options);
            return System.Text.Json.JsonSerializer.Deserialize<InstrumentLayoutConfiguration>(json, options)
                   ?? throw new InvalidOperationException("Failed to duplicate configuration.");
        }
    }
}
