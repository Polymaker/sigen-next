using Avalonia.Controls;
using SiGen.Layouts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.UI.LayoutViewer.Visuals
{
    public abstract class VisualElementBase<T> : Panel where T : LayoutElement
    {
        public T Element { get; private set; }

        protected StringedInstrumentLayout Layout => Element.Layout!;

        protected VisualElementBase(T element)
        {
            Element = element ?? throw new ArgumentNullException(nameof(element));
            GenerateVisuals();
            // Set the name for debugging purposes
            //Name = element.Name;
        }

        protected abstract void GenerateVisuals();



    }
}
