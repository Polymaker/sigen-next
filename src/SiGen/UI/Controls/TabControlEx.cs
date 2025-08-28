using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.UI.Controls
{

    public class TabReorderedEventArgs : EventArgs
    {
        public int OldIndex { get; }
        public int NewIndex { get; }
        public TabReorderedEventArgs(int oldIndex, int newIndex)
        {
            OldIndex = oldIndex;
            NewIndex = newIndex;
        }
    }

    public class TabControlEx : TabControl
    {
        protected override Type StyleKeyOverride => typeof(TabControl);

        private int? dragStartTabIndex;
        private int? dragTargetTabIndex;
        private ItemsPresenter? ItemsPresenterPart { get; set; }

        public event EventHandler<TabReorderedEventArgs>? TabReordered;

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            ItemsPresenterPart = e.NameScope.Find<ItemsPresenter>("PART_ItemsPresenter");
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

            var point = e.GetCurrentPoint(this);
            if (point.Properties.IsLeftButtonPressed && point.Pointer.Type == PointerType.Mouse)
            {
                var container = GetContainerFromEventSource(e.Source);
                if (container != null)
                {
                    dragStartTabIndex = IndexFromContainer(container);
                }
            }
        }
 
        private TabItem? GetTabItemAtPoint(Point point)
        {
            if (ItemsPresenterPart == null)
                return null;

            foreach (var tabItem in ItemsPresenterPart.GetVisualDescendants().OfType<TabItem>())
            {
                var bounds = tabItem.Bounds;
                if (bounds.Contains(point))
                    return tabItem;
            }

            return null;
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);
  
            if (ItemsPresenterPart == null || !dragStartTabIndex.HasValue)
                return;

            var point = e.GetPosition(ItemsPresenterPart);

            var hoveredTab = GetTabItemAtPoint(point);

            if (hoveredTab != null)
            {
                int overIndex = IndexFromContainer(hoveredTab);
                if (overIndex == dragStartTabIndex.Value)
                {
                    dragTargetTabIndex = null;
                    return;
                }

                var tabBounds = hoveredTab.Bounds;
                bool isAfter = overIndex > dragStartTabIndex.Value;
                double midpoint = tabBounds.X + (tabBounds.Width * (isAfter ? 0.45d : 0.55d));
               
                bool shouldReorder = false;

                if (isAfter && point.X > midpoint)
                    shouldReorder = true;
                else if (!isAfter && point.X < midpoint)
                    shouldReorder = true;

                dragTargetTabIndex = shouldReorder ? overIndex : null;
            }
            else
            {
                //allow to drag after the last tab, so don't reset dragTargetTabIndex here
            }
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);

            var point = e.GetCurrentPoint(this);
            
            if (e.InitialPressMouseButton == MouseButton.Left && dragStartTabIndex.HasValue)
            {
                if (dragTargetTabIndex.HasValue && dragStartTabIndex.HasValue && dragTargetTabIndex.Value != dragStartTabIndex.Value)
                {
                    TabReordered?.Invoke(this, new TabReorderedEventArgs(dragStartTabIndex.Value, dragTargetTabIndex.Value));
                }
                dragStartTabIndex = null;
                dragTargetTabIndex = null;
            }
        }

    }
}
