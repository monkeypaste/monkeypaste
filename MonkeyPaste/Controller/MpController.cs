using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public abstract class MpController {
        //margin ratio
        private float _mr = 0.00f;
        public float Mr {
            get {
                return _mr;
            }
            set {
                _mr = value;
                UpdateBounds();
            }
        }
        //pad ratio
        private float _pr = 0.05f;
        public float Pr {
            get {
                return _pr;
            }
            set {
                _pr = value;
                UpdateBounds();
            }
        }

        private Dictionary<string,MpView> _view { get; set; }
        public Dictionary<string,MpView> View { get { return _view; } set { _view = value; } }

        private Dictionary<string,MpModel> _model { get; set; }
        public Dictionary<string,MpModel> Model { get { return _model; } set { _model = value; } } 

        private MpController _parentController { get; set; }
        public MpController ParentController { get { return _parentController; } set { _parentController = value; } }
                
        public MpController(MpController parentController,object view = null,object model = null) {
            ParentController = parentController;
            View = view == null ? new Dictionary<string,MpView>() : (Dictionary<string,MpView>)view;
            Model = model == null ? new Dictionary<string,MpModel>() : (Dictionary<string,MpModel>)model;
            if(View != null && View.GetType() == typeof(Dictionary<string,MpView>)) {
                foreach(KeyValuePair<string,MpView> v in ((Dictionary<string,MpView>)View)) {
                    v.Value.KeyPress += View_KeyPress;
                    v.Value.Click += View_Click;
                }
            }
        }

        //uses ParentController and children to define rect
        public abstract void UpdateBounds();

        protected virtual void View_KeyPress(object sender,KeyPressEventArgs e) {
            MpSingletonController.Instance.GetMpData().SearchStringList.Add(e.KeyChar.ToString());
        }

        private void View_Click(object sender,EventArgs e) {
            Console.WriteLine("MpController view clicked w/ sender: " + sender.ToString());
        }
    }
}
