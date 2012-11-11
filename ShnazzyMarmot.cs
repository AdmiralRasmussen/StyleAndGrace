using System;
using System.Collections.Generic;
using System.Linq;
using Robocode;
using System.Drawing;

namespace TizzleTazzle {
    enum TargetStrategy {
        CurrentVelocity,
        PeggedVelocity,
    }

    class ShnazzyMarmot : StylishBot {
        private bool FiredOnLastSighting = true;
        private BotState? LastFoeState = null;
        private PointF? LastShotTarget = null;
        private PointF? LastShotSource = null;
        private List<PointF> LocationHistory = null;
        private int ShotsFired = 0;
        private int ShotsHit = 0;
        private Random rng = new Random();
        private Dictionary<Bullet, TargetStrategy> BulletStrategies = new Dictionary<Bullet, TargetStrategy>();
        private TargetStrategy CurrentStrategy = TargetStrategy.PeggedVelocity;

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

        private PointF? AheadPoint = null;
        private PointF? BehindPoint = null;
        private void RunBody() {
            const double DRIVE_DISTANCE = 120;

            while (this.FiredOnLastSighting) {
                base.TurnRadarRight(360);
            }

            var bounds = this.GetRobotBounds();
            PointF center = bounds.GetCenter();

            PointF ahead = Geometry.ShiftBy(this.Location, this.Heading, DRIVE_DISTANCE, bounds);
            PointF behind = Geometry.ShiftBy(this.Location, this.Heading, -DRIVE_DISTANCE, bounds);
            this.AheadPoint = ahead;
            this.BehindPoint = behind;

            double distanceAhead = Geometry.Distance(ahead, center);
            double distanceBehind = Geometry.Distance(behind, center);
            bool aheadCloser = distanceAhead <= distanceBehind;

            if (aheadCloser) {
                base.Ahead(DRIVE_DISTANCE);
            } else {
                base.Back(DRIVE_DISTANCE);
            }
            
            this.LocationHistory.Add(this.Location);

            int variance = (int)(this.DistanceTo(bounds.GetCenter()) * 0.2);
            this.TurnTo(this.GetHeadingTo(this.LastFoeState.Value.Location) + 90 + rng.Next(-variance, variance + 1));

            this.MakeRefinedShot();
        }

        private void MakeRefinedShot() {
            const double TARGET_ACCURACY = 1;
            const int AIM_TURNS = 2;

            PointF lastTarget;
            PointF target = this.GetProjectedFoeLocation(AIM_TURNS);
            this.TurnGunTo(target);

            double dist = this.GetDistanceTo(target);
            double power = GetShotPower(dist);
            double shotSpeed = GetShotSpeed(power);

            int iterations = 0;
            do {
                int timeToTarget = (int)(dist / shotSpeed) + AIM_TURNS;

                lastTarget = target;
                target = this.GetProjectedFoeLocation(timeToTarget);
                dist = this.GetDistanceTo(target);
            } while (Geometry.Distance(target, lastTarget) > TARGET_ACCURACY && ++iterations < 10);

            this.LastShotTarget = target;
            this.LastShotSource = this.Location;
            this.TurnGunTo(target);
            var bullet = base.FireBullet(power);
            this.BulletStrategies[bullet] = this.CurrentStrategy;
            this.FiredOnLastSighting = true;

            this.ShotsFired++;
        }

        private static double GetShotPower(double dist) {
            return Geometry.Clamp(300 / dist, .1, 3);
        }

        private PointF GetProjectedFoeLocation(int turns) {
            if (!this.LastFoeState.HasValue) return PointF.Empty;

            var velocity = this.LastFoeState.Value.Velocity;
            if (velocity > 0 && this.CurrentStrategy == TargetStrategy.PeggedVelocity) velocity = 8;

            return this.LastFoeState.Value.GetProjectedLocation(turns, velocity);
        }

        public override void OnBulletMissed(BulletMissedEvent evnt) {
            switch (this.BulletStrategies[evnt.Bullet]) {
                case TargetStrategy.CurrentVelocity:
                    this.CurrentStrategy = TargetStrategy.PeggedVelocity;
                    break;

                case TargetStrategy.PeggedVelocity:
                    this.CurrentStrategy = TargetStrategy.CurrentVelocity;
                    break;
            }

            this.Out.WriteLine("Bullet missed. Switched to {0}", this.CurrentStrategy);
        }

        public override void OnScannedRobot(ScannedRobotEvent evnt) {
            this.LastFoeState = this.GetFoeState(evnt);
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
            if (this.AheadPoint.HasValue && this.BehindPoint.HasValue) {
                graphics.DrawLine(Pens.MediumPurple, this.Location, this.BehindPoint.Value);
                graphics.DrawLine(Pens.White, this.Location, this.AheadPoint.Value);
            }
        }
    }
}
