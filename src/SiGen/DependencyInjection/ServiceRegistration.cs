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
            //services.AddSingleton<InstrumentValuesProviderFactory>();

            // ViewModels

            services.AddSingleton<MainViewModel>();
            
            services.AddSingleton<DesktopMainViewModel>();
            services.AddTransient<ScaleLengthPanelViewModel>();
            services.AddTransient<InstrumentInfoPanelViewModel>();
            services.AddTransient<LayoutDocumentViewModel>();
            
            return services;
        }
    }
}
