using SiGen.Maths;

namespace SiGen.Paths
{
    public abstract class PathBase
    {
        public abstract void Offset(VectorD offset);

        public void Offset(PreciseDouble x, PreciseDouble y)
            => Offset(new VectorD(x, y));

        public abstract VectorD GetFirstPoint();
        public abstract VectorD GetLastPoint();

        public virtual PathBase? Extend(PreciseDouble amount)
        {
            return null;
        }

        public abstract void FlipHorizontal();
    }
}
