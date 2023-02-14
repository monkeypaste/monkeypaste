namespace MonkeyPaste.Common {
    public static class MpTriangleExtensions {
        // from https://stackoverflow.com/a/2049593/105028
        private static double sign(MpPoint p1, MpPoint p2, MpPoint p3) {
            return (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);
        }

        private static bool PointInTriangle(MpPoint pt, MpPoint v1, MpPoint v2, MpPoint v3) {
            double d1, d2, d3;
            bool has_neg, has_pos;

            d1 = sign(pt, v1, v2);
            d2 = sign(pt, v2, v3);
            d3 = sign(pt, v3, v1);

            has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

            return !(has_neg && has_pos);
        }

        public static bool Contains(this MpTriangle triangle, MpPoint p) {
            return PointInTriangle(p, triangle.P1, triangle.P2, triangle.P3);
        }
    }
}
