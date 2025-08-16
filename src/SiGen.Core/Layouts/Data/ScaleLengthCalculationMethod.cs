using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Layouts.Data
{
    public enum ScaleLengthCalculationMethod
    {
        /// <summary>
        /// For Single scale layout, the default mode is AlongFingerboard.
        /// For multiscale, the default mode is AlongString.
        /// </summary>
        Auto,
        /// <summary>
        /// The scale length is applied/calculated with the string's length taking into account the taper of the fingerboard. 
        /// Commonly used for multiscale. Is also used for calculating fret compensation.
        /// </summary>
        AlongString,
        /// <summary>
        /// The scale length is applied/calculated with the string's length along the center of the fingerboard.
        /// Standard for non-multiscale instruments.
        /// </summary>
        AlongFingerboard
    }
}
