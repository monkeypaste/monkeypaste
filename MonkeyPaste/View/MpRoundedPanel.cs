using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace MonkeyPaste {
    public class MpRoundedPanel : Panel {
        public void DrawRoundRect(Graphics g,float X,float Y,float width,float height,float radius) {
            GraphicsPath gp = new GraphicsPath();
            gp.AddLine(X + radius,Y,X + width - (radius * 2),Y);
            gp.AddArc(X + width - (radius * 2),Y,radius * 2,radius * 2,270,90);
            gp.AddLine(X + width,Y + radius,X + width,Y + height - (radius * 2));
            gp.AddArc(X + width - (radius * 2),Y + height - (radius * 2),radius * 2,radius * 2,0,90);
            gp.AddLine(X + width - (radius * 2),Y + height,X + radius,Y + height);
            gp.AddArc(X,Y + height - (radius * 2),radius * 2,radius * 2,90,90);
            gp.AddLine(X,Y + height - (radius * 2),X,Y + radius);
            gp.AddArc(X,Y,radius * 2,radius * 2,180,90);
            gp.CloseFigure();
            g.FillPath(_topBrush,gp);
            gp.Dispose();
        }
        public Color BackColor2 { get; set; }

        private Color _backColor = Color.Pink;
        public override Color BackColor {
            get {
                return _backColor;
            }
            set {
                if(MpHelperSingleton.Instance.IsBright(value)) {
                    BackColor2 = MpHelperSingleton.Instance.ChangeColorBrightness(value,-0.5f);
                    _backColor = value;
                    _topBrush = new SolidBrush(_backColor);
                    _bottomBrush = new SolidBrush(BackColor2);
                } else {
                    BackColor2 = value;
                    _backColor = MpHelperSingleton.Instance.ChangeColorBrightness(value,0.5f);
                    _topBrush = new SolidBrush(BackColor2);
                    _bottomBrush = new SolidBrush(_backColor);
                }
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
                _pen = new Pen(_borderColor,(float)BorderThickness);
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
                _pen = new Pen(_borderColor,BorderThickness);
                Invalidate();
            }
        }

        private int _radius =50;
        public int Radius {
            get {
                return _radius;
            }
            set {
                _radius = value;
                Invalidate();
            }
        }

        private SolidBrush _topBrush, _bottomBrush;
        private Pen _pen;

        public MpRoundedPanel() : base() {
            _pen = new Pen(BorderColor,BorderThickness);
        }
        private Rectangle GetLeftUpper(int e) {
            return new Rectangle(0,0,e,e);
        }
        private Rectangle GetRightUpper(int e) {
            return new Rectangle(Width - e,0,e,e);
        }
        private Rectangle GetRightLower(int e) {
            return new Rectangle(Width - e,Height - e,e,e);
        }
        private Rectangle GetLeftLower(int e) {
            return new Rectangle(0,Height - e,e,e);
        }
        private void ExtendedDraw(PaintEventArgs e) {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            GraphicsPath path = new GraphicsPath();
            path.StartFigure();
            path.AddArc(GetLeftUpper(Radius),180,90);
            path.AddLine(Radius,0,Width - Radius,0);
            path.AddArc(GetRightUpper(Radius),270,90);
            path.AddLine(Width,Radius,Width,Height - Radius);
            path.AddArc(GetRightLower(Radius),0,90);
            path.AddLine(Width - Radius,Height,Radius,Height);
            path.AddArc(GetLeftLower(Radius),90,90);
            path.AddLine(0,Height - Radius,0,Radius);
            path.CloseFigure();
            //e.Graphics.FillPath(_topBrush,path);
            Region = new Region(path);
            //half height
           // int hh = (int)((float)Height / 2.0f);
            //GraphicsPath topPath = new GraphicsPath();

            //topPath.StartFigure();
            ////top left
            //topPath.AddArc(GetLeftUpper(Radius),180,90);
            ////top
            //topPath.AddLine(Radius,0,Width - Radius,0);
            ////top right
            //topPath.AddArc(GetRightUpper(Radius),270,90);
            ////mid right
            //topPath.AddLine(Width,Radius,Width,hh - Radius);
            ////mid
            //topPath.AddLine(Width,hh,0,hh);
            ////mid left
            //topPath.AddLine(0,hh - Radius,0,Radius);
            //topPath.CloseFigure();
            //Region = new Region(topPath);
            
            //GraphicsPath bottomPath = new GraphicsPath();
            ////bottom right
            //bottomPath.AddArc(GetRightLower(Radius),0,90);
            ////bottom
            //bottomPath.AddLine(Width - Radius,Height,Radius,Height);
            ////bottom left
            //bottomPath.AddArc(GetLeftLower(Radius),90,90);
            ////mid left
            //bottomPath.AddLine(0,Height - Radius,0,Radius);
            ////mid
            //bottomPath.AddLine(0,hh,Width,hh);
            ////mid right
            //bottomPath.AddLine(Width,hh,Width,Height - Radius);
            //bottomPath.CloseFigure();

            //e.Graphics.SetClip(topPath);
            //e.Graphics.FillPath()
            //e.Graphics.FillPath(_bottomBrush,bottomPath);
            
        }
        private void DrawSingleBorder(Graphics graphics) {
            graphics.DrawArc(_pen,new Rectangle(0,0,Radius,Radius),180,90);
            graphics.DrawArc(_pen,new Rectangle(Width - Radius - 1,-1,Radius,Radius),270,90);
            graphics.DrawArc(_pen,new Rectangle(Width - Radius - 1,Height - Radius - 1,Radius,Radius),0,90);
            graphics.DrawArc(_pen,new Rectangle(0,Height - Radius - 1,Radius,Radius),90,90);
            graphics.DrawRectangle(_pen,0.0f,0.0f,(float)Width - 1.0f,(float)Height - 1.0f);
        }
        private void DrawBorder(Graphics graphics) {
            DrawSingleBorder(graphics);
        }
        protected override void OnPaint(PaintEventArgs e) {
            //Graphics v = e.Graphics;
            //DrawRoundRect(v,e.ClipRectangle.Left,e.ClipRectangle.Top,e.ClipRectangle.Width - 1,e.ClipRectangle.Height - 1,Radius);
            //Without rounded corners
            //e.Graphics.DrawRectangle(Pens.Blue,e.ClipRectangle.Left,e.ClipRectangle.Top,e.ClipRectangle.Width - 1,e.ClipRectangle.Height - 1);
            //base.OnPaint(e);
            base.OnPaint(e);
            //ExtendedDraw(e);
            //if(BorderThickness > 0) {
            //    DrawBorder(e.Graphics);
            //}
        }
    }
}