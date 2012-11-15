using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Robocode;

namespace TizzleTazzle {
    interface ITargetPredictor {
        PointF Predict(double turns, IGraphics graphics= null);
        string GetDescription();
    }

    class LinearTargetPredictor : ITargetPredictor {
        private IList<BotState> History;
        public LinearTargetPredictor(IList<BotState> state) {
            this.History = state;
        }

        public PointF Predict(double turns, IGraphics graphics) {
            var state = History.Last();
            return state.GetProjectedLocation(turns);
        }

        public string GetDescription() {
            return "Linear targeting";
        }
    }

    class FixedVelocityPredictor : ITargetPredictor {
        private double Velocity;
        private IList<BotState> History;
        public FixedVelocityPredictor(IList<BotState> state, double velocity) {
            this.History = state;
            this.Velocity = velocity;
        }

        public PointF Predict(double turns, IGraphics graphics) {
            var state = History.Last();
            double distance = (state.Observer.Time - state.Turn + turns) * this.Velocity * Math.Sign(state.Velocity);
            return state.Location.ShiftBy(state.Heading, distance, state.Observer.GetArenaBounds());
        }

        public string GetDescription() {
            return string.Format("Fixed velocity({0})", this.Velocity);
        }
    }

    class RandomRadiusPredictor : ITargetPredictor {
        private double MaxRadiusExpansion;
        private Random rng = new Random();
        private IList<BotState> History;

        public RandomRadiusPredictor(IList<BotState> state, int maxRadiusExpansion) {
            this.History = state;
            this.MaxRadiusExpansion = maxRadiusExpansion;
        }

        public PointF Predict(double turns, IGraphics graphics) {
            var state = History.Last();
            double maxRadius = (state.Observer.Time - state.Turn + turns) * this.MaxRadiusExpansion;
            double distance = rng.NextDouble() * maxRadius;
            double heading = rng.NextDouble() * 360;

            return state.Location.ShiftBy(heading, distance, state.Observer.GetArenaBounds());
        }

        public string GetDescription() {
            return string.Format("Random Radius ({0})", this.MaxRadiusExpansion);
        }
    }

    class CircularPredictor : ITargetPredictor {
        private bool Verbose;
        private IList<BotState> History;
        public CircularPredictor(IList<BotState> state, bool verbose = false) {
            this.History = state;
            this.Verbose = verbose;
        }

        public PointF Predict(double turns, IGraphics graphics) {
            if (History.Count < 2) return History.Last().Location;

            var last = History.Last();
            var previous = History[History.Count - 2];

            double turnRate = Geometry.NormalizeHeading(last.Heading - previous.Heading) / (last.Turn - previous.Turn);
            double velocity = (last.Velocity + previous.Velocity) / 2;
            double totalTurns = last.Age + turns;

            if (Math.Abs(turnRate) < .01) return last.Location;

            if (this.Verbose) {
                last.Observer.Out.WriteLine("");
                last.Observer.Out.WriteLine("Circular predictor:");
                last.Observer.Out.WriteLine("last: {0} {1} {2}", last.Location, last.Heading, last.Turn);
                last.Observer.Out.WriteLine("previous: {0} {1} {2}", previous.Location, previous.Heading, previous.Turn);
                last.Observer.Out.WriteLine("turnRate: {0}", turnRate);
                last.Observer.Out.WriteLine("velocity: {0} totalTurns: {1}", velocity, totalTurns);
            }

            double timeToCompleteCircle = 360 / Math.Abs(turnRate);
            double circumference = velocity * timeToCompleteCircle;
            double radius = circumference / 2 / Math.PI;

            PointF center = last.Location.ShiftBy(last.Heading + 90 * Math.Sign(turnRate), radius, last.Observer.GetArenaBounds());

            if (graphics != null) {
                float diameter = 2 * (float)radius;
                graphics.DrawEllipse(Pens.MintCream, center.X - (float)radius, center.Y - (float)radius, diameter, diameter);
            }

            if (this.Verbose) last.Observer.Out.WriteLine("radius: {0} center: {1}", radius, center);

            double angularVelocityRadians = velocity / radius;
            double startAngleRadians = Math.Atan2(last.Location.X - center.X, last.Location.Y - center.Y);
            double endAngleRadians = startAngleRadians + angularVelocityRadians * Math.Sign(turnRate) * totalTurns;
            double endAngleDegrees = Geometry.RadiansToDegrees(endAngleRadians);

            if (this.Verbose) last.Observer.Out.WriteLine("startAngleRadians: {0}, endAngleRadians: {1}", startAngleRadians, endAngleRadians);

            var result = center.ShiftBy(endAngleDegrees, radius, last.Observer.GetArenaBounds());

            if (this.Verbose) last.Observer.Out.WriteLine("center: {0}, result: {1}", center, result);
            return result;
        }

        public string GetDescription() {
            return "Circular";
        }
    }
}
