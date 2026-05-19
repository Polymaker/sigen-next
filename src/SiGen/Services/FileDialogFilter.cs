using System.Collections.Generic;

namespace SiGen.Services
{
    public class FileDialogFilter
    {
        public FileDialogFilter()
        {
        }

        public FileDialogFilter(string name, params string[] extensions)
        {
            Name = name;
            Extensions = [.. extensions];
        }

        public string Name { get; set; } = string.Empty; // e.g. "Instrument Layout Files"
        public List<string> Extensions { get; set; } = new(); // e.g. { "sgl", "xml" }


    }
}
