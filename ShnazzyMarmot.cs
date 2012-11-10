using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robocode;

namespace TizzleTazzle {
    class ShnazzyMarmot : Robot {
        private bool Safety = false;
        private bool FiredOnLastSighting = false;

        public override void Run() {
            base.TurnGunRight(360);

            while (true) {
                base.Ahead(50);
                if (this.LastSighting.HasValue) {
                    double target = this.LastHeading + 180;

                    double diff = target - this.Heading;
                    while (diff > 180) diff -= 360;
                    double speedFactor = Math.Max(this.LastVelocity / 8, 1);
                    diff *= speedFactor;
                    base.TurnRight(diff);

                } else {
                    base.TurnRight(20);
                }

                if (!this.LastSighting.HasValue) {
                    base.TurnGunRight(10);
                } else {
                    double targetX = this.LastX + (this.Time - this.LastSighting.Value) * this.LastVelocity * Math.Sin(this.LastHeadingRadians);
                    double targetY = this.LastY + (this.Time - this.LastSighting.Value) * this.LastVelocity * Math.Cos(this.LastHeadingRadians);

                    double dist = Math.Sqrt(Math.Pow(targetX - this.X, 2) + Math.Pow(targetX - this.Y, 2));
                    double power = Math.Min(Math.Max(300 / dist, .1), 3);
                    double shotSpeed = 20 - (3 * power); // http://robowiki.net/wiki/Robocode/FAQ
                    double timeToTarget = dist / shotSpeed;

                    targetX += timeToTarget * Math.Sin(this.LastHeadingRadians);
                    targetY += timeToTarget * Math.Cos(this.LastHeadingRadians);

                    double target = RadiansToDegrees(Math.Atan2(targetX - base.X, targetY - base.Y));
                    double diff = target - this.RadarHeading;

                    base.Out.WriteLine("target: {0:0.00} heading: {1:0.00} diff: {2:0.00}, ({3:0.00}, {4:0.00}) LastVelocity: {5:0.00} LastHeading: {6:0.00} timeToTarget: {7:0.00}", 
                        target, diff, this.RadarHeading, targetX, targetY, this.LastVelocity, this.LastHeadingRadians, timeToTarget);

                    while (diff > 180) diff -= 360;
                    while (diff < -180) diff += 360;

                    base.TurnGunRight(diff);

                    if (!this.FiredOnLastSighting)
                    {
                        base.Fire(power);
                        this.FiredOnLastSighting = true;
                    }

                    if (diff > 0) {
                        base.TurnGunRight(10 + (this.Time - this.LastSighting.Value) * 2);
                    }

                    if (diff < 0) {
                        base.TurnGunLeft(10 + (this.Time - this.LastSighting.Value) * 2);
                    }
                }
            }
        }

        private double LastX = 0;
        private double LastY = 0;
        private double LastHeadingRadians = 0;
        private double LastHeading = 0;
        private double LastVelocity = 0;
        private long? LastSighting = null;

        public override void OnScannedRobot(ScannedRobotEvent evnt) {
            this.LastX = base.X + evnt.Distance * Math.Sin(this.RadarHeadingRadians);
            this.LastY = base.Y + evnt.Distance * Math.Cos(this.RadarHeadingRadians);
            this.LastHeadingRadians = evnt.HeadingRadians;
            this.LastHeading = evnt.Heading;
            this.LastVelocity = evnt.Velocity;
            this.LastSighting = this.Time;

            base.Out.WriteLine("{0} saw at {1}, {2}", this.LastSighting, this.LastX, this.LastY);
            this.FiredOnLastSighting = false;

            if (evnt.Distance < this.Width || evnt.Velocity == 0) {
                this.Fire(3);
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
