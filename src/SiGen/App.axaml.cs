using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using SiGen.DependencyInjection;
using SiGen.Services;
using SiGen.ViewModels;
using SiGen.Views;
using System;
using System.Globalization;
using System.Linq;

namespace SiGen;

public partial class App : Application
{

    public IServiceProvider Services { get; set; } = default!;

    public App(IServiceProvider services)
    {
        Services = services;
    }

    public App()
    {
        var collection = new ServiceCollection();
        collection.AddSiGenServices();
        collection.AddSingleton<IFileDialogService, DummyFileDialogService>();
        Services = collection.BuildServiceProvider();
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }


    public override void OnFrameworkInitializationCompleted()
    {
        var collection = new ServiceCollection();
        collection.AddSiGenServices();




        CultureInfo.CurrentUICulture = new CultureInfo("fr-CA");
        //Lang.Resources.Culture = new CultureInfo("en-US");
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DisableAvaloniaDataAnnotationValidation();
            //collection.AddSingleton<IFileDialogService, DummyFileDialogService>();
            var mainWindow = new MainWindow();
            collection.AddSingleton<IFileDialogService, DesktopFileDialogService>((sp) => new DesktopFileDialogService(mainWindow));
            
            Services = collection.BuildServiceProvider();
            mainWindow.DataContext = Services.GetService<DesktopMainViewModel>();
            desktop.MainWindow = mainWindow;
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            collection.AddSingleton<IFileDialogService, DummyFileDialogService>();

            Services = collection.BuildServiceProvider();
            singleViewPlatform.MainView = new MobileMainView
            {
                DataContext = Services.GetService<MainViewModel>() ?? new MainViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
    // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}