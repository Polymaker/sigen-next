using SiGen.Db;
using SiGen.Layouts.Configuration;
using SiGen.Layouts.Data;
using SiGen.Measuring;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Layouts
{
    public class LayoutDataHelper
    {
        public int NumberOfStrings => Configuration.NumberOfStrings;

        public InstrumentLayoutConfiguration Configuration { get; set; }

        public SiGenDbContext DbContext { get; }

        public LayoutDataHelper(InstrumentLayoutConfiguration configuration)
        {
            Configuration = configuration;
            DbContext = new SiGenDbContext();
        }

        public BaseStringConfiguration? GetStringConfig(StringIndex stringIndex)
        {
            return Configuration.GetString(stringIndex.GetArrayIndex(NumberOfStrings));
        }

        protected Db.Models.InstrumentString? GetInstrumentString(StringIndex stringIndex)
        {
            var stringCfg = GetStringConfig(stringIndex);

            if (!string.IsNullOrEmpty(stringCfg?.Properties?.InstrumentStringId))
            {
                var stringInfo = DbContext.InstrumentStrings
                    .FirstOrDefault(x => x.Id == stringCfg.Properties.InstrumentStringId);

                if (stringInfo != null)
                    return stringInfo;
            }


            if (Configuration.StringSetId != null)
            {
                var stringInfo = DbContext.StringSetStrings
                    .Where(x =>
                        x.SetId == Configuration.StringSetId &&
                        x.Position == stringIndex.GetArrayIndex(NumberOfStrings)
                    )
                    .Select(x => x.String).FirstOrDefault();

                if (stringInfo != null)
                    return stringInfo;
            }

            return null;
        }

        public StringMaterialConfiguration? GetStringMaterial(StringIndex stringIndex)
        {
            var stringInfo = GetInstrumentString(stringIndex);
            if (stringInfo != null)
            {
                return new StringMaterialConfiguration
                {
                    CoreDiameter = stringInfo.CoreDiameter,
                    ModulusOfElasticity = stringInfo.MoE,
                    UnitWeight = stringInfo.UnitWeight,
                    Style = stringInfo.Style,
                    Material = stringInfo.Material,
                };
            }
            return GetStringConfig(stringIndex)?.Properties?.Material;
        }

        public Measure? GetStringGauge(StringIndex stringIndex)
        {
            var stringInfo = GetInstrumentString(stringIndex);
            if (stringInfo != null)
                return Measure.In(stringInfo.Gauge);

            return GetStringConfig(stringIndex)?.Gauge;
        }
    }
}
