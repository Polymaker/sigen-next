using System.Collections.Generic;

namespace SiGen.Services
{
    public class FileDialogFilter
    {
        public string Name { get; set; } = string.Empty; // e.g. "Instrument Layout Files"
        public List<string> Extensions { get; set; } = new(); // e.g. { "sgl", "xml" }
    }
}
