using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SiGen.Views;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SiGen.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _greeting = "Welcome to Avalonia!";

    [ObservableProperty]
    private ObservableCollection<object> tabs = new();

    [ObservableProperty]
    private object? selectedTab;

    public MainViewModel()
    {
        Tabs.Add(new HomeViewModel());
        SelectedTab = Tabs[0];
    }

    [RelayCommand]
    private void ShowHomePage()
    {
        var foundPage = Tabs.OfType<HomeViewModel>().FirstOrDefault();
        if (foundPage != null)
            SelectedTab = foundPage;
        else
        {
            Tabs.Insert(0, new HomeViewModel());
            SelectedTab = Tabs[0];
        }
    }

    //[RelayCommand]
    //private async Task ShowAppSettings()
    //{
    //    var dialog = new AppSettingsView
    //    {
    //        //DataContext = new AppSettingsViewModel()
    //    };
    //    //await dialog.ShowDialog((Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow);
    //}
}
