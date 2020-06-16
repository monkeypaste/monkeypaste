using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MpWpfApp {
    public class MpRoundedPanel : Panel { 
        
        public override Color BackColor {
            get {
                return base.BackColor;
            }
            set {
                base.BackColor = value;
                _brush = new SolidBrush(base.BackColor);
                Invalidate();
            }
        }

        private int _borderThickness = 0;
        public int BorderThickness {
            get {
                return _borderThickness;
            }
            set {
                _borderThickness = value;
                _pen = new Pen(_borderColor, (float)BorderThickness);
                Invalidate();
            }
        }

        private Color _borderColor = Color.White;
        public Color BorderColor {
            get {
                return _borderColor;
            }
            set {
                _borderColor = value;
                _pen = new Pen(_borderColor, BorderThickness);
                Invalidate();
            }
        }

        private int _radius = 50;
        public int Radius {
            get {
                return _radius;
            }
            set {
                _radius = value;
                Invalidate();
            }
        }

        private SolidBrush _brush;
        private Pen _pen;

        public MpRoundedPanel() : base() {
            _pen = new Pen(BorderColor, BorderThickness);
        }
        public Rectangle GetChildBounds() {
            Rectangle cb = ClientRectangle ;
            cb.Inflate(-(_borderThickness * 2), -(_borderThickness * 2) );
            cb.Location = new Point(_borderThickness, _borderThickness);
            return ClientRectangle;
            //return cb;
            //return new Rectangle((int)((float)_borderThickness/2.0f),(int)((float)_borderThickness/2.0f), Width - (int)((float)_borderThickness / 1.0f), Height - (int)((float)_borderThickness / 1.0f));
            //return new Rectangle(_borderThickness, _borderThickness , Width, Height);
        }
        private GraphicsPath DrawRoundedRectanglePath(Rectangle rect, int radius) {
            return DrawRoundedRectanglePath(rect, radius, false);
        }
        private GraphicsPath DrawRoundedRectanglePath(Rectangle rect, int radius, bool dropStyle) {
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
            } else {
                p.AddLine(xwr, yh, xr, yh);
            }
            //Bottom Left Corner
            if (r2 > 0) {
                p.AddArc(x, yhr2, r2, r2, 90, 90);
            }
            //Left Edge
            p.AddLine(x, yhr, x, yr);
            p.CloseFigure();
            return p;
        }
        private GraphicsPath DrawFilledRoundedRectangle(Graphics graphics, Brush rectBrush, Rectangle rect, int radius) {
            GraphicsPath path = DrawRoundedRectanglePath(rect, radius);
            graphics.SmoothingMode = SmoothingMode.HighSpeed;
            graphics.FillPath(rectBrush, path);
            this.Region = new Region(path);
            return path;
        }
        private GraphicsPath DrawRoundedRectangle(Graphics graphics, Pen pen, Rectangle rect, int radius) {
            GraphicsPath path = DrawRoundedRectanglePath(rect, radius);
            graphics.SmoothingMode = SmoothingMode.HighSpeed;
            graphics.DrawPath(pen, path);
            //this.Region = new Region(path);
            return path;
        }
        protected override void OnPaint(PaintEventArgs e) {
            Rectangle borderRect =  new Rectangle(0,0,Width,Height);
            Rectangle fillRect = borderRect;
            //fillRect.Inflate(-(_borderThickness * 1), -(_borderThickness * 1));
            DrawFilledRoundedRectangle(e.Graphics, _brush, e.ClipRectangle, _radius);
            //base.OnPaint(e);
            DrawRoundedRectangle(e.Graphics, _pen, e.ClipRectangle, _radius);
        }
    }
}