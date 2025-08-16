using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.ViewModels
{
    public partial class HomeViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ObservableCollection<string> recentFiles = new()
        {
            "Document1.txt",
            "Notes.docx"
        };
        [ObservableProperty]
        private ObservableCollection<string> templates = new()
        {
            "Blank",
            "Invoice",
            "Report"
        };
    }
}
