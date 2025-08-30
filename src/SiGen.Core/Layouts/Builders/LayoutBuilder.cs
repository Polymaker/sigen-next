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

        private bool ExecuteBuilder(Type builderType, int pass)
        {
            var builder = (LayoutBuilderBase)Activator.CreateInstance(builderType, Layout, Configuration)!;
            
            bool success = builder.ExecuteBuilder(pass);
            Messages.AddRange(builder.Messages);
            return success;
        }

        public LayoutBuildResult BuildLayout(InstrumentLayoutConfiguration configuration)
        {
            Configuration = configuration;
            Layout.Configuration = configuration;
            Layout.Elements.Clear();

            var builderPasses = new (Type builderType, int pass)[]
            {
                (typeof(LayoutStringsBuilder), 1), // First pass of strings to create all the strings paths
                (typeof(FingerBoardEdgesBuilder), 1), // First pass of fingerboard edges to create the edges paths for fret calculation
                (typeof(FretsBuilder), 1),
                (typeof(FingerBoardEdgesBuilder), 2), //Finish the fingerboard shape
                (typeof(LayoutStringsBuilder), 2) 
            };

            foreach (var builderPass in builderPasses)
            {
                try
                {
                    Success |= ExecuteBuilder(builderPass.builderType, builderPass.pass);

                    if (!Success) break;
                }
                catch (Exception ex)
                {
                    Success = false;
                    Messages.Add(new ValidationMessage(ValidationMessageType.Error, "Unexpected error:" + ex.ToString()));
                    break;
                }
            }

            if (Success && configuration.LeftHanded)
            {
                foreach (var element in Layout.Elements)
                    element.FlipHorizontal();
            }

            if (Success)
                Layout.CalculateBounds();

            return new LayoutBuildResult(Success, Layout, Messages.ToList());
        }

        public static LayoutBuildResult Build(InstrumentLayoutConfiguration configuration)
        {
            var builder = new LayoutBuilder();
            return builder.BuildLayout(configuration);
        }
    }
}
