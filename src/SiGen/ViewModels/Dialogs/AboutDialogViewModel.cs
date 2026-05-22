using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.Reflection;

namespace SiGen.ViewModels.Dialogs;

public partial class AboutDialogViewModel : DialogViewModelBase
{
    private const string RepositoryUrlValue = "https://github.com/Polymaker/sigen-next";

    public override string Title => Lang.Resources.AboutDialog_Title;

    public string ApplicationName => Lang.Resources.AboutDialog_AppName;

    public string Tagline => Lang.Resources.AboutDialog_Tagline;

    public string Description => Lang.Resources.AboutDialog_Description;

    public string VersionLabel => Lang.Resources.AboutDialog_VersionLabel;

    public string Version => GetApplicationVersion();

    public string RepositoryLabel => Lang.Resources.AboutDialog_RepositoryLabel;

    public string RepositoryUrl => RepositoryUrlValue;

    public string LicenseLabel => Lang.Resources.AboutDialog_LicenseLabel;

    public string LicenseName => Lang.Resources.AboutDialog_LicenseName;

    public string Copyright => Lang.Resources.AboutDialog_Copyright;

    public string HighlightsTitle => Lang.Resources.AboutDialog_HighlightsTitle;

    public string Highlight1 => Lang.Resources.AboutDialog_Highlight1;

    public string Highlight2 => Lang.Resources.AboutDialog_Highlight2;

    public string Highlight3 => Lang.Resources.AboutDialog_Highlight3;

    public string CloseButtonText => Lang.Resources.Common_Close;

    public AboutDialogViewModel()
    {
        Resizable = false;
    }

    [RelayCommand]
    private void Close()
    {
        Complete();
    }

    [RelayCommand]
    private void OpenRepository()
    {
        OpenUrl(RepositoryUrl);
    }

    private static string GetApplicationVersion()
    {
        var assembly = Assembly.GetEntryAssembly() ?? typeof(AboutDialogViewModel).Assembly;
        var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        if (!string.IsNullOrWhiteSpace(informationalVersion))
        {
            if (informationalVersion.Contains('+'))
            {
                informationalVersion = informationalVersion.Split('+')[0];
            }
            return informationalVersion;
        }
            

        return assembly.GetName().Version?.ToString(3) ?? "1.0.0";
    }

    private static void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch
        {
        }
    }
}
