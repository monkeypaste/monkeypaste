using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpController {
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
                    v.Value.KeyPress += Vc_KeyPress;
                }
            }
        }

        protected virtual void Vc_KeyPress(object sender,KeyPressEventArgs e) {
            Console.WriteLine("MpController keypress: " + e.ToString());
        }
    }
}
