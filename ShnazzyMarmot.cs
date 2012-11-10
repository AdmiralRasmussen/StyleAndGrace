using System;
using System.Collections.Generic;
using System.Linq;
using Robocode;
using System.Drawing;

namespace TizzleTazzle {
    class ShnazzyMarmot : Robot {
        private bool FiredOnLastSighting = true;
        private BotState? LastFoeState = null;
        private PointF? LastShotTarget = null;
        private PointF? LastShotSource = null;
        private List<PointF> LocationHistory = null;
        private int ShotsFired = 0;
        private int ShotsHit = 0;
        private Random rng = new Random();

        public override void Run() {
            this.SetColors(Color.Pink, Color.CornflowerBlue, Color.LimeGreen);
            this.LocationHistory = new List<PointF>();

            while (true) {
                try {
                    RunBody();
                } catch (Exception e) {
                    this.Out.WriteLine(e.ToString());
                    throw;
                }
            }
        }

        protected double GetHeadingTo(PointF target) {
            double radians = Math.Atan2(target.X - this.X, target.Y - this.Y);
            return Geometry.RadiansToDegrees(radians);
        }

        private void RunBody() {
            const double DRIVE_DISTANCE = 90;

            while (this.FiredOnLastSighting) {
                base.TurnRadarRight(360);
            }

            var bounds = this.GetRobotBounds();
            PointF center = bounds.GetCenter();

            PointF ahead = Geometry.ShiftBy(this.Location, this.Heading, DRIVE_DISTANCE, bounds);
            PointF behind = Geometry.ShiftBy(this.Location, this.Heading, -DRIVE_DISTANCE, bounds);

            bool aheadCloser = Geometry.Distance(ahead, center) <= Geometry.Distance(behind, center);

            if (aheadCloser) {
                base.Ahead(DRIVE_DISTANCE);
            } else {
                base.Back(DRIVE_DISTANCE);
            }
            
            this.LocationHistory.Add(this.Location);

            this.TurnTo(this.GetHeadingTo(this.LastFoeState.Value.Location) + 90 + rng.Next(-20, 21));

            this.MakeRefinedShot();
            this.FiredOnLastSighting = true;
        }

        private void MakeRefinedShot() {
            const double TARGET_ACCURACY = 1;
            const int AIM_TURNS = 2;

            PointF currentLocation = this.Location;
            PointF lastTarget;
            PointF target = this.GetProjectedFoeLocation(AIM_TURNS);
            this.TurnGunTo(target);

            double dist = Geometry.Distance(currentLocation, target);
            double power = GetShotPower(dist);
            double shotSpeed = GetShotSpeed(power);

            do {
                double timeToTarget = dist / shotSpeed + AIM_TURNS;

                lastTarget = target;
                target = this.GetProjectedFoeLocation(timeToTarget);
            } while (Geometry.Distance(target, lastTarget) > TARGET_ACCURACY);

            this.LastShotTarget = target;
            this.LastShotSource = currentLocation;
            this.TurnGunTo(target);
            base.Fire(power);

            this.ShotsFired++;
        }

        private static double GetShotSpeed(double power) {
            return 20 - (3 * power); // http://robowiki.net/wiki/Robocode/FAQ
        }

        private static double GetShotPower(double dist) {
            return Geometry.Clamp(300 / dist, .1, 3);
        }

        private PointF GetProjectedFoeLocation(double turns) {
            if (!this.LastFoeState.HasValue) return PointF.Empty;

            double elapsed = this.Time - this.LastFoeState.Value.Turn;
            double traveled = (elapsed + turns) * this.LastFoeState.Value.Velocity;
            double heading = this.LastFoeState.Value.HeadingRadians;
            PointF target = this.LastFoeState.Value.Location;

            return Geometry.ShiftBy(target, heading, traveled, this.GetRobotBounds());
        }

        private RectangleF GetRobotBounds() {
            return new RectangleF(
                (float)this.Width / 2, (float)this.Height / 2,
                (float)(this.BattleFieldWidth - this.Width / 2), (float)(this.BattleFieldHeight - this.Height / 2));
        }

        protected void TurnTo(double heading) {
            double turn = Normalize(heading - this.Heading);
            base.TurnRight(turn);
        }

        protected void TurnGunTo(PointF target) {
            double heading = Geometry.RadiansToDegrees(Math.Atan2(target.X - base.X, target.Y - base.Y));
            this.TurnGunTo(heading);
        }

        protected void TurnGunTo(double heading) {
            double turn = Normalize(heading - this.GunHeading);
            base.TurnGunRight(turn);
        }

        protected PointF Location {
            get { return Geometry.MakePoint(this.X, this.Y); }
        }

        private static double Normalize(double a) {
            while (a > 180) a -= 360;
            while (a < -180) a += 360;
            return a;
        }

        private double DistanceTo(PointF dest) {
            return Geometry.Distance(this.Location, dest);
        }

        public override void OnScannedRobot(ScannedRobotEvent evnt) {
            var direction = this.HeadingRadians + evnt.BearingRadians;

            this.LastFoeState = new BotState {
                Location = Geometry.MakePoint(base.X + evnt.Distance * Math.Sin(direction), base.Y + evnt.Distance * Math.Cos(direction)),
                Heading = evnt.Heading,
                Velocity = evnt.Velocity,
                Turn = this.Time,
            };

            this.FiredOnLastSighting = false;
        }

        public override void OnBulletHit(BulletHitEvent evnt) {
            this.ShotsHit++;
        }

        public override void OnWin(WinEvent evnt) {
            PrintStats();
        }

        public override void OnDeath(DeathEvent evnt) {
            PrintStats();
        }

        private void PrintStats() {
            this.Out.WriteLine("{0} / {1} shots hit. ({2:0.0}%)", this.ShotsHit, this.ShotsFired, 100.0 * this.ShotsHit / this.ShotsFired);
        }

        private readonly Pen seenEnemyPen = new Pen(Color.LightGreen, 3);
        private readonly Pen projectedEnemyPen = new Pen(Color.MistyRose, 3);
        private readonly Pen lastShotTargetPen = new Pen(Color.OrangeRed, 3);
        public override void OnPaint(IGraphics graphics) {
            if (this.LastFoeState.HasValue) {
                graphics.DrawLine(seenEnemyPen, this.Location, this.LastFoeState.Value.Location);
                graphics.DrawLine(projectedEnemyPen, this.Location, this.GetProjectedFoeLocation(0));
            }
            if (this.LastShotTarget.HasValue && this.LastShotSource.HasValue) {
                graphics.DrawLine(lastShotTargetPen, this.LastShotSource.Value, this.LastShotTarget.Value);
            }
        }

        protected double RadarHeadingRadians {
            get { return Geometry.DegreesToRadians(base.RadarHeading); }
        }

        protected double HeadingRadians {
            get { return Geometry.DegreesToRadians(base.Heading); }
        }
    }
}
