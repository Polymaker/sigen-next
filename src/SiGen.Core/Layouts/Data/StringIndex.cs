using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Layouts.Data
{
    public readonly struct StringIndex
    {
        public readonly int Index;
        public readonly FingerboardSide Side;

        public StringIndex(int index, FingerboardSide side)
        {
            Index = index;
            Side = side;
        }

        public int GetArrayIndex(int numberOfStrings)
        {
            return Side == FingerboardSide.Bass ? Index : numberOfStrings - 1 - Index;
        }

        public int GetRelativeIndex(int numberOfStrings, FingerboardSide side)
        {
            if (Side == side)
                return Index;
            return numberOfStrings - 1 - Index;
        }

        public static implicit operator StringIndex(int index)
        {
            return new StringIndex(index, FingerboardSide.Bass);
        }
    }
}
