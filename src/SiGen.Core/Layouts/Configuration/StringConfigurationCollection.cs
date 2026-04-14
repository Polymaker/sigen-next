using SiGen.Data.Common;
using SiGen.Layouts.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Layouts.Configuration
{
    public interface IStringConfigurationCollection
    {
        int TotalNumberOfStrings { get; }
        StringIndex GetIndex(BaseStringConfiguration @string);
        //StringPosition GetPosition(BaseStringConfiguration stringConfig, FingerboardSide side);
    }

    public class StringConfigurationCollection : ObservableCollection<BaseStringConfiguration>, IStringConfigurationCollection
    {
        public StringConfigurationCollection()
        {
        }

        public StringConfigurationCollection(IEnumerable<BaseStringConfiguration> collection) : base(collection)
        {
        }

        public StringConfigurationCollection(List<BaseStringConfiguration> list) : base(list)
        {
        }

        public int TotalNumberOfStrings { get; private set; }

        //private record StringCourseInfo(int index, int numberOfStrings);

        protected override void InsertItem(int index, BaseStringConfiguration item)
        {
            base.InsertItem(index, item);
            item.AssignOwner(this);
            RecalculateIndices();
        }

        protected override void RemoveItem(int index)
        {
            if (index < Count && index >= 0)
                this[index].AssignOwner(null);

            base.RemoveItem(index);

            RecalculateIndices();
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            base.MoveItem(oldIndex, newIndex);
            RecalculateIndices();
        }

        public StringIndex GetIndex(BaseStringConfiguration @string)
        {
            if (!Contains(@string)) return new(0,0,null);

            int overallIndex = 0;
            for (int courseIndex = 0; courseIndex < Count; courseIndex++)
            {
                if (this[courseIndex] == @string) return new StringIndex(overallIndex, courseIndex, null);

                if (this[courseIndex] is StringGroupConfiguration sgc) overallIndex += sgc.NumberOfStrings;
                else overallIndex++;
            }

            return new(0, 0, null);
        }

        private void RecalculateIndices()
        {
            int overallIndex = 0;
            for (int courseIndex = 0; courseIndex < Count; courseIndex++)
            {
                if (this[courseIndex] is SingleStringConfiguration ssc)
                {
                    //ssc.Position = new StringIndex(overallIndex++, courseIndex, null);
                }
                else if (this[courseIndex] is StringGroupConfiguration sgc)
                {
                    for (int j = 0; j < sgc.NumberOfStrings; j++)
                    {
                        //sgc.Strings[j].Position = new StringIndex(overallIndex++, courseIndex, j);
                    }
                }
            }
            TotalNumberOfStrings = overallIndex;
        }
    }
}
