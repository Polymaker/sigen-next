using SiGen.Layouts;
using SiGen.Layouts.Builders;
using SiGen.Layouts.Configuration;
using SiGen.Services;
using SiGen.Services.InstrumentProfiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.ViewModels.Design
{
    public class MockLayoutDocumentContext : ILayoutDocumentContext
    {
        public InstrumentLayoutConfiguration Configuration { get; }

        public bool HasUnsavedChanges { get; private set; }

        public StringedInstrumentLayout? CurrentLayout { get; private set; }

        public IInstrumentValuesProvider? InstrumentValuesProvider { get; }

        public MockLayoutDocumentContext()
        {
            InstrumentValuesProvider = new ElectricGuitarValuesProvider();
            // Initialize with a mock configuration for design-time purposes
            Configuration = new InstrumentLayoutConfiguration
            {
                InstrumentType = Data.Common.InstrumentType.ElectricGuitar,
                NumberOfStrings = 6,
                LeftHanded = false,
                NumberOfFrets = 24,
            };
            Configuration.ScaleLength.Mode = ScaleLengthMode.Single;
            Configuration.ScaleLength.SingleScale = Measuring.Measure.In(25.5);
            Configuration.NutSpacing.StringDistances.Add(Measuring.Measure.Mm(7.5));
            Configuration.NutSpacing.CenterAlignment = Layouts.Data.LayoutCenterAlignment.OuterStrings;
            Configuration.BridgeSpacing.StringDistances.Add(Measuring.Measure.Mm(10.5));
            Configuration.BridgeSpacing.CenterAlignment = Layouts.Data.LayoutCenterAlignment.OuterStrings;
            Configuration.Margin.SetAll(Measuring.Measure.Mm(3.25));
            Configuration.InitializeStringConfigs();

            var result = LayoutBuilder.Build(Configuration);
            CurrentLayout = result.Layout;
        }

        public void UpdateConfiguration(string reason, Action<InstrumentLayoutConfiguration> updateAction)
        {
            updateAction(Configuration);
            HasUnsavedChanges = true;
        }
    }
}
