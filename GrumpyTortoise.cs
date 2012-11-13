using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robocode;
using System.Drawing;

namespace TizzleTazzle {
    class GrumpyTortoise : StylishBot {
        private BotState? LastSeenFoe = null;
        private double ExpectedFoeEnergy = double.MinValue;

        public override void Run() {
            this.SetAllColors(Color.OliveDrab);
            this.IsAdjustGunForRobotTurn = false;
            this.IsAdjustRadarForGunTurn = false;
            this.IsAdjustRadarForRobotTurn = false;

            this.FoeSweepSearch();

            while (true) {
                double radarHeading = this.RadarHeading;
                this.TurnRadarTo(this.LastSeenFoe.Value.GetProjectedLocation(0));
                if (this.RadarHeading == RadarHeading) this.Scan();

                if (this.LastSeenFoe.Value.Age < 3) this.MakeRefinedShot();
                else this.FoeSweepSearch();

                
            }
        }

        Random rng = new Random();
        int Direction = 1;
        public override void OnScannedRobot(ScannedRobotEvent evnt) {
            if (this.LastSeenFoe != null) {
                if (evnt.Energy < this.ExpectedFoeEnergy - .001) {
                    // enemy fired

                    this.Ahead(this.Direction * rng.Next(50, 150));

                    this.Direction *= -1;
                }
            }

            this.LastSeenFoe = this.GetFoeState(evnt);
            this.ExpectedFoeEnergy = evnt.Energy;
        }

        public override void OnBulletHit(BulletHitEvent evnt) {
            this.ExpectedFoeEnergy -= evnt.Bullet.GetDamage();
        }

        private void MakeRefinedShot() {
            const double HEADING_THRESHOLD = 2;
            const double HEADING_ACCURACY = .1;

            PointF currentLocation = this.Location;
            double lastHeading;
            PointF target = this.LastSeenFoe.Value.GetProjectedLocation();
            double heading = this.GetHeadingTo(target);

            double dist = Geometry.Distance(currentLocation, target);
            double power = 2;
            double shotSpeed = GetShotSpeed(power);

            do {
                double timeToTarget = dist / shotSpeed;

                target = this.LastSeenFoe.Value.GetProjectedLocation(timeToTarget);
                lastHeading = heading;
                heading = this.GetHeadingTo(target);
                dist = this.GetDistanceTo(target);
            } while (Math.Abs(heading - lastHeading) > HEADING_ACCURACY);

            this.TurnGunTo(target, HEADING_THRESHOLD);
            if(this.GunHeat == 0) base.Fire(power);
        }

        protected void FoeSweepSearch() {
            this.LastSeenFoe = null;
            while (!this.LastSeenFoe.HasValue) this.TurnRadarRight(45);
        }
    }
}
