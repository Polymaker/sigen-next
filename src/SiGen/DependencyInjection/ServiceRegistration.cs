using Microsoft.Extensions.DependencyInjection;
using SiGen.Services;
using SiGen.ViewModels;
using SiGen.ViewModels.EditorPanels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.DependencyInjection
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddSiGenServices(this IServiceCollection services)
        {
            // Register all shared services here
            services.AddSingleton<IInstrumentValuesProviderFactory, InstrumentValuesProviderFactory>();
            //services.AddSingleton<IScalePresetRepository, ScalePresetRepository>();
            //services.AddSingleton<ITuningPresetRepository, TuningPresetRepository>();

            // ViewModels
            services.AddSingleton<MainViewModel>();
            services.AddTransient<InstrumentValuesProviderFactory>();
            services.AddTransient<ScaleLengthPanelViewModel>();

            return services;
        }
    }
}
