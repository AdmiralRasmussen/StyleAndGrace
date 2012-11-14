using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace TizzleTazzle {
    interface ITargetPredictor {
        PointF Predict(IList<BotState> state, double turns);
        string GetDescription();
    }

    class LinearTargetPredictor : ITargetPredictor {
        public PointF Predict(IList<BotState> states, double turns) {
            var state = states.Last();
            return state.GetProjectedLocation(turns);
        }

        public string GetDescription() {
            return "Linear targeting";
        }
    }

    class FixedVelocityPredictor : ITargetPredictor {
        private double Velocity;
        public FixedVelocityPredictor(double velocity) {
            this.Velocity = velocity;
        }

        public PointF Predict(IList<BotState> states, double turns) {
            var state = states.Last();
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

        public RandomRadiusPredictor(int maxRadiusExpansion) {
            this.MaxRadiusExpansion = maxRadiusExpansion;
        }

        public PointF Predict(IList<BotState> states, double turns) {
            var state = states.Last();
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
        public CircularPredictor(bool verbose = false) {
            this.Verbose = verbose;
        }

        public PointF Predict(IList<BotState> states, double turns) {
            var last = states.Last();
            if (states.Count < 2) return last.Location;
            var previous = states[states.Count - 2];

            double turnRate = (last.Heading - previous.Heading) / (last.Turn - previous.Turn);
            double velocity = (last.Velocity + previous.Velocity) / 2;
            double totalTurns = last.Age + turns;

            if (this.Verbose) last.Observer.Out.WriteLine("Circular predictor: velocity: {0} totalTurns: {1}", velocity, totalTurns);

            if (Math.Abs(turnRate) < .1) return last.GetProjectedLocation(totalTurns, velocity);

            double timeToCompleteCircle = 360 / Math.Abs(turnRate);
            double circumference = velocity * timeToCompleteCircle;
            double radius = circumference / 2 / Math.PI;

            PointF center = last.Location.ShiftBy(last.Heading + 90 * Math.Sign(turnRate), radius, last.Observer.GetArenaBounds());

            if (this.Verbose) last.Observer.Out.WriteLine("Circular predictor: radius: {0} center: {1}", radius, center);

            double angularVelocityRadians = velocity / radius;
            double startAngleRadians = Math.Atan2(last.Location.X - center.X, last.Location.Y - center.Y);
            double endAngleRadians = startAngleRadians + angularVelocityRadians * Math.Sign(turnRate) * totalTurns;
            double endAngleDegrees = Geometry.RadiansToDegrees(endAngleRadians);

            if (this.Verbose) last.Observer.Out.WriteLine("Circular predictor: startAngleRadians: {0}, endAngleRadians: {1}", startAngleRadians, endAngleRadians);

            var result = center.ShiftBy(endAngleDegrees, radius, last.Observer.GetArenaBounds());

            if (this.Verbose) last.Observer.Out.WriteLine("Circular predictor: center: {0}, result: {1}", center, result);
            return result;
        }

        public string GetDescription() {
            return "Circular";
        }
    }
}
