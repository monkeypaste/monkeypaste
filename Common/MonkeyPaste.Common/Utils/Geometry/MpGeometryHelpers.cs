using System.Collections.Generic;
using System.Linq;

namespace MonkeyPaste.Common {
    public static class MpGeometryHelpers {
        public static double GetDeterminant(double x1, double y1, double x2, double y2) {
            return x1 * y2 - x2 * y1;
        }

        public static double GetArea(IList<MpPoint> vertices) {
            if (vertices.Count < 3) {
                return 0;
            }
            double area = GetDeterminant(vertices[vertices.Count - 1].X, vertices[vertices.Count - 1].Y, vertices[0].X, vertices[0].Y);
            for (int i = 1; i < vertices.Count; i++) {
                area += GetDeterminant(vertices[i - 1].X, vertices[i - 1].Y, vertices[i].X, vertices[i].Y);
            }
            return area / 2;
        }

        public static double cross(MpPoint O, MpPoint A, MpPoint B) {
            return (A.X - O.X) * (B.Y - O.Y) - (A.Y - O.Y) * (B.X - O.X);
        }

        public static List<MpPoint> GetConvexHull(List<MpPoint> points) {
            if (points == null)
                return null;

            if (points.Count() <= 1)
                return points;

            int n = points.Count(), k = 0;
            List<MpPoint> H = new List<MpPoint>(new MpPoint[2 * n]);

            points.Sort((a, b) =>
                 a.X == b.X ? a.Y.CompareTo(b.Y) : a.X.CompareTo(b.X));

            // Build lower hull
            for (int i = 0; i < n; ++i) {
                while (k >= 2 && cross(H[k - 2], H[k - 1], points[i]) <= 0)
                    k--;
                H[k++] = points[i];
            }

            // Build upper hull
            for (int i = n - 2, t = k + 1; i >= 0; i--) {
                while (k >= t && cross(H[k - 2], H[k - 1], points[i]) <= 0)
                    k--;
                H[k++] = points[i];
            }

            return H.Take(k - 1).ToList();
        }
    }
}
