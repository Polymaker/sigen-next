using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Physics
{
    public enum Temperament
    {
        Equal, //Equal temperament
        Just, //Pure intervals based on harmonic series
        Thidell, // Used for true temperament on fretted instruments
        Pythagorean, //Pure intervals based on perfect fifths (3/2 ratio)
        Custom //When intervals are specified manually by the user
    }
}
