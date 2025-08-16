using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Physics
{
    public class Temperament
    {
        public int Tones { get; }

        public Temperament(int tones)
        {
            Tones = tones;
        }



        public static Temperament ET12 { get; } = new Temperament(12);


    }
}
