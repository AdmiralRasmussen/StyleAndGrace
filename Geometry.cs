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
            if (double.IsNaN(val)) return low;
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

        public static double GetDamage(this Bullet bullet) {
            double damage = bullet.Power * 4;
            if (bullet.Power > 1) damage += (bullet.Power - 1) * 2;
            return damage;
        }

        public static PointF MidPointWith(this PointF a, PointF b) {
            return MakePoint((a.X + b.X) / 2, (a.Y + b.Y) / 2);
        }

        public static RectangleF GetArenaBounds(this Robot bot) {
            return new RectangleF(
                (float)bot.Width / 2, (float)bot.Height / 2,
                (float)(bot.BattleFieldWidth - bot.Width), (float)(bot.BattleFieldHeight - bot.Height));
        }

        public static PointF GetLocation(this Robot bot) {
            return Geometry.MakePoint(bot.X, bot.Y);
        }

        public static double NormalizeHeading(double a) {
            while (a > 180) a -= 360;
            while (a < -180) a += 360;
            return a;
        }

        public static double GetShotSpeed(double power) {
            return 20 - (3 * power); // http://robowiki.net/wiki/Robocode/FAQ
        }

        public static bool TurnGunTo(this Robot bot, double heading, double headingThreshold = 0) {
            double turn = NormalizeHeading(heading - bot.GunHeading);
            if (Math.Abs(turn) > headingThreshold) {
                bot.TurnGunRight(turn);
                return turn != 0;
            } else {
                return false;
            }
        }

        public static bool TurnGunTo(this Robot bot, PointF target, double headingThreshold = 0) {
            return bot.TurnGunTo(bot.GetHeadingTo(target), headingThreshold);
        }

        public static double GetHeadingTo(this Robot bot, PointF target) {
            return Geometry.RadiansToDegrees(Math.Atan2(target.X - bot.X, target.Y - bot.Y));
        }

        public static bool TurnTo(this Robot bot, double heading) {
            double turn = NormalizeHeading(heading - bot.Heading);
            bot.TurnRight(turn);
            return turn != 0;
        }

        public static bool TurnRadarTo(this Robot bot, double heading) {
            double turn = NormalizeHeading(heading - bot.RadarHeading);
            bot.TurnRadarRight(turn);
            return turn != 0;
        }

        public static bool TurnRadarTo(this Robot bot, PointF target) {
            return bot.TurnRadarTo(bot.GetHeadingTo(target));
        }

        public static double DistanceTo(this Robot bot, PointF dest) {
            return Geometry.Distance(bot.GetLocation(), dest);
        }

        public static double GetDistanceTo(this Robot bot, PointF target) {
            return Geometry.Distance(bot.GetLocation(), target);
        }
    }
}
