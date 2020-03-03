using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTileTitleLabel:UserControl, MpIView {
        //private Label _foreLabel;
        //public Label ForeLabel {
        //    get {
        //        return _foreLabel;
        //    }
        //    set {
        //        _foreLabel = value;
                
        //        Invalidate();
        //    }
        //}
        //private Label _backLabel;
        //public Label BackLabel {
        //    get {
        //        return _backLabel;
        //    }
        //    set {
        //        _backLabel = value;
        //        Invalidate();
        //    }
        //}
        private string _text = string.Empty;
        public override string Text {
            get {
                return _text;
            }
            set {
                _text = value;
                Invalidate();
            }
        }
        public string ViewType {get;set;}
        public string ViewName {get;set;}
        public int ViewId {get;set;}
        public object ViewData {get;set;}

        protected override void OnPaintBackground(PaintEventArgs e) {
            //do nonthing
        }
        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            e.Graphics.FillRectangle(new SolidBrush(BackColor),ClientRectangle);
            e.Graphics.DrawString(Text,Font,new SolidBrush(ForeColor),PointF.Empty);
        }
        public MpTileTitleLabel(int tileId,int panelId) : base() { //text,MpHelperSingleton.Instance.IsBright(bgColor) ? Color.Black:Color.White,Color.Transparent,18.0f,"Impact") {            
            this.DoubleBuffered = true;
            ForeColor = Color.White;
            BackColor = Color.Black;

            ViewType = this.GetType().ToString();
            ViewName = ViewType + panelId + "_" + tileId;
            ViewId = MpSingletonController.Instance.Rand.Next(1,int.MaxValue);
            ViewData = this;    
        }
    }
}
