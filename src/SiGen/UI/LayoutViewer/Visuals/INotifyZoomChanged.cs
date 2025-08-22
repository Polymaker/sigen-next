using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.UI.LayoutViewer.Visuals
{
    public interface INotifyZoomChanged
    {
        void ZoomChanged(double newZoom);

        void BeginZoomChange();

        void EndZoomChange();
    }
}
