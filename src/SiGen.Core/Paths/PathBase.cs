using SiGen.Maths;

namespace SiGen.Paths
{
    public abstract class PathBase
    {
        public abstract void Offset(VectorD offset);

        public void Offset(double x, double y)
            => Offset(new VectorD(x, y));

        public abstract VectorD GetFirstPoint();
        public abstract VectorD GetLastPoint();

        public virtual PathBase? Extend(double amount)
        {
            return null;
        }

        public abstract bool Intersects(LinearPath line, out VectorD intersection);

        public abstract void FlipHorizontal();
    }
}
