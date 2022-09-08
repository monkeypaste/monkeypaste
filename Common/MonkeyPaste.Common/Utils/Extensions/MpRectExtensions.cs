using MonkeyPaste.Common;
using MonkeyPaste.Common.Utils.Extensions;
using System;
using System.Linq;

namespace MonkeyPaste.Common.Utils.Extensions {
    public static class MpRectExtensions {
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
