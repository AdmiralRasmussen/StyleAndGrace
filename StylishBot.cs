using Robocode;
using System.Drawing;
using System;

namespace TizzleTazzle {
    internal abstract class StylishBot : Robot {
        protected double RadarHeadingRadians {
            get { return Geometry.DegreesToRadians(base.RadarHeading); }
        }

        public double HeadingRadians {
            get { return Geometry.DegreesToRadians(base.Heading); }
        }

        public PointF Location {
            get { return Geometry.MakePoint(this.X, this.Y); }
        }

        protected bool TurnGunTo(double heading, double headingThreshold = 0) {
            double turn = NormalizeHeading(heading - this.GunHeading);
            if (Math.Abs(turn) > headingThreshold) {
                base.TurnGunRight(turn);
                return turn != 0;
            } else {
                return false;
            }
        }

        protected bool TurnGunTo(PointF target, double headingThreshold = 0) {
            return this.TurnGunTo(this.GetHeadingTo(target), headingThreshold);
        }

        protected bool TurnTo(double heading) {
            double turn = NormalizeHeading(heading - this.Heading);
            base.TurnRight(turn);
            return turn != 0;
        }

        protected bool TurnRadarTo(double heading) {
            double turn = NormalizeHeading(heading - this.RadarHeading);
            base.TurnRadarRight(turn);
            return turn != 0;
        }

        protected bool TurnRadarTo(PointF target) {
            return this.TurnRadarTo(this.GetHeadingTo(target));
        }

        protected double GetHeadingTo(PointF target) {
            return Geometry.RadiansToDegrees(Math.Atan2(target.X - base.X, target.Y - base.Y));
        }

        private static double NormalizeHeading(double a) {
            while (a > 180) a -= 360;
            while (a < -180) a += 360;
            return a;
        }

        protected double DistanceTo(PointF dest) {
            return Geometry.Distance(this.Location, dest);
        }

        public static double GetShotSpeed(double power) {
            return 20 - (3 * power); // http://robowiki.net/wiki/Robocode/FAQ
        }

        protected BotState? GetFoeState(ScannedRobotEvent evnt) {
            var direction = this.Heading + evnt.Bearing;

            return new BotState {
                Observer = this,
                Energy = evnt.Energy,
                Location = Geometry.ShiftBy(this.Location, direction, evnt.Distance, this.GetRobotBounds()),
                Heading = evnt.Heading,
                Velocity = evnt.Velocity,
                Turn = this.Time,
            };
        }

        protected double GetDistanceTo(PointF target) {
            return Geometry.Distance(this.Location, target);
        }
    }
}
