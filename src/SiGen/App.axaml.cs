using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.Utilities;
using Microsoft.Extensions.DependencyInjection;
using SiGen.DependencyInjection;
using SiGen.Measuring;
using SiGen.Services;
using SiGen.Settings;
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
        collection.AddSingleton<IDialogService, MockDialogService>();
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
        

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            //DisableAvaloniaDataAnnotationValidation();
            

            var mainWindow = new MainWindow();
            collection.AddSingleton<IDialogService, DesktopDialogService>(sp => new DesktopDialogService(mainWindow, sp));
            Services = collection.BuildServiceProvider();
            InitializeDatabase();
            // Load and apply user settings
            ApplyUserSettings();

            mainWindow.Content = new DesktopMainView();
            mainWindow.DataContext = Services.GetService<DesktopMainViewModel>();
            desktop.MainWindow = mainWindow;
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            collection.AddSingleton<IDialogService, MockDialogService>();
            Services = collection.BuildServiceProvider();

            InitializeDatabase();
            // Load and apply user settings
            ApplyUserSettings();


            singleViewPlatform.MainView = new MobileMainView
            {
                //DataContext = Services.GetService<MainViewModel>() ?? new MainViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void InitializeDatabase()
    {
        var initializer = Services.GetService<IDatabaseInitializationService>();
        //set to true to re-import from StringsDB xml
        initializer?.InitializeAsync(false).GetAwaiter().GetResult();
    }

    private void ApplyUserSettings()
    {
        var settingsService = Services.GetService<ISettingsService>();
        if (settingsService == null)
            return;

        var settings = settingsService.Settings;

        // Apply language setting
        ApplyLanguage(settings.Language);

        // Apply theme setting
        ApplyTheme(settings.Theme);

        // Subscribe to settings changes for live updates
        settingsService.LanguageChanged += (s, language) => ApplyLanguage(language);
        settingsService.ThemeChanged += (s, theme) => ApplyTheme(theme);
    }

    private void ApplyLanguage(AppLanguage language)
    {
        CultureInfo culture;

        if (language == AppLanguage.System)
        {
            culture = CultureInfo.InstalledUICulture;
        }
        else
        {
            var cultureCode = language switch
            {
                AppLanguage.French => "fr",
                AppLanguage.Spanish => "es",
                AppLanguage.German => "de",
                AppLanguage.English => "en",
                _ => CultureInfo.InstalledUICulture.TwoLetterISOLanguageName
            };

            culture = new CultureInfo(cultureCode);
        }

        void ApplyCulture()
        {
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
        }

        if (Dispatcher.UIThread.CheckAccess())
            ApplyCulture();
        else
            Dispatcher.UIThread.Post(ApplyCulture);
    }

    private void ApplyTheme(AppTheme theme)
    {
        if (Current == null)
            return;

        Current.RequestedThemeVariant = theme switch
        {
            AppTheme.Light => ThemeVariant.Light,
            AppTheme.Dark => ThemeVariant.Dark,
            AppTheme.System => null, // null = follow system theme
            _ => null
        };
    }

    //// Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
    //// More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
    //private void DisableAvaloniaDataAnnotationValidation()
    //{
    //    // Get an array of plugins to remove
    //    var dataValidationPluginsToRemove =
    //        BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();
        
    //    // remove each entry found
    //    foreach (var plugin in dataValidationPluginsToRemove)
    //    {
    //        BindingPlugins.DataValidators.Remove(plugin);
    //    }
    //}
}