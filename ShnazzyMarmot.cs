using System;
using System.Collections.Generic;
using System.Linq;
using Robocode;
using System.Drawing;

namespace TizzleTazzle {
    class ShnazzyMarmot : StylishBot {
        class PredictorStats {
		    public int Misses = 0;
            public int Hits = 0;

            public int TotalShots { get { return this.Misses + this.Hits; }}
	    }

        private bool FiredOnLastSighting = true;
        private BotState? LastFoeState = null;
        private PointF? LastShotTarget = null;
        private PointF? LastShotSource = null;
        private List<PointF> LocationHistory = null;
        private int ShotsFired = 0;
        private int ShotsHit = 0;
        private Random rng = new Random();
        private Dictionary<Bullet, ITargetPredictor> BulletStrategies = new Dictionary<Bullet, ITargetPredictor>();
        private Dictionary<ITargetPredictor, PredictorStats> Predictors = new Dictionary<ITargetPredictor, PredictorStats> {
            {new LinearTargetPredictor(),       new PredictorStats()},
            {new FixedVelocityPredictor(8),     new PredictorStats()},
            {new FixedVelocityPredictor(0),     new PredictorStats()},
            {new RandomRadiusPredictor(2),      new PredictorStats()},
            {new FixedVelocityPredictor(4),     new PredictorStats()},
            {new FixedVelocityPredictor(-2),    new PredictorStats()},
        };

        public const string VERSION = "1.0.2"; // reflection to get assembly version fails in arena
        public void ShowVersion() {
            this.Out.WriteLine("Shnazzy Marmot {0}", VERSION);
        }

        public override void Run() {
            this.ShowVersion();
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
            this.LocationHistory.Add(this.Location);

            while (this.FiredOnLastSighting) {
                base.TurnRadarRight(360);
            }

            var bounds = this.GetRobotBounds();
            int variance = (int)(this.DistanceTo(bounds.GetCenter()) * 0.2);
            this.TurnTo(this.GetHeadingTo(this.LastFoeState.Value.Location) + 90 + rng.Next(-variance, variance + 1));

            this.MakeRefinedShot();

            this.Drive();
        }

        private void Drive() {
            const double DRIVE_DISTANCE = 120;

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
        }

        private void MakeRefinedShot() {
            const double TARGET_ACCURACY = 1;
            const int AIM_TURNS = 2;

            if (this.GunHeat > 0) return;

            ITargetPredictor predictor = this.GetBestPredictor();
            this.Out.WriteLine("{0} selected.", predictor.GetDescription());

            PointF lastTarget;
            PointF target = predictor.Predict(this.LastFoeState.Value, AIM_TURNS);
            this.TurnGunTo(target);

            double dist = this.GetDistanceTo(target);
            double power = GetShotPower(dist);
            double shotSpeed = GetShotSpeed(power);

            int iterations = 0;
            do {
                int timeToTarget = (int)(dist / shotSpeed) + AIM_TURNS;

                lastTarget = target;
                target = predictor.Predict(this.LastFoeState.Value, timeToTarget);
                dist = this.GetDistanceTo(target);
            } while (Geometry.Distance(target, lastTarget) > TARGET_ACCURACY && ++iterations < 10);

            this.LastShotTarget = target;
            this.LastShotSource = this.Location;
            this.TurnGunTo(target);
            if (this.GunHeat > 0) return;
            var bullet = base.FireBullet(power);
            if (bullet == null) return;
            this.BulletStrategies[bullet] = predictor;
            this.FiredOnLastSighting = true;

            this.ShotsFired++;
        }

        private ITargetPredictor GetBestPredictor() {
            return (
                from pair in this.Predictors
                let predictor = pair.Key
                let stats = pair.Value
                orderby (stats.Hits + 1.0) / (stats.Misses + 2.0) descending
                select predictor
            ).First();
        }

        private static double GetShotPower(double dist) {
            return Geometry.Clamp(300 / dist, .1, 3);
        }

        public override void OnBulletMissed(BulletMissedEvent evnt) {
            var predictor = this.BulletStrategies[evnt.Bullet];
            this.Predictors[predictor].Misses += 1;
        }

        public override void OnScannedRobot(ScannedRobotEvent evnt) {
            this.LastFoeState = this.GetFoeState(evnt);
            this.FiredOnLastSighting = false;
        }

        public override void OnBulletHit(BulletHitEvent evnt) {
            var predictor = this.BulletStrategies[evnt.Bullet];
            this.Predictors[predictor].Hits += 1;

            this.ShotsHit++;
        }

        public override void OnWin(WinEvent evnt) {
            PrintStats();
        }

        public override void OnDeath(DeathEvent evnt) {
            PrintStats();
        }

        private void PrintStats() {
            foreach (var pair in this.Predictors) {
                this.Out.WriteLine("{0} {1} / {2}", pair.Key.GetDescription(), pair.Value.Hits, pair.Value.TotalShots);
            }

            this.Out.WriteLine("{0} / {1} shots hit. ({2:0.0}%)", this.ShotsHit, this.ShotsFired, 100.0 * this.ShotsHit / this.ShotsFired);
        }

        private readonly Pen seenEnemyPen = new Pen(Color.WhiteSmoke, 3);
        private readonly Pen projectedEnemyPen = new Pen(Color.Gray, 3);
        private readonly Pen lastShotTargetPen = new Pen(Color.OrangeRed, 3);
        public override void OnPaint(IGraphics graphics) {
            if (this.LastFoeState.HasValue) {
                foreach (var pair in this.Predictors) {
                    var predicted = pair.Key.Predict(this.LastFoeState.Value, 0);
                    graphics.DrawLine(projectedEnemyPen, this.Location, predicted);
                }
                graphics.DrawLine(seenEnemyPen, this.Location, this.LastFoeState.Value.Location);
            }
            if (this.LastShotTarget.HasValue && this.LastShotSource.HasValue) {
                graphics.DrawLine(lastShotTargetPen, this.LastShotSource.Value, this.LastShotTarget.Value);
            }
            if (this.AheadPoint.HasValue && this.BehindPoint.HasValue) {
                graphics.DrawLine(Pens.MediumPurple, this.Location, this.BehindPoint.Value);
                graphics.DrawLine(Pens.MediumPurple, this.Location, this.AheadPoint.Value);
            }
        }
    }
}
