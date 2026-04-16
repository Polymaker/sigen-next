using SiGen.Data.Common;
using SiGen.Layouts.Configuration;
using SiGen.Layouts.Data;
using SiGen.Measuring;
using SiGen.Physics;
using System.Collections.Generic;

namespace SiGen.Layouts.Configuration.Builders
{
    public class LayoutConfigurationBuilder
    {
        private readonly InstrumentLayoutConfiguration _config;

        public LayoutConfigurationBuilder()
        {
            _config = new InstrumentLayoutConfiguration();
        }

        public LayoutConfigurationBuilder WithInstrumentType(InstrumentType type)
        {
            _config.InstrumentType = type;
            return this;
        }

        public LayoutConfigurationBuilder WithNumberOfFrets(int frets)
        {
            _config.NumberOfFrets = frets;
            return this;
        }

        #region Strings

        public LayoutConfigurationBuilder AddSingleString(
            Measure? gauge = null,
            NoteAndOctave? tuning = null,
            FretConfiguration? frets = null,
            Measure? scaleLength = null,
            FingerboardSide side = FingerboardSide.Treble)
        {
            var stringConfig = new SingleStringConfiguration
            {
                ScaleLength = scaleLength,
                Frets = frets,
                Tuning = tuning,
                Gauge = gauge
            };
            if (side == FingerboardSide.Treble)
                _config.StringConfigurations.Add(stringConfig);
            else
                _config.StringConfigurations.Insert(0, stringConfig);
            _config.NumberOfStrings = _config.StringConfigurations.Count;
            return this;
        }

        public LayoutConfigurationBuilder AddSingleString(System.Action<SingleStringBuilder> config, FingerboardSide side = FingerboardSide.Treble)
        {
            var builder = new SingleStringBuilder();
            config(builder);
            var stringConfig = builder.Build();
            if (side == FingerboardSide.Treble)
                _config.StringConfigurations.Add(stringConfig);
            else
                _config.StringConfigurations.Insert(0, stringConfig);
            _config.NumberOfStrings = _config.StringConfigurations.Count;
            return this;
        }

        public LayoutConfigurationBuilder AddStringCourse(System.Action<StringCourseBuilder> groupConfig,
            FingerboardSide side = FingerboardSide.Treble)
        {
            var groupBuilder = new StringCourseBuilder();
            groupConfig(groupBuilder);
            var stringConfig = groupBuilder.Build();
            if (side == FingerboardSide.Treble)
                _config.StringConfigurations.Add(stringConfig);
            else
                _config.StringConfigurations.Insert(0, stringConfig);
            _config.NumberOfStrings = _config.StringConfigurations.Count;
            return this;
        }

        #endregion

        #region Scale length

        public LayoutConfigurationBuilder WithScaleLength(Measure scaleLength)
        {
            _config.ScaleLength.Mode = ScaleLengthMode.Single;
            _config.ScaleLength.SingleScale = scaleLength;
            return this;
        }

        public LayoutConfigurationBuilder WithMultiscaleLength(Measure bassScale, Measure trebleScale, double alignmentRatio = 0.5)
        {
            _config.ScaleLength.Mode = ScaleLengthMode.Multiscale;
            _config.ScaleLength.BassScale = bassScale;
            _config.ScaleLength.TrebleScale = trebleScale;
            _config.ScaleLength.MultiScaleRatio = alignmentRatio;
            return this;
        }

        #endregion

        #region String Spacing

        public LayoutConfigurationBuilder WithNutSpacing(Measure spacing, 
            StringSpacingMode mode = StringSpacingMode.Proportional, 
            LayoutCenterAlignment centerAlignment = LayoutCenterAlignment.OuterStrings)
        {
            if (_config.NutSpacing.StringDistances.Count == 0)
                _config.NutSpacing.AddDistance(Data.FingerboardSide.Bass, spacing);
            else
                _config.NutSpacing.StringDistances[0] = spacing;
            _config.NutSpacing.SpacingMode = mode;
            _config.NutSpacing.CenterAlignment = centerAlignment;
            return this;
        }

        public LayoutConfigurationBuilder WithBridgeSpacing(Measure spacing, 
            StringSpacingMode mode = StringSpacingMode.CenterToCenter, 
            LayoutCenterAlignment centerAlignment = LayoutCenterAlignment.OuterStrings)
        {
            if (_config.BridgeSpacing.StringDistances.Count == 0)
                _config.BridgeSpacing.AddDistance(Data.FingerboardSide.Bass, spacing);
            else
                _config.BridgeSpacing.StringDistances[0] = spacing;
            _config.BridgeSpacing.SpacingMode = mode;
            _config.BridgeSpacing.CenterAlignment = centerAlignment;
            return this;
        }

        #endregion

        public LayoutConfigurationBuilder WithMargins(Measure measure, bool compensateForStrings = false)
        {
            _config.Fingerboard.SetAllMargins(measure);
            _config.Fingerboard.CompensateMarginsForStrings = compensateForStrings;
            return this;
        }

        public LayoutConfigurationBuilder WithNutBridgeMargins(Measure nut, Measure bridge, bool compensateForStrings = false)
        {
            _config.Fingerboard.SetNutAndBridgeMargins(FingerboardEnd.Nut, nut);
            _config.Fingerboard.SetNutAndBridgeMargins(FingerboardEnd.Bridge, bridge);
            _config.Fingerboard.CompensateMarginsForStrings = compensateForStrings;
            return this;
        }

        public LayoutConfigurationBuilder WithExtension(Measure? measure)
        {
            _config.Fingerboard.ExtensionAfterLastFret = measure;
            return this;
        }

        public InstrumentLayoutConfiguration Build()
        {
            _config.NumberOfStrings = _config.StringConfigurations.Count;
            return _config;
        }
    }

    public class StringCourseBuilder
    {
        private readonly StringGroupConfiguration _courseConfig = new();

        public StringCourseBuilder AddString(
            NoteAndOctave? tuning = null,
            Measure? gauge = null,
            StringMaterialConfiguration? material = null)
        {
            var stringProps = new StringProperties
            {
                Tuning = tuning,
                Gauge = gauge,
                Material = material
            };
            _courseConfig.Strings.Add(stringProps);
            return this;
        }

        public StringCourseBuilder AddString(System.Action<StringPropertiesBuilder> config)
        {
            var builder = new StringPropertiesBuilder();
            config(builder);
            var stringProps = builder.Build();
            _courseConfig.Strings.Add(stringProps);
            return this;
        }

        public StringCourseBuilder WithSpacing(Measure spacing)
        {
            _courseConfig.Spacing = spacing;
            return this;
        }

        public StringCourseBuilder WithNumberOfFrets(int frets)
        {
            _courseConfig.Frets ??= new FretConfiguration();
            _courseConfig.Frets.NumberOfFrets = frets;
            return this;
        }

        public StringGroupConfiguration Build() => _courseConfig;
    }

    public class StringPropertiesBuilder
    {
        private readonly StringProperties _stringProps = new();
        public StringPropertiesBuilder WithGauge(Measure? gauge)
        {
            _stringProps.Gauge = gauge;
            return this;
        }

        public StringPropertiesBuilder WithTuning(NoteAndOctave? tuning)
        {
            _stringProps.Tuning = tuning;
            return this;
        }

        public StringPropertiesBuilder WithTuning(NoteName note, int octave)
        {
            _stringProps.Tuning = new NoteAndOctave(note, octave);
            return this;
        }

        public StringPropertiesBuilder WithMaterial(StringMaterialConfiguration? material)
        {
            _stringProps.Material = material;
            return this;
        }

        public StringPropertiesBuilder WithMaterialType(StringMaterialType? material)
        {
            _stringProps.Material ??= new StringMaterialConfiguration();
            _stringProps.Material.MaterialType = material;
            return this;
        }

        public StringProperties Build() => _stringProps;

    }

    public class SingleStringBuilder
    {
        private readonly SingleStringConfiguration _stringConfig = new();

        public SingleStringBuilder WithGauge(Measure? gauge)
        {
            _stringConfig.Gauge = gauge;
            return this;
        }

        public SingleStringBuilder WithTuning(NoteAndOctave? tuning)
        {
            _stringConfig.Tuning = tuning;
            return this;
        }

        public SingleStringBuilder WithTuning(NoteName note, int octave)
        {
            _stringConfig.Tuning = new NoteAndOctave(note, octave);
            return this;
        }

        public SingleStringBuilder WithFrets(FretConfiguration? frets)
        {
            _stringConfig.Frets = frets;
            return this;
        }

        public SingleStringBuilder WithNumberOfFrets(int frets)
        {
            _stringConfig.Frets ??= new FretConfiguration();
            _stringConfig.Frets.NumberOfFrets = frets;
            return this;
        }

        public SingleStringBuilder WithStartingFret(int fret)
        {
            _stringConfig.Frets ??= new FretConfiguration();
            _stringConfig.Frets.StartingFret = fret;
            return this;
        }

        public SingleStringBuilder WithScaleLength(Measure? scaleLength)
        {
            _stringConfig.ScaleLength = scaleLength;
            return this;
        }

        public SingleStringBuilder WithMaterial(StringMaterialConfiguration? material)
        {
            if (_stringConfig.Properties == null)
                _stringConfig.Properties = new StringProperties();
            _stringConfig.Properties.Material = material;
            return this;
        }

        public SingleStringBuilder WithMaterialType(StringMaterialType? material)
        {
            if (_stringConfig.Properties == null)
                _stringConfig.Properties = new StringProperties();
            _stringConfig.Properties.Material ??= new StringMaterialConfiguration();
            _stringConfig.Properties.Material.MaterialType = material;
            return this;
        }

        public SingleStringConfiguration Build() => _stringConfig;
    }
}
