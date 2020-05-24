using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpScrollbarGripPanelController : MpColorButtonPanelController {
        public Panel GripPanel { get; set; }
        
        private bool _isHorizontal = false;
        private Control _itemControl;
        private int _offset = 0;
        public int Offset {
            get {
                return _offset;
            }
            set {
                if(_offset != value) {
                    //int diff = value-_offset;
                    ////delta offset (normalized for item size)
                    //int doffset = (_isHorizontal) ?
                    //    (int)(((float)_itemControl.Width / (float)GetGripLength()) * (float)diff) :
                    //    (int)(((float)_itemControl.Height / (float)GetGripLength()) * (float)diff);
                    _offset = value;
                    if (_offset < 0) {
                        _offset = 0;
                    }
                    else {
                        //scrollbar panel rect
                        Rectangle sbpr = ((MpScrollbarPanelController)Parent).GetBounds();
                        //force offset to stay within scrollpanel
                        int tailPos = _offset + GetGripLength();
                        if (_isHorizontal) {
                            if (tailPos > sbpr.Width) {
                                _offset = sbpr.Width - GetGripLength();
                            }
                        }
                        else {
                            if (tailPos > sbpr.Height) {
                                _offset = sbpr.Height - GetGripLength();
                            }
                        }
                    }
                    ((MpTileContentPanelController)((MpScrollPanelController)((MpScrollbarPanelController)Parent).Parent).Parent).Update();
                }
            }
        }
        public MpScrollbarGripPanelController(MpScrollbarPanelController parentScrollbarController,Control controlToScroll,bool isHorizontal) : base(parentScrollbarController) {
            _isHorizontal = isHorizontal;
            _itemControl = controlToScroll;
            GripPanel = new Panel() {
                AutoScroll = false,
                AutoSize = false,
                Bounds = GetBounds(),
                Visible = true,
                BackColor = Properties.Settings.Default.TileItemScrollBarGripInactiveColor
            };
            GripPanel.DoubleBuffered(true);
            GripPanel.MouseHover += (s, e) => {
                GripPanel.BackColor = Properties.Settings.Default.TileItemScrollBarGripActiveColor;
                Cursor.Current = Cursors.Hand;
                Update();
            };
            GripPanel.MouseLeave += (s, e) => {
                GripPanel.BackColor = Properties.Settings.Default.TileItemScrollBarGripInactiveColor;
                Cursor.Current = Cursors.Arrow;
                Update();
            };
        }
        public int GetGripLength() {
            //scrollbar panel rect
            Rectangle sbpr = ((MpScrollbarPanelController)Parent).GetBounds();
            if (_isHorizontal) {
                //scrollbar ratio
                float sbr = (float)sbpr.Width / (float)_itemControl.Width;

                //scrollbar grip height
                return (int)((float)sbpr.Width * sbr);
            } else {
                //scrollbar ratio
                float sbr = (float)sbpr.Height / (float)_itemControl.Height;

                //scrollbar grip height
                return (int)((float)sbpr.Height * sbr);
            }
        }
        
        public override Rectangle GetBounds() {
            //scrollbar panel rect
            Rectangle sbpr = ((MpScrollbarPanelController)Parent).GetBounds();
            if (_isHorizontal) {
                return new Rectangle(Offset, 0, GetGripLength(), sbpr.Height);
            } else {                
                return new Rectangle(0, Offset, sbpr.Width, GetGripLength());
            }
        }

        public override void Update() {
            GripPanel.Bounds = GetBounds();

            GripPanel.Invalidate();
        }
    }
}
