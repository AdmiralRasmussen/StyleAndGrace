using System.Drawing;

namespace TizzleTazzle {
    struct BotState {
        public PointF Location;
        public double Heading;
        public double Velocity;
        public long   Turn;

        public double HeadingRadians {
            get { return Geometry.DegreesToRadians(this.Heading); }
        }
    }
}
