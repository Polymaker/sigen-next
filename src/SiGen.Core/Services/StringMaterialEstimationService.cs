using SiGen.Data.Common;
using SiGen.Layouts.Configuration;
using SiGen.Measuring;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SiGen.Services
{
    public interface IStringMaterialEstimationService
    {
        Task EstimateUnitWeightsAsync(InstrumentLayoutConfiguration configuration, bool overwriteExisting = false);

        Task EstimateModulusOfElasticityAsync(InstrumentLayoutConfiguration configuration, bool overwriteExisting = false);

        Task EstimateMaterialPropertiesAsync(InstrumentLayoutConfiguration configuration, bool overwriteUnitWeight = false, bool overwriteModulus = false);
    }

    public class StringMaterialEstimationService : IStringMaterialEstimationService
    {
        private readonly IStringDataService stringDataService;

        public StringMaterialEstimationService(IStringDataService stringDataService)
        {
            this.stringDataService = stringDataService;
        }

        public async Task EstimateUnitWeightsAsync(InstrumentLayoutConfiguration configuration, bool overwriteExisting = false)
        {
            foreach (var stringProps in EnumerateStringProperties(configuration))
            {
                if (!stringProps.Gauge.HasValue)
                    continue;

                stringProps.Material ??= new StringMaterialConfiguration();

                if (!overwriteExisting && (stringProps.Material.UnitWeight ?? 0) > 0)
                    continue;

                stringProps.Material.UnitWeight = await stringDataService.InterpolateUnitWeight(
                    (double)stringProps.Gauge.Value[LengthUnit.In],
                    stringProps.Material.MaterialType);
            }
        }

        public Task EstimateModulusOfElasticityAsync(InstrumentLayoutConfiguration configuration, bool overwriteExisting = false)
        {
            foreach (var stringProps in EnumerateStringProperties(configuration))
            {
                if (!stringProps.Gauge.HasValue)
                    continue;

                stringProps.Material ??= new StringMaterialConfiguration();

                if (!overwriteExisting && (stringProps.Material.ModulusOfElasticity ?? 0) > 0)
                    continue;

                stringProps.Material.ModulusOfElasticity = EstimateModulusByMaterial(stringProps.Material.MaterialType);
            }

            return Task.CompletedTask;
        }

        public async Task EstimateMaterialPropertiesAsync(InstrumentLayoutConfiguration configuration, bool overwriteUnitWeight = false, bool overwriteModulus = false)
        {
            await EstimateUnitWeightsAsync(configuration, overwriteUnitWeight);
            await EstimateModulusOfElasticityAsync(configuration, overwriteModulus);
        }

        private static IEnumerable<StringProperties> EnumerateStringProperties(InstrumentLayoutConfiguration configuration)
        {
            foreach (var config in configuration.StringConfigurations)
            {
                if (config is SingleStringConfiguration single && single.Properties != null)
                {
                    yield return single.Properties;
                }
                else if (config is StringGroupConfiguration group)
                {
                    foreach (var str in group.Strings)
                        yield return str;
                }
            }
        }

        private static double? EstimateModulusByMaterial(StringMaterialType? materialType)
        {
            return materialType switch
            {
                StringMaterialType.SteelPlain => 200,
                StringMaterialType.NickelWound => 190,
                StringMaterialType.BronzeWound => 110,
                StringMaterialType.NylonPlain => 3,
                StringMaterialType.SilverPlatedWound => 95,
                _ => null
            };
        }
    }
}
