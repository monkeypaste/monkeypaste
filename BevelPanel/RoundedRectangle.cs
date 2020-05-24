using System.Drawing;
using System.Drawing.Drawing2D;

namespace BeveledPanel {
    public class RoundedRectangle {
        public static GraphicsPath DrawRoundedRectanglePath(Rectangle rect, int radius, bool roundTop, bool roundBottom) {
            return DrawRoundedRectanglePath(rect, radius, false,roundTop,roundBottom);
        }
        public static GraphicsPath DrawRoundedRectanglePath(Rectangle rect,int radius, bool dropStyle, bool roundTop, bool roundBottom) {
            int x = rect.X;
            int y = rect.Y;
            int width = rect.Width;
            int height = rect.Height;

            int xw = x + width;
            int yh = y + height;
            int xwr = xw - radius;
            int yhr = yh - radius;
            int xr = x + radius;
            int yr = y + radius;
            int r2 = radius * 2;

            int xwr2 = xw - r2;
            int yhr2 = yh - r2;
            int xw2 = x + width / 2;
            int yh10 = yh - height / 20;

            GraphicsPath p = new GraphicsPath();
            p.StartFigure();

            if(roundTop) {
                //Top Left Corner
                if (r2 > 0) {
                    p.AddArc(x, y, r2, r2, 180, 90);
                }
                //Top Edge
                p.AddLine(xr, y, xwr, y);
                //Top Right Corner
                if (r2 > 0) {
                    p.AddArc(xwr2, y, r2, r2, 270, 90);
                }
                //Right Edge
                p.AddLine(xw, yr, xw, yhr);
            }
            else {
                //top edge
                p.AddLine(x, y, width,y);
                //right edge
                if(roundBottom) {
                    p.AddLine(xw, y, xw, yhr);
                }
                else {
                    p.AddLine(xw, y, xw, yh);
                }
            }
            
            if(roundBottom) {
                //Bottom Right Corner
                if (r2 > 0) {
                    p.AddArc(xwr2, yhr2, r2, r2, 0, 90);
                }

                //Bottom Edge
                if (dropStyle) {
                    p.AddBezier(
                        new Point(xwr, yh),
                        new Point(xw2, yh10),
                        new Point(xw2, yh10),
                        new Point(xr, yh));
                }
                else {
                    p.AddLine(xwr, yh, xr, yh);
                }
                //Bottom Left Corner
                if (r2 > 0) {
                    p.AddArc(x, yhr2, r2, r2, 90, 90);
                }
                //Left Edge
                p.AddLine(x, yhr, x, yr);
            } else {
                //bottom edge
                p.AddLine(xw, yh, x, yh);
                //left edge
                if(roundTop) {
                    p.AddLine(x, yh, x, yr);
                } else {
                    p.AddLine(x, yh, y, y);
                }
            }            
            p.CloseFigure();
            return p;
        }
        public static GraphicsPath DrawFilledRoundedRectangle(Graphics graphics, Brush rectBrush, Rectangle rect, int radius,bool roundTop,bool roundBottom) {
            GraphicsPath path = DrawRoundedRectanglePath(rect, radius,roundTop,roundBottom);
            SmoothingMode mode = graphics.SmoothingMode;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.FillPath(rectBrush, path);
            graphics.SmoothingMode = mode;
            return path;
        }
        public static GraphicsPath DrawRoundedRectangle(Graphics graphics, Pen pen, Rectangle rect,int radius, bool roundTop, bool roundBottom) {
            GraphicsPath path = DrawRoundedRectanglePath(rect, radius,roundTop,roundBottom);
            SmoothingMode mode = graphics.SmoothingMode;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.DrawPath(pen, path);
            graphics.SmoothingMode = mode;
            return path;
        }
    }
}
