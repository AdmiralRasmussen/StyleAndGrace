using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace TizzleTazzle {
    interface ITargetPredictor {
        PointF Predict(BotState state, double turns);
        string GetDescription();
    }

    class LinearTargetPredictor : ITargetPredictor {
        public PointF Predict(BotState state, double turns) {
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

        public PointF Predict(BotState state, double turns) {
            double distance = (state.Observer.Time - state.Turn + turns) * this.Velocity * Math.Sign(state.Velocity);
            return state.Location.ShiftBy(state.Heading, distance, state.Observer.GetRobotBounds());
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

        public PointF Predict(BotState state, double turns) {
            double maxRadius = (state.Observer.Time - state.Turn + turns) * this.MaxRadiusExpansion;
            double distance = rng.NextDouble() * maxRadius;
            double heading = rng.NextDouble() * 360;

            return state.Location.ShiftBy(heading, distance, state.Observer.GetRobotBounds());
        }

        public string GetDescription() {
            return string.Format("Random Radius ({0})", this.MaxRadiusExpansion);
        }
    }
}
