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

        public VectorD Interpolate(double t)
        {
            t = MathD.Clamp(t);

            double omt = 1d - t;

            return Math.Pow(omt, 3d) * ControlPoints[0] +
                3 * Math.Pow(omt, 2d) * t * ControlPoints[1] +
                3 * omt * Math.Pow(t, 2d) * ControlPoints[2] +
                Math.Pow(t, 3d) * ControlPoints[3];
        }

        public override bool Intersects(LinearPath line, out VectorD intersection)
        {
            const int sampleCount = 50;
            VectorD prev = Interpolate(0);
            for (int i = 1; i <= sampleCount; i++)
            {
                double t = i / (double)sampleCount;
                VectorD curr = Interpolate(t);
                var segment = new LinearPath(prev, curr);
                if (LinearPath.Intersects(segment, line, out intersection))
                    return true;
                prev = curr;
            }
            intersection = VectorD.Empty;
            return false;
        }
    }
}
