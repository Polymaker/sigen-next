using SiGen.Layouts.Configuration;
using SiGen.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Services
{
    public interface IDocumentManager
    {
        void OpenDocumentFile(string? filePath);
        void OpenLayoutConfiguration(string documentName, InstrumentLayoutConfiguration configuration);
    }
}
