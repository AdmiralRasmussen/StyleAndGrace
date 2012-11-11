using System.Drawing;
using System;
using Robocode;

namespace TizzleTazzle {
    struct BotState {
        public Robot Observer;
        public PointF Location;
        public double Heading;
        public double Velocity;
        public long   Turn;
        public double Energy { get; set; }

        public double HeadingRadians {
            get { return Geometry.DegreesToRadians(this.Heading); }
        }

        public PointF GetProjectedLocation(double turnsInFuture = 0) {
            double elapsed = this.Observer.Time - this.Turn;
            double traveled = (elapsed + turnsInFuture) * this.Velocity;

            return this.Location.ShiftBy(this.Heading, traveled, this.Observer.GetRobotBounds());
        }

        public long Age { get { return this.Observer.Time - this.Turn; } }
    }
}
