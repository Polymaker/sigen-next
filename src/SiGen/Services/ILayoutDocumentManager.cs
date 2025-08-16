using SiGen.Layouts.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Services
{
    public interface ILayoutDocumentManager
    {
        InstrumentLayoutConfiguration Configuration { get; }


        void Undo();
        void Redo();

        //void ApplyChanges();
    }
}
