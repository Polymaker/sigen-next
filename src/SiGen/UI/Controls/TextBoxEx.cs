using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.UI.Controls
{
    public class TextBoxEx : TextBox
    {
        private ScrollViewer? _scrollViewer;

        protected override Type StyleKeyOverride => typeof(TextBox);

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            _scrollViewer = e.NameScope.Find<ScrollViewer>("PART_ScrollViewer");
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            // Measure normally
            var baseSize = base.MeasureOverride(availableSize);

            //// Force width to be limited by available width
            //var forcedWidth = availableSize.Width;
            //if (double.IsInfinity(availableSize.Width))
            //    return base.MeasureOverride(availableSize); // No need to clamp if available width is infinite
            // Keep height as calculated but clamp width
            return new Size(MinWidth, baseSize.Height);
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            
            var arranged = base.ArrangeOverride(finalSize);

            if (_scrollViewer != null && Bounds.Width > 0)
            {
                _scrollViewer.MaxWidth = Bounds.Width - (Padding.Left + Padding.Right);
            }

            return base.ArrangeOverride(finalSize);
        }
    }
}
