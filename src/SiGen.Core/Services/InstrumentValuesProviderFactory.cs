using SiGen.Data.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Services
{
    public interface IInstrumentValuesProviderFactory
    {
        IInstrumentValuesProvider Create(InstrumentType type);
    }

    public class InstrumentValuesProviderFactory : IInstrumentValuesProviderFactory
    {
        private readonly Dictionary<InstrumentType, Func<IInstrumentValuesProvider>> _registry;

        public InstrumentValuesProviderFactory() 
        { 
            _registry = new Dictionary<InstrumentType, Func<IInstrumentValuesProvider>>
            {
                { InstrumentType.ElectricGuitar, () => new InstrumentProfiles.ElectricGuitarValuesProvider() },
                { InstrumentType.AcousticGuitar, () => new InstrumentProfiles.AcousticGuitarValuesProvider() },
                { InstrumentType.ClassicalGuitar, () => new InstrumentProfiles.ClassicalGuitarValuesProvider() },
                { InstrumentType.BassGuitar, () => new InstrumentProfiles.ElectricBassGuitarValuesProvider() },
                { InstrumentType.AcousticBass, () => new InstrumentProfiles.AcousticBassValuesProvider() },
                //{ InstrumentType.Mandolin, () => new InstrumentProfiles.MandolinValuesProvider() },
                { InstrumentType.Banjo, () => new InstrumentProfiles.BanjoValuesProvider() },
                //{ InstrumentType.Ukulele, () => new InstrumentProfiles.UkuleleValuesProvider() },
            };
        }

        public IInstrumentValuesProvider Create(InstrumentType type)
        {
            if (_registry.TryGetValue(type, out var creator))
                return creator();

            throw new ArgumentException($"No values provider registered for instrument type '{type}'.");
        }
    }
}
