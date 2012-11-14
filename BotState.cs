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
        public double Energy;

        public BotState(Robot bot, ScannedRobotEvent evnt) {
            var direction = bot.Heading + evnt.Bearing;

            this.Observer = bot;
            this.Energy = evnt.Energy;
            this.Location = Geometry.ShiftBy(bot.GetLocation(), direction, evnt.Distance, bot.GetArenaBounds());
            this.Heading = evnt.Heading;
            this.Velocity = evnt.Velocity;
            this.Turn = bot.Time;
        }

        public double HeadingRadians {
            get { return Geometry.DegreesToRadians(this.Heading); }
        }

        public PointF GetProjectedLocation(double turnsInFuture = 0, double velocityOverride = 0) {
            double elapsed = this.Observer.Time - this.Turn;
            double traveled = (elapsed + turnsInFuture) * velocityOverride;

            return this.Location.ShiftBy(this.Heading, traveled, this.Observer.GetArenaBounds());
        }

        public long Age { get { return this.Observer.Time - this.Turn; } }
    }
}
