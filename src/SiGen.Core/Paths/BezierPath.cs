using SiGen.Maths;

namespace SiGen.Paths
{
    public class BezierPath : PathBase
    {
        private VectorD[] controlPoints;

        public BezierPath(VectorD[] controlPoints)
        {
            if (controlPoints?.Length != 4)
                throw new ArgumentException("controlPoints must have four points");
            this.controlPoints = controlPoints;
            Update();
        }

        public VectorD Start
        {
            get => controlPoints[0];
            set
            {
                controlPoints[0] = value;
                Update();
            }
        }

        public VectorD End
        {
            get => controlPoints[^1];
            set
            {
                controlPoints[^1] = value;
                Update();
            }
        }

        public VectorD[] ControlPoints 
        { 
            get => controlPoints;
            set {
                controlPoints = value;
                Update();
            } 
        }

        public override VectorD GetFirstPoint()
        {
            return Start;
        }

        public override VectorD GetLastPoint()
        {
            return End;
        }

        public override void FlipHorizontal()
        {
            for (int i = 0; i < controlPoints.Length; i++)
            {
                controlPoints[i] = new VectorD(-controlPoints[i].X, controlPoints[i].Y);
            }
        }

        private void Update()
        {

        }

        public override void Offset(VectorD offset)
        {
            for (int i = 0; i < 4; i++)
                controlPoints[i] += offset;
            Update();
        }

        public VectorD Interpolate(PreciseDouble t)
        {
            t = MathD.Clamp(t);

            PreciseDouble omt = 1d - t;

            return MathD.Pow(omt, 3f) * ControlPoints[0] +
                3 * MathD.Pow(omt, 2f) * t * ControlPoints[1] +
                3 * omt * MathD.Pow(t, 2f) * ControlPoints[2] +
                MathD.Pow(t, 3f) * ControlPoints[3];
        }
    }
}
