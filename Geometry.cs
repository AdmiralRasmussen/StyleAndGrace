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

        public static PointF ShiftBy(PointF point, double heading, double distance, RectangleF bounds) {
            double targetX = Geometry.Clamp(point.X + distance * Math.Sin(heading), bounds.Left, bounds.Right);
            double targetY = Geometry.Clamp(point.Y + distance * Math.Cos(heading), bounds.Top, bounds.Bottom);

            return Geometry.MakePoint(targetX, targetY);
        }

        public static PointF MakePoint(double x, double y) {
            return new PointF((float)x, (float)y);
        }

        public static PointF GetCenter(this RectangleF bounds) {
            return MakePoint((bounds.Right - bounds.Left) / 2, (bounds.Bottom - bounds.Top) / 2);
        }
    }
}
