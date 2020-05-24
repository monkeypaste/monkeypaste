using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public abstract class MpController : MpTreeNode {
        public static Dictionary<string,object> ControllerDictionary { get; set; } = new Dictionary<string,object>();

        private MpModel _model;
        public MpModel Model {
            get {
                return _model;
            }
            set {
                if(_model != value) {
                    _model = value;
                }
            }
        }

        private Control _view;
        public Control View {
            get {
                return _view;
            }
            set {
                if (_view != value) {
                    _view = value;
                }
            }
        }

        public ObservableCollection<MpController> ChildControllerList = new ObservableCollection<MpController>();
       

        public MpController(MpController parentController) : base(parentController) {
            ControllerDictionary.Add(this.GetType().ToString()+"_"+MpSingletonController.Instance.Rand.Next(0,100000), this);
        }
        public MpController() : this(null) { }
        

        public object Find(Type type) {
            return Find(type.ToString());
        }
        public object Find(string name) {
            foreach(KeyValuePair<string, object> ctkvp in ControllerDictionary) {
                if(ctkvp.Key.ToLower().Contains(name.ToLower())) {
                    return ctkvp.Value;
                }
            }
            return null;
        }

        public virtual void Update() {

        }
        
        //public abstract void UpdateBounds(Rectangle refRectangle,float refRatio,bool xRatio,bool yRatio);
        //public abstract void UpdateContent(Rectangle refRect,float refRatio,bool xRatio,bool yRatio);

        //public virtual void Update(Rectangle r,List<float> refRatio,List<bool> xRatio,List<bool> yRatio) {
        //    if(refRatio.Count != xRatio.Count && refRatio.Count != yRatio.Count) {
        //        throw new Exception("MpController Exception: ref value lists not same sizes");
        //    }
        //    for(int i = 0;i < refRatio.Count;i++) {
        //        MpIView cv = ViewList[i];
        //        float rx = 1.0f, ry = 1.0f;
        //        int px = 0, py = 0;
        //        if(xRatio[i]) {
        //            rx = r.Width * refRatio[i];
        //            px = r.Width - (int)(rx*0.5f);
        //        } else if(yRatio[i]) {
        //            ry = r.Height * refRatio[i];
        //            py = r.Height - (int)(ry * 0.5f);
        //        }
        //        Control c = ((Control)cv);
        //        c.Bounds = new Rectangle(
        //           c.Location.X + px,
        //           c.Location.Y + py,
        //           (int)((float)c.Width * rx),
        //           (int)((float)c.Height * ry)
        //        );
        //    }
        //}





    }
}
