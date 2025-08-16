using SiGen.Layouts.Configuration;
using SiGen.Layouts.Elements;
using SiGen.Maths;
using SiGen.Measuring;
using SiGen.Paths;
using SiGen.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Layouts.Builders
{
    public class LayoutBuilder
    {
        //public LayoutController Controller { get; }
        public StringedInstrumentLayout Layout { get; }
        public InstrumentLayoutConfiguration Configuration { get; private set; }
        public List<ValidationMessage> Messages { get; }
        public bool Success { get; private set; }

        public LayoutBuilder()
        {
            //Controller = new LayoutController(new InstrumentLayoutConfiguration());
            Layout = new StringedInstrumentLayout();
            Configuration = new InstrumentLayoutConfiguration();
            Messages = new List<ValidationMessage>();
        }

        //private T GetBuilder<T>() where T : LayoutBuilderBase
        //{
        //    return (T)Activator.CreateInstance(typeof(T), Layout, Configuration)!;
        //}

        //private void ExecuteBuilder<T>() where T : LayoutBuilderBase
        //{
        //    var builder = GetBuilder<T>();
        //    builder.BuildLayoutCore();
        //    Messages.AddRange(builder.Messages);
        //}

        private bool ExecuteBuilder(Type builderType)
        {
            var builder = (LayoutBuilderBase)Activator.CreateInstance(builderType, Layout, Configuration)!;
            bool success = builder.BuildLayout();
            Messages.AddRange(builder.Messages);
            return success;
        }

        public LayoutBuildResult BuildLayout(InstrumentLayoutConfiguration configuration)
        {
            Configuration = configuration;
            Layout.Configuration = configuration;
            Layout.Elements.Clear();

            var builderTypes = new Type[] {
                typeof(LayoutStringsBuilder),
                typeof(FingerBoardEdgesBuilder),
                typeof(FretsBuilder)
            };

            foreach (var builderType in builderTypes)
            {
                try
                {
                    Success |= ExecuteBuilder(builderType);

                    if (!Success) break;
                }
                catch (Exception ex)
                {
                    Success = false;
                    Messages.Add(new ValidationMessage(ValidationMessageType.Error, "Unexpected error:" + ex.ToString()));
                    break;
                }
            }

            if (Success)
                BuildStringGroups();

            if (Success && configuration.LeftHanded)
            {
                foreach (var element in Layout.Elements)
                    element.FlipHorizontal();
            }

            return new LayoutBuildResult(Success, Layout, Messages.ToList());
        }

        private void BuildStringGroups()
        {
            for (int i = 0; i < Configuration.NumberOfStrings; i++)
            {
                if (Configuration.StringConfigurations[i] is StringGroupConfiguration group)
                {
                    var offsetX = group.GetTotalSpacing() * -0.5d;
                    var stringElem = Layout.GetStringElement(i);
                    Layout.Elements.Remove(stringElem);
                    for (int j = 0; j < group.StringCount; j++)
                    {
                        var path = (LinearPath)stringElem.Path.Clone();
                        path.Offset(new VectorD(offsetX.NormalizedValue, 0));

                        var newString = new StringElement(i, j, path)
                        {
                            NutPoint = stringElem.NutPoint + new PointM(offsetX, Measure.Zero),
                            BridgePoint = stringElem.BridgePoint + new PointM(offsetX, Measure.Zero),
                            StartPoint = stringElem.StartPoint + new PointM(offsetX, Measure.Zero)
                        };
                        Layout.AddElement(newString);

                        offsetX += group.Spacing ?? Measure.Mm(1.5);
                    }
                }
            }
        }

        public static LayoutBuildResult Build(InstrumentLayoutConfiguration configuration)
        {
            var builder = new LayoutBuilder();
            return builder.BuildLayout(configuration);
        }
    }
}
