using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SiGen.Data.Entities;
using SiGen.Services;
using SiGen.Services.Audio;
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
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<IDatabaseInitializationService, DatabaseInitializationService>();

            services.AddDbContextFactory<SiGenDbContext>(options =>
                options.UseSqlite(SiGenDatabasePath.GetConnectionString()));

            services.AddSingleton<ViewModelFactory>();
            services.AddSingleton<IStringDataService, StringDataService>();
            services.AddTransient<IStringMaterialEstimationService, StringMaterialEstimationService>();

            services.AddSingleton<AudioService>();

            services.AddSingleton<HomePageViewModel>();
            services.AddSingleton<DesktopMainViewModel>();

            return services;
        }
    }
}
