using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {    
    
    public class MpScrollbarPanelController : MpControlController {
        public MpScrollbarGripPanelController ScrollbarGripControlController { get; set; }

        public Panel ScrollbarPanel { get; set; }

        public delegate void Scroll(object sender, ScrollEventArgs e);
        public event Scroll ScrollStartEvent;
        public event Scroll ScrollContinueEvent;
        public event Scroll ScrollEndEvent;

        private bool _isHorizontal;
        private Point _startDragPoint = Point.Empty;
        private bool _isScrolling = false;

        public MpScrollbarPanelController(MpControlController parent, Control controlToScroll,bool isHorizontal) : base(parent) {
           _isHorizontal = isHorizontal;

            ScrollbarPanel = new Panel() {
                AutoSize = false,
                AutoScroll = false,
                Visible = false,
                Bounds = GetBounds(),
                BackColor = Properties.Settings.Default.TileItemScrollBarBgColor
            };
            ScrollbarPanel.DoubleBuffered(true);

            ScrollbarGripControlController = new MpScrollbarGripPanelController(this,controlToScroll,isHorizontal);
            ScrollbarPanel.Controls.Add(ScrollbarGripControlController.GripPanel);

            ScrollbarGripControlController.GripPanel.MouseDown += (s, e) => {
                _startDragPoint = e.Location;
                _isScrolling = true;
                ScrollStartEvent(this, new ScrollEventArgs(0));
            };
            ScrollbarGripControlController.GripPanel.MouseMove += (s, e) => {
                if(_isScrolling) {
                    int lastOffset = ScrollbarGripControlController.Offset;
                    if(_isHorizontal) {
                        //float or = ScrollbarGripControlController.GetOffsetRatio();
                        int dx = (int)((float)(e.Location.X - _startDragPoint.X));
                        Cursor.Current = Cursors.Hand;
                        ScrollbarGripControlController.Offset += dx;
                        Console.WriteLine("Scroll Delta X: " + dx);
                    } else {
                        //float or = ScrollbarGripControlController.GetOffsetRatio();
                        int dy = (int)((float)(e.Location.Y - _startDragPoint.Y));
                        Cursor.Current = Cursors.Hand;
                        ScrollbarGripControlController.Offset += dy;
                        ScrollContinueEvent(this, new ScrollEventArgs(dy));
                        Console.WriteLine("Scroll Delta Y: " + dy);
                    }
                    ScrollContinueEvent(this, new ScrollEventArgs(ScrollbarGripControlController.Offset - lastOffset));
                }
            };
            ScrollbarGripControlController.GripPanel.MouseUp += (s, e) => {
                _isScrolling = false;
                Cursor.Current = Cursors.Arrow;

                ScrollEndEvent(this, new ScrollEventArgs(0));
            };
        }
        
        public override Rectangle GetBounds() {
            //scrollable  rect
            Rectangle sr = ((MpScrollPanelController)Parent).GetBounds();
            if (_isHorizontal) {
                //scroll bar size
                int sbs = (int)(sr.Height * Properties.Settings.Default.TileItemScrollBarThicknessRatio);
                return new Rectangle(sr.X, sr.Bottom - sbs, sr.Width, sbs);
            }
            else {
                //scroll bar size
                int sbs = (int)(sr.Width * Properties.Settings.Default.TileItemScrollBarThicknessRatio);
                return new Rectangle(sr.Right - sbs, sr.Y, sbs, sr.Height);
            }
        }

        public override void Update() {
            ScrollbarPanel.Bounds = GetBounds();

            ScrollbarGripControlController.Update();
            ScrollbarGripControlController.GripPanel.BringToFront();
            ScrollbarPanel.Invalidate();
        }
    }
    public class ScrollEventArgs : EventArgs {
        public int ScrollAmount { get; set; }
        public ScrollEventArgs(int amount) : base() {
            ScrollAmount = amount;
        }
    }
}
