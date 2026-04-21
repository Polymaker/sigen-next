using SiGen.Data.Common;
using SiGen.Layouts.Configuration;
using SiGen.Layouts.Data;
using SiGen.Measuring;
using SiGen.Physics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SiGen.Serialization.Layouts.Xml
{
    /// <summary>
    /// Serializes and deserializes layout configurations in XML format (legacy, versions 1-2).
    /// </summary>
    public class XmlLayoutSerializer : ILayoutSerializer
    {
        public FileFormat Format => FileFormat.Xml;

        public int[] SupportedVersions => new[] { 1, 2 };

        public Task<InstrumentLayoutConfiguration> DeserializeAsync(
            Stream stream,
            CancellationToken cancellationToken = default)
        {
            var document = XDocument.Load(stream);
            var configuration = Deserialize(document);
            return Task.FromResult(configuration);
        }

        public Task SerializeAsync(
            InstrumentLayoutConfiguration configuration,
            Stream stream,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException("XML serialization is not supported for new configurations. Use JSON format instead.");
        }

        private InstrumentLayoutConfiguration Deserialize(XDocument document)
        {
            var root = document.Root;
            if (root == null || root.Name != "Layout")
                throw new InvalidDataException("Invalid XML layout file.");

            var config = new InstrumentLayoutConfiguration();
            //force version 1 since version 2 only add a new field an we are already trying to load it (if present) so no need to migrate from version 1 to 2
            config.Version = 1;// ReadIntAttribute(root, "Version", 1);

            var stringsElement = root.Element("Strings");
            if (stringsElement == null)
                throw new InvalidDataException("Missing Strings element.");

            config.NumberOfStrings = ReadIntAttribute(stringsElement, "Count", 6);
            config.InitializeStringConfigs();

            if (ReadBoolElement(root, "LeftHanded", false))
                config.LeftHanded = true;

            DeserializeScaleLength(root, config);
            DeserializeMargins(root, config);
            DeserializeStrings(root, config);
            DeserializeTemperament(root, config);
            DeserializeFretCompensation(root, config);
            DeserializeStringSpacing(root, config);

            // Detect instrument type based on string configuration
            DetectInstrumentType(config);

            return config;
        }

        /// <summary>
        /// Attempts to detect the instrument type based on number of strings and string gauges.
        /// Currently only detects Electric Bass (4-6 strings with typical bass gauges).
        /// </summary>
        private void DetectInstrumentType(InstrumentLayoutConfiguration config)
        {
            // Check if it's likely an electric bass
            if (config.NumberOfStrings >= 4 && config.NumberOfStrings <= 6)
            {
                var stringConfigs = config.StringConfigurations
                    .OfType<SingleStringConfiguration>()
                    .Where(s => s.Gauge.HasValue && s.Gauge.Value.Value > 0)
                    .ToList();

                if (stringConfigs.Count >= 3) // Need at least 3 strings with gauge data
                {
                    // Convert all gauges to inches for comparison
                    var gaugesInInches = stringConfigs
                        .Select(s => s.Gauge!.Value[LengthUnit.In])
                        .ToList();

                    // Bass strings typically range from 0.030" (high B on 6-string) to 0.135"
                    // Most strings should be >= 0.040"
                    var thickStrings = gaugesInInches.Count(g => g >= 0.040);
                    var validBassRange = gaugesInInches.All(g => g >= 0.025 && g <= 0.140);

                    // If most strings are bass-gauge thickness, it's likely a bass
                    if (validBassRange && thickStrings >= stringConfigs.Count - 1)
                    {
                        config.InstrumentType = InstrumentType.ElectricBass;
                        return;
                    }
                }
            }

            // Default to electric guitar for most cases
            config.InstrumentType = InstrumentType.ElectricGuitar;
        }

        private void DeserializeScaleLength(XElement root, InstrumentLayoutConfiguration config)
        {
            var slElement = root.Element("ScaleLength");
            if (slElement == null)
                return;

            var typeAttr = slElement.Attribute("Type")?.Value;
            var scaleMode = MapOldScaleLengthType(typeAttr);

            config.ScaleLength.Mode = scaleMode;

            var lengthFunc = slElement.Attribute("LengthFunction")?.Value;
            if (!string.IsNullOrEmpty(lengthFunc))
            {
                config.ScaleLength.CalculationMethod = ParseEnum<ScaleLengthCalculationMethod>(lengthFunc);
            }

            var bassTrebleSkewAttr = slElement.Attribute("BassTrebleSkew");
            if (bassTrebleSkewAttr != null)
            {
                config.ScaleLength.BassTrebleSkew = ParseMeasure(bassTrebleSkewAttr.Value);
            }

            switch (scaleMode)
            {
                case ScaleLengthMode.Single:
                    var lengthAttr = slElement.Attribute("Value");
                    if (lengthAttr != null)
                        config.ScaleLength.SingleScale = ParseMeasure(lengthAttr.Value);
                    break;

                case ScaleLengthMode.Multiscale:
                    var trebleAttr = slElement.Attribute("Treble");
                    var bassAttr = slElement.Attribute("Bass");
                    var perpRatioAttr = slElement.Attribute("PerpendicularFretRatio");

                    if (trebleAttr != null)
                        config.ScaleLength.TrebleScale = ParseMeasure(trebleAttr.Value);
                    if (bassAttr != null)
                        config.ScaleLength.BassScale = ParseMeasure(bassAttr.Value);
                    if (perpRatioAttr != null)
                        config.ScaleLength.MultiScaleRatio = double.Parse(perpRatioAttr.Value, CultureInfo.InvariantCulture);
                    break;

                case ScaleLengthMode.PerString:
                    // Per-string scale lengths are handled in DeserializeStrings
                    break;
            }
        }

        private void DeserializeMargins(XElement root, InstrumentLayoutConfiguration config)
        {
            var marginsElement = root.Element("FingerboardMargins");
            if (marginsElement == null)
                return;

            // Check for "Edges" attribute first - all margins are the same
            var edgesAttr = marginsElement.Attribute("Edges");
            if (edgesAttr != null)
            {
                var value = ParseMeasure(edgesAttr.Value);
                config.Fingerboard.SetAllMargins(value);
            }
            // Check for "Treble" and "Bass" attributes - sides are the same for both ends
            else if (marginsElement.Attribute("Treble") != null && marginsElement.Attribute("Bass") != null)
            {
                var trebleValue = ParseMeasure(marginsElement.Attribute("Treble")!.Value);
                var bassValue = ParseMeasure(marginsElement.Attribute("Bass")!.Value);
                
                config.Fingerboard.NutTrebleMargin = trebleValue;
                config.Fingerboard.BridgeTrebleMargin = trebleValue;
                config.Fingerboard.NutBassMargin = bassValue;
                config.Fingerboard.BridgeBassMargin = bassValue;
            }
            // Check for "Nut" and "Bridge" attributes - ends are the same for both sides
            else if (marginsElement.Attribute("Nut") != null && marginsElement.Attribute("Bridge") != null)
            {
                var nutValue = ParseMeasure(marginsElement.Attribute("Nut")!.Value);
                var bridgeValue = ParseMeasure(marginsElement.Attribute("Bridge")!.Value);
                
                config.Fingerboard.NutTrebleMargin = nutValue;
                config.Fingerboard.NutBassMargin = nutValue;
                config.Fingerboard.BridgeTrebleMargin = bridgeValue;
                config.Fingerboard.BridgeBassMargin = bridgeValue;
            }
            // Individual attributes
            else
            {
                var nutTrebleAttr = marginsElement.Attribute("NutTreble");
                var nutBassAttr = marginsElement.Attribute("NutBass");
                var bridgeTrebleAttr = marginsElement.Attribute("BridgeTreble");
                var bridgeBassAttr = marginsElement.Attribute("BridgeBass");

                if (nutTrebleAttr != null)
                    config.Fingerboard.NutTrebleMargin = ParseMeasure(nutTrebleAttr.Value);
                if (nutBassAttr != null)
                    config.Fingerboard.NutBassMargin = ParseMeasure(nutBassAttr.Value);
                if (bridgeTrebleAttr != null)
                    config.Fingerboard.BridgeTrebleMargin = ParseMeasure(bridgeTrebleAttr.Value);
                if (bridgeBassAttr != null)
                    config.Fingerboard.BridgeBassMargin = ParseMeasure(bridgeBassAttr.Value);
            }

            // Also read the Compensated and LastFret attributes if they exist
            var compensatedAttr = marginsElement.Attribute("Compensated");
            if (compensatedAttr != null && bool.TryParse(compensatedAttr.Value, out var compensated))
            {
                config.Fingerboard.CompensateMarginsForStrings = compensated;
            }

            var lastFretAttr = marginsElement.Attribute("LastFret");
            if (lastFretAttr != null)
            {
                config.Fingerboard.ExtensionAfterLastFret = ParseMeasure(lastFretAttr.Value);
            }
        }

        private void DeserializeStrings(XElement root, InstrumentLayoutConfiguration config)
        {
            var stringsElement = root.Element("Strings");
            if (stringsElement == null)
                return;

            var stringElements = stringsElement.Elements("String").OrderBy(e => ReadIntAttribute(e, "Index", 0)).ToList();

            for (int i = 0; i < stringElements.Count && i < config.StringConfigurations.Count; i++)
            {
                var stringElem = stringElements[i];
                
                // Old version: strings are ordered treble to bass (index 0 = treble)
                // New version: strings are ordered bass to treble (index 0 = bass)
                // Need to flip the index: newIndex = (count - 1) - oldIndex
                var newIndex = config.StringConfigurations.Count - 1 - i;
                var stringConfig = config.StringConfigurations[newIndex] as SingleStringConfiguration;
                if (stringConfig == null)
                    continue;

                if (config.ScaleLength.Mode == ScaleLengthMode.PerString)
                {
                    var scaleLengthAttr = stringElem.Attribute("ScaleLength");
                    if (scaleLengthAttr != null)
                        stringConfig.ScaleLength = ParseMeasure(scaleLengthAttr.Value);

                    var multiScaleRatioAttr = stringElem.Attribute("MultiScaleRatio");
                    if (multiScaleRatioAttr != null)
                        stringConfig.MultiScaleRatio = double.Parse(multiScaleRatioAttr.Value, CultureInfo.InvariantCulture);
                }

                var fretElem = stringElem.Element("Frets");
                if (fretElem != null)
                {
                    var startingFret = ReadIntAttribute(fretElem, "StartingFret", 0);
                    var numberOfFrets = ReadIntAttribute(fretElem, "NumberOfFrets", 24);

                    if (stringConfig.Frets == null)
                        stringConfig.Frets = new FretConfiguration();
                    if (startingFret != 0)
                        stringConfig.Frets.StartingFret = startingFret;
                    if (numberOfFrets != config.NumberOfFrets)
                        stringConfig.Frets.NumberOfFrets = numberOfFrets;
                }

                var tuningElem = stringElem.Element("Tuning");
                if (tuningElem != null)
                {
                    stringConfig.Tuning = DeserializeTuning(tuningElem);
                }

                var actionElem = stringElem.Element("Action");
                if (actionElem != null)
                {
                    // Note: ActionAtFirstFret and ActionAtTwelfthFret properties don't exist yet in the new configuration
                    // These values are currently not migrated
                    // TODO: Add these properties to SingleStringConfiguration when implemented
                    //var firstFretAttr = actionElem.Attribute("AtFirstFret");
                    //var twelfthFretAttr = actionElem.Attribute("AtTwelfthFret");

                    //if (firstFretAttr != null)
                    //    stringConfig.ActionAtFirstFret = ParseMeasure(firstFretAttr.Value);
                    //if (twelfthFretAttr != null)
                    //    stringConfig.ActionAtTwelfthFret = ParseMeasure(twelfthFretAttr.Value);
                }

                var propertiesElem = stringElem.Element("Properties");
                if (propertiesElem != null)
                {
                    DeserializeStringProperties(propertiesElem, stringConfig);
                }
            }
        }

        private NoteAndOctave? DeserializeTuning(XElement tuningElem)
        {
            var noteAttr = tuningElem.Attribute("Note");
            var octaveAttr = tuningElem.Attribute("Octave");

            if (noteAttr == null || octaveAttr == null)
                return null;

            var noteName = ParseEnum<NoteName>(noteAttr.Value);
            var octave = int.Parse(octaveAttr.Value);

            return new NoteAndOctave(noteName, octave);
        }

        private void DeserializeStringProperties(XElement propertiesElem, SingleStringConfiguration stringConfig)
        {
            var materialElem = propertiesElem.Element("Material");
            if (materialElem != null)
            {
                var typeAttr = materialElem.Attribute("Type");
                if (typeAttr != null)
                {
                    var materialType = MapOldStringMaterialType(typeAttr.Value);
                    stringConfig.MaterialType = materialType;
                }
            }

            var gaugeAttr = propertiesElem.Attribute("StringDiameter");
            if (gaugeAttr != null)
            {
                stringConfig.Gauge = ParseMeasure(gaugeAttr.Value);
            }
        }

        private void DeserializeTemperament(XElement root, InstrumentLayoutConfiguration config)
        {
            var temperamentElem = root.Element("Temperament");
            if (temperamentElem != null)
            {
                var valueAttr = temperamentElem.Attribute("Value");
                if (valueAttr != null)
                {
                    config.Temperament = ParseEnum<Temperament>(valueAttr.Value);
                }
            }
        }

        private void DeserializeFretCompensation(XElement root, InstrumentLayoutConfiguration config)
        {
            var fretCompElem = root.Element("FretCompensation");
            if (fretCompElem != null)
            {
                // Note: CompensateFretPositions property doesn't exist yet in the new configuration
                // This value is currently not migrated
                // TODO: Add this property to FretConfiguration when implemented
                //var valueAttr = fretCompElem.Attribute("Value");
                //if (valueAttr != null && bool.TryParse(valueAttr.Value, out var value))
                //{
                //    config.Frets.CompensateFretPositions = value;
                //}
            }
        }

        private void DeserializeStringSpacing(XElement root, InstrumentLayoutConfiguration config)
        {
            var spacingsElem = root.Element("StringSpacings");
            if (spacingsElem == null)
                return;

            var modeAttr = spacingsElem.Attribute("Mode");
            var isManualMode = modeAttr?.Value == "Manual";

            // Read alignment settings
            var nutAlignAttr = spacingsElem.Attribute("NutAlignment");
            var bridgeAlignAttr = spacingsElem.Attribute("BridgeAlignment");

            if (nutAlignAttr != null)
            {
                config.NutSpacing.CenterAlignment = MapOldStringSpacingAlignment(nutAlignAttr.Value);
            }
            if (bridgeAlignAttr != null)
            {
                config.BridgeSpacing.CenterAlignment = MapOldStringSpacingAlignment(bridgeAlignAttr.Value);
            }

            if (isManualMode)
            {
                // Manual mode: Read all individual spacing values from <Spacing> elements
                config.NutSpacing.SpacingMode = StringSpacingMode.Manual;
                config.BridgeSpacing.SpacingMode = StringSpacingMode.Manual;

                var spacingElements = spacingsElem.Elements("Spacing").OrderBy(e => ReadIntAttribute(e, "Index", 0)).ToList();

                config.NutSpacing.StringDistances.Clear();
                config.BridgeSpacing.StringDistances.Clear();

                foreach (var spacingElem in spacingElements)
                {
                    var nutAttr = spacingElem.Attribute("Nut");
                    var bridgeAttr = spacingElem.Attribute("Bridge");

                    if (nutAttr != null)
                    {
                        config.NutSpacing.StringDistances.Add(ParseMeasure(nutAttr.Value));
                    }
                    if (bridgeAttr != null)
                    {
                        config.BridgeSpacing.StringDistances.Add(ParseMeasure(bridgeAttr.Value));
                    }
                }
            }
            else
            {
                // Simple mode: Determine if it's CenterToCenter or Proportional based on StringSpacingMethod
                var nutSpacingMethodAttr = spacingsElem.Attribute("NutSpacingMode");
                var bridgeSpacingMethodAttr = spacingsElem.Attribute("BridgeSpacingMode");

                // Map StringSpacingMethod (EqualDistance/EqualSpacing) to StringSpacingMode
                var nutSpacingMode = MapStringSpacingMethod(nutSpacingMethodAttr?.Value);
                var bridgeSpacingMode = MapStringSpacingMethod(bridgeSpacingMethodAttr?.Value);

                config.NutSpacing.SpacingMode = nutSpacingMode;
                config.BridgeSpacing.SpacingMode = bridgeSpacingMode;

                // Read the single spacing value for simple mode
                var nutSpacingAttr = spacingsElem.Attribute("StringSpacingAtNut");
                var bridgeSpacingAttr = spacingsElem.Attribute("StringSpacingAtBridge");

                config.NutSpacing.StringDistances.Clear();
                config.BridgeSpacing.StringDistances.Clear();

                if (nutSpacingAttr != null)
                {
                    // Store single spacing value
                    config.NutSpacing.StringDistances.Add(ParseMeasure(nutSpacingAttr.Value));
                }
                //else if (nutSpacingMode == StringSpacingMode.Proportional)
                //{
                //    // For EqualSpacing mode, the adjusted spacings might be in <Spacing> elements
                //    var spacingElements = spacingsElem.Elements("Spacing").OrderBy(e => ReadIntAttribute(e, "Index", 0)).ToList();
                //    foreach (var spacingElem in spacingElements)
                //    {
                //        var nutAttr = spacingElem.Attribute("Nut");
                //        if (nutAttr != null)
                //        {
                //            config.NutSpacing.StringDistances.Add(ParseMeasure(nutAttr.Value));
                //        }
                //    }
                //}

                if (bridgeSpacingAttr != null)
                {
                    // Store single spacing value
                    config.BridgeSpacing.StringDistances.Add(ParseMeasure(bridgeSpacingAttr.Value));
                }
                //else if (bridgeSpacingMode == StringSpacingMode.Proportional)
                //{
                //    // For EqualSpacing mode, the adjusted spacings might be in <Spacing> elements
                //    var spacingElements = spacingsElem.Elements("Spacing").OrderBy(e => ReadIntAttribute(e, "Index", 0)).ToList();
                //    foreach (var spacingElem in spacingElements)
                //    {
                //        var bridgeAttr = spacingElem.Attribute("Bridge");
                //        if (bridgeAttr != null)
                //        {
                //            config.BridgeSpacing.StringDistances.Add(ParseMeasure(bridgeAttr.Value));
                //        }
                //    }
                //}
            }
        }

        #region Helper Methods

        private int ReadIntAttribute(XElement element, string attributeName, int defaultValue)
        {
            var attr = element.Attribute(attributeName);
            if (attr != null && int.TryParse(attr.Value, out var value))
                return value;
            return defaultValue;
        }

        private bool ReadBoolElement(XElement root, string elementName, bool defaultValue)
        {
            var elem = root.Element(elementName);
            if (elem != null)
            {
                var valueAttr = elem.Attribute("Value");
                if (valueAttr != null && bool.TryParse(valueAttr.Value, out var value))
                    return value;
            }
            return defaultValue;
        }

        private Measure ParseMeasure(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return Measure.Zero;

            if (MeasureParser.TryParse(value, out var measure))
                return measure.Value;

            return Measure.Zero;
        }

        private T ParseEnum<T>(string value) where T : struct, Enum
        {
            if (Enum.TryParse<T>(value, true, out var result))
                return result;
            return default;
        }

        /// <summary>
        /// Maps old ScaleLengthType enum values to new ScaleLengthMode enum values.
        /// Old: Single, Dual, Multiple → New: Single, Multiscale, PerString
        /// </summary>
        private ScaleLengthMode MapOldScaleLengthType(string? oldValue)
        {
            if (string.IsNullOrEmpty(oldValue))
                return ScaleLengthMode.Single;

            return oldValue switch
            {
                "Single" => ScaleLengthMode.Single,
                "Dual" => ScaleLengthMode.Multiscale,
                "Multiple" => ScaleLengthMode.PerString,
                _ => ScaleLengthMode.Single
            };
        }

        /// <summary>
        /// Maps old StringSpacingAlignment enum values to new LayoutCenterAlignment enum.
        /// Old: OuterStrings, MiddleString, FingerboardEdges → New: OuterStrings, MiddleStrings, Fingerboard
        /// </summary>
        private LayoutCenterAlignment MapOldStringSpacingAlignment(string oldValue)
        {
            return oldValue switch
            {
                "OuterStrings" => LayoutCenterAlignment.OuterStrings,
                "MiddleString" => LayoutCenterAlignment.MiddleStrings,
                "FingerboardEdges" => LayoutCenterAlignment.Fingerboard,
                _ => LayoutCenterAlignment.OuterStrings
            };
        }

        /// <summary>
        /// Maps old StringSpacingMethod enum values to new StringSpacingMode enum.
        /// Old: EqualDistance, EqualSpacing → New: CenterToCenter, Proportional
        /// </summary>
        private StringSpacingMode MapStringSpacingMethod(string? oldValue)
        {
            if (string.IsNullOrEmpty(oldValue))
                return StringSpacingMode.CenterToCenter;

            return oldValue switch
            {
                "EqualDistance" => StringSpacingMode.CenterToCenter,
                "StringsCenter" => StringSpacingMode.CenterToCenter,
                "EqualSpacing" => StringSpacingMode.Proportional,
                "BetweenStrings" => StringSpacingMode.Proportional,
                _ => StringSpacingMode.CenterToCenter
            };
        }

        /// <summary>
        /// Maps old StringMaterialType enum values to new ones.
        /// Old: PlainSteel, SteelWound → New: SteelPlain, NickelWound
        /// </summary>
        private StringMaterialType MapOldStringMaterialType(string oldValue)
        {
            return oldValue switch
            {
                "PlainSteel" => StringMaterialType.SteelPlain,
                "SteelWound" => StringMaterialType.NickelWound,
                // If it's already a new value, try to parse it
                _ => ParseEnum<StringMaterialType>(oldValue)
            };
        }

        #endregion
    }
}
