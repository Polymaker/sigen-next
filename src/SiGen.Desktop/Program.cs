using System;
using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SiGen.DependencyInjection;
using SiGen.Services;

namespace SiGen.Desktop;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Call the shared registration method
                services.AddSiGenServices();
                
                // Register platform-specific services if needed
                services.AddSingleton<IFileDialogService, AvaloniaFileDialogService>();
            })
            .Build();

        BuildAvaloniaApp(host)
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure(() => new App())
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();


    public static AppBuilder BuildAvaloniaApp(IHost host)
        => AppBuilder.Configure(() => new App(host.Services))
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
