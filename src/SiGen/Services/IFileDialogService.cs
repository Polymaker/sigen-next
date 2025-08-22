using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Services
{
    public interface IFileDialogService
    {
        // For saving files
        Task<string?> ShowSaveFileDialogAsync(string? title = null, string? defaultFileName = null, IEnumerable<FileDialogFilter>? filters = null);

        // For opening files
        Task<string?> ShowOpenFileDialogAsync(string? title = null, IEnumerable<FileDialogFilter>? filters = null);
    }

    public class FileDialogFilter
    {
        public string Name { get; set; } = string.Empty; // e.g. "Instrument Layout Files"
        public List<string> Extensions { get; set; } = new(); // e.g. { "sgl", "xml" }
    }
}
