using System;
using System.Linq;

namespace MonkeyPaste.Common {
    public static class MpRectExtensions {

        public static MpTriangle[] ToFaces(this MpRect rect) {
            // NOTE wrapping clock-wise

            if (rect == null) {
                return new MpTriangle[] { };
            }
            MpPoint c = rect.Centroid();

            return new MpTriangle[] {
                //BR+BL (Bottom)
                new MpTriangle(rect.BottomRight,rect.BottomLeft,c),
                //TR+BR (Right)
                new MpTriangle(rect.TopRight,rect.BottomRight,c),
                //TL+TR (Top)
                new MpTriangle(rect.TopLeft,rect.TopRight,c),
                //BL+TL (Left)
                new MpTriangle(rect.BottomLeft,rect.TopLeft,c)
            };
        }

        public static MpPoint Centroid(this MpRect rect) {
            if (rect == null) {
                return MpPoint.Zero;
            }
            double mid_x = rect.Left + (rect.Width / 2);
            double mid_y = rect.Top + (rect.Height / 2);
            return new MpPoint(mid_x, mid_y);
        }
        public static bool FuzzyEquals(this MpRect rect, MpRect otherRect, double maxThresh = 0.1d) {
            if (rect == null && otherRect == null) {
                return true;
            }
            if (rect == null || otherRect == null) {
                return false;
            }
            bool isMatch_l = Math.Abs(rect.Left - otherRect.Left) <= maxThresh;
            bool isMatch_t = Math.Abs(rect.Top - otherRect.Top) <= maxThresh;
            bool isMatch_r = Math.Abs(rect.Right - otherRect.Right) <= maxThresh;
            bool isMatch_b = Math.Abs(rect.Bottom - otherRect.Bottom) <= maxThresh;
            return isMatch_l && isMatch_t && isMatch_r && isMatch_b;
        }

        public static bool IsAnyPointWithinOtherRect(this MpRect rect, MpRect otherRect) {
            return otherRect.Contains(rect.TopLeft) ||
                    otherRect.Contains(rect.TopRight) ||
                    otherRect.Contains(rect.BottomLeft) ||
                    otherRect.Contains(rect.BottomRight);
        }

        public static bool IsAllPointWithinOtherRect(this MpRect rect, MpRect otherRect) {
            return otherRect.Contains(rect.TopLeft) &&
                    otherRect.Contains(rect.TopRight) &&
                    otherRect.Contains(rect.BottomLeft) &&
                    otherRect.Contains(rect.BottomRight);
        }

        public static bool Contains(this MpRect rect, MpPoint p) {
            return p.X >= rect.Left && p.X <= rect.Right &&
                   p.Y >= rect.Top && p.Y <= rect.Bottom;
        }

        public static bool Contains(this MpRect rect, MpRect other) {
            return rect.Contains(other.TopLeft) && rect.Contains(other.BottomRight);
        }
        public static bool Intersects(this MpRect rect, MpRect other) {
            return other.Points.Any(x => rect.Contains(x));
        }

        public static void Union(this MpRect rect, MpRect b) {
            rect.Left = Math.Min(rect.Left, b.Left);
            rect.Top = Math.Min(rect.Top, b.Top);
            rect.Right = Math.Max(rect.Right, b.Right);
            rect.Bottom = Math.Max(rect.Bottom, b.Bottom);
        }

        public static MpRectSideHitTest GetClosestSideToPoint(this MpRect rect, MpPoint p, string excludedSideLabelsCsv = "") {
            double l_dist = Math.Abs(rect.Left - p.X);
            double t_dist = Math.Abs(rect.Top - p.Y);
            double r_dist = Math.Abs(rect.Right - p.X);
            double b_dist = Math.Abs(rect.Bottom - p.Y);
            double[] side_dist_a = new double[] { l_dist, t_dist, r_dist, b_dist };

            var excludedSideIdxs = excludedSideLabelsCsv
                .Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => MpRectSideHitTest.GetSideIdx(x)).ToArray();

            int min_idx = -1;
            double min_dist = double.MaxValue;
            for (int i = 0; i < side_dist_a.Length; i++) {
                if (excludedSideIdxs.Contains(i)) {
                    continue;
                }
                double cur_dist = side_dist_a[i];
                if (cur_dist < min_dist) {
                    min_dist = cur_dist;
                    min_idx = i;
                }
            }
            return new MpRectSideHitTest() {
                ClosestSideDistance = min_dist,
                ClosestSideLabel = MpRectSideHitTest.GetSideLabel(min_idx),
                TestRect = rect
            };
        }

        public static MpPoint GetCornerByLabel(this MpRect rect, string cornerLabel) {
            cornerLabel = cornerLabel.ToLower();
            switch (cornerLabel) {
                case "tl":
                    return rect.TopLeft;
                case "tr":
                    return rect.TopRight;
                case "bl":
                    return rect.BottomLeft;
                case "br":
                    return rect.BottomRight;
            }
            return MpPoint.Zero;
        }

        public static MpLine GetSideByLabel(this MpRect rect, string sideLabel) {
            if (string.IsNullOrEmpty(sideLabel)) {
                MpConsole.WriteTraceLine("Error! sidelabel is null or empty");
                return MpLine.Empty;
            }
            switch (sideLabel.ToLower()) {
                case "l":
                    return new MpLine(rect.TopLeft, rect.BottomLeft);
                case "t":
                    return new MpLine(rect.TopLeft, rect.TopRight);
                case "r":
                    return new MpLine(rect.TopRight, rect.BottomRight);
                case "b":
                    return new MpLine(rect.BottomLeft, rect.BottomRight);
                default:
                    MpConsole.WriteTraceLine("Error! Unknown MpRect sidelabel (returning null): " + sideLabel);
                    return MpLine.Empty; ;
            }
        }
    }
}
