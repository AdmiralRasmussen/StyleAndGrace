using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robocode;

namespace TizzleTazzle {
    class ShnazzyMarmot : Robot {
        private bool Safety = false;

        public override void Run() {
            base.TurnGunRight(360);

            while (true) {
                base.Ahead(50);
                base.TurnRight(20);

                if (!this.LastSighting.HasValue) {
                    base.TurnGunRight(10);
                } else {
                    double target = RadiansToDegrees(Math.Atan2(this.LastX - base.X, this.LastY - base.Y));
                    double diff = target - this.RadarHeading;

                    base.Out.WriteLine("target: {0} heading: {1} diff: {2}", target, diff, this.RadarHeading);

                    while (diff > 180) diff -= 360;
                    while (diff < -180) diff += 360;

                    if (diff > 0) {
                        base.TurnGunRight(diff + 10 + Math.Sqrt(this.Time - this.LastSighting.Value));
                    }

                    if (diff < 0) {
                        base.TurnGunLeft(-diff + 10 + Math.Sqrt(this.Time - this.LastSighting.Value));
                    }
                }
            }
        }

        private double LastX = 0;
        private double LastY = 0;
        private long? LastSighting = null;

        public override void OnScannedRobot(ScannedRobotEvent evnt) {
            this.LastX = base.X + evnt.Distance * Math.Sin(this.RadarHeadingRadians);
            this.LastY = base.Y + evnt.Distance * Math.Cos(this.RadarHeadingRadians);
            this.LastSighting = this.Time;

            base.Out.WriteLine("{0} saw at {1}, {2}", this.LastSighting, this.LastX, this.LastY);

            if (!this.Safety) {
                base.Fire(.5);
            }
        }

        private static double DegreesToRadians(double degrees) {
            return degrees * 2 * Math.PI / 360;
        }

        private static double RadiansToDegrees(double radians) {
            return radians / 2 / Math.PI * 360;
        }

        protected double RadarHeadingRadians {
            get { return DegreesToRadians(base.RadarHeading); }
        }
    }
}
