using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robocode;
using System.Drawing;

namespace TizzleTazzle {
    abstract class GrumpyTortoise : StylishBot {
        private BotState? LastSeenFoe = null;

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
                if (this.LastSeenFoe.Value.Age > 7) this.FoeSweepSearch();

                this.MakeRefinedShot();
            }
        }

        public override void OnScannedRobot(ScannedRobotEvent evnt) {
            if (this.LastSeenFoe != null) {
                if (evnt.Energy < this.LastSeenFoe.Value.Energy) {
                    // enemy fired
                }
            }

            this.LastSeenFoe = this.GetFoeState(evnt);
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
