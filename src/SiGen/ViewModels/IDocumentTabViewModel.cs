using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.ViewModels
{
    public interface IDocumentTabViewModel
    {
        string Title { get; }
        bool IsHomePage { get; }
        bool IsDocument { get; }
        bool HasUnsavedChanges { get; }
        string? TabToolTip { get; }
    }
}
