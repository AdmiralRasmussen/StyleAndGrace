using System;
using System.Drawing;
using Robocode;

namespace TizzleTazzle {
    static class Geometry {
        public static double DegreesToRadians(double degrees) {
            return degrees * 2 * Math.PI / 360;
        }

        public static double RadiansToDegrees(double radians) {
            return radians / 2 / Math.PI * 360;
        }

        public static double Distance(PointF a, PointF b) {
            return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        public static double Clamp(double val, double low, double high) {
            return Math.Min(Math.Max(val, low), high);
        }

        public static PointF ShiftBy(this PointF point, double heading, double distance, RectangleF bounds) {
            double headingRadians = DegreesToRadians(heading);
            double targetX = Geometry.Clamp(point.X + distance * Math.Sin(headingRadians), bounds.Left, bounds.Right);
            double targetY = Geometry.Clamp(point.Y + distance * Math.Cos(headingRadians), bounds.Top, bounds.Bottom);

            return Geometry.MakePoint(targetX, targetY);
        }

        public static PointF MakePoint(double x, double y) {
            return new PointF((float)x, (float)y);
        }

        public static PointF GetCenter(this RectangleF bounds) {
            return MakePoint((bounds.Right - bounds.Left) / 2, (bounds.Bottom - bounds.Top) / 2);
        }

        public static RectangleF GetRobotBounds(this Robot bot) {
            return new RectangleF(
                (float)bot.Width / 2, (float)bot.Height / 2,
                (float)(bot.BattleFieldWidth - bot.Width), (float)(bot.BattleFieldHeight - bot.Height));
        }
    }
}
