using SiGen.Measuring;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Layouts
{
    public abstract class LayoutElement
    {
        public StringedInstrumentLayout? Layout { get; private set; }

        public string Id { get; set; } = string.Empty;
        private RectangleM? _Bounds;
        private bool boundsInvalidated = true;

        public RectangleM? Bounds => boundsInvalidated ? CalculateBounds() : _Bounds;

        public void InvalidateBounds()
        {
            boundsInvalidated = true;
        }

        protected RectangleM? CalculateBounds()
        {
            _Bounds = CalculateBoundsCore();
            boundsInvalidated = false;
            return _Bounds;
        }

        protected virtual RectangleM? CalculateBoundsCore()
        {
            return null;
        } 

        internal void AssignLayout(StringedInstrumentLayout? layout)
        {
            Layout = layout;
        }

        public void FlipHorizontal()
        {
            FlipHorizontalCore();
            InvalidateBounds();
        }

        protected abstract void FlipHorizontalCore();

    }

    public class LayoutElementCollection : ICollection<LayoutElement>
    {
        private List<LayoutElement> _elements;
        private StringedInstrumentLayout _layout;

        public LayoutElementCollection(StringedInstrumentLayout layout)
        {
            _layout = layout;
            _elements = new List<LayoutElement>();
        }

        public int Count => ((ICollection<LayoutElement>)_elements).Count;

        public bool IsReadOnly => ((ICollection<LayoutElement>)_elements).IsReadOnly;

        public void Add(LayoutElement item)
        {
            _elements.Add(item);
            item.AssignLayout(_layout);
        }

        public bool Remove(LayoutElement item)
        {
            bool result = _elements.Remove(item);
            if (result)
                item.AssignLayout(null);
            return result;
        }

        public void Clear()
        {
            foreach (var item in _elements)
                item.AssignLayout(null);
            _elements.Clear();
        }

        public bool Contains(LayoutElement item)
        {
            return _elements.Contains(item);
        }

        public void CopyTo(LayoutElement[] array, int arrayIndex)
        {
            ((ICollection<LayoutElement>)_elements).CopyTo(array, arrayIndex);
        }

        public IEnumerator<LayoutElement> GetEnumerator()
        {
            return ((IEnumerable<LayoutElement>)_elements).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_elements).GetEnumerator();
        }
    }
}
