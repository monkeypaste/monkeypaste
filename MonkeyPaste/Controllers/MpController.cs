using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public interface MpIController {
        void Link(List<MpIView> viewList = null,List<object> modelList = null);
        void Update();
        object Find(string name);
    }   

    public abstract class MpController :  MpIController {
        public static Dictionary<string,Dictionary<string,object>> ControllerDictionary { get; set; } = new Dictionary<string,Dictionary<string,object>>();
        public static int ControllerCount { get; set; } = 0;
        public static int ViewCount { get; set; } = 0;

        public string ControllerType { get; set; } = string.Empty;
        public string ControllerName { get; set; } = string.Empty;
        public int ControllerId { get; set; }

        public MpController Parent { get; set; }

        public MpController(MpController p,int cid = -1) {
            ++ControllerCount;
            Parent = p;
            ControllerType = GetType().ToString();
            ControllerId = cid;
            ControllerName = ControllerType + (ControllerId == -1 ? ControllerCount : ControllerId);
        }

        public abstract void Update();

        public void Link(List<MpIView> viewList = null,List<object> modelList = null) {
            //return;
            //add controller type as sub-dicitonary
            if(!ControllerDictionary.ContainsKey(ControllerType)) {
                ControllerDictionary.Add(ControllerType,new Dictionary<string,object>());                
            }
            //add this controller to its type dictionary
            ControllerDictionary[ControllerType].Add(ControllerName,this);

            //add views
            foreach(MpIView vo in viewList) {
                if(!ControllerDictionary[ControllerType].ContainsKey(vo.ViewName)) {
                    ControllerDictionary[ControllerType].Add(vo.ViewName,vo.ViewData);
                }
                else {
                    Console.WriteLine("Warning overriding view: " + vo.ViewName);
                    ControllerDictionary[ControllerType][vo.ViewName] = vo;
                }
                if(vo.ViewData.GetType().IsSubclassOf(typeof(ContainerControl)) || vo.ViewData.GetType().IsSubclassOf(typeof(Control))) {
                    ((Control)vo.ViewData).KeyPress += View_KeyPress;
                }
                ((Control)vo.ViewData).Click += View_Click;
            }
        }
        
        public object Find(string name) {
            List<object> cl = new List<object>();
            // loop controller types
            foreach(KeyValuePair<string,Dictionary<string,object>> ctkvp in ControllerDictionary) {
                if(ctkvp.Key.ToLower().Contains(name.ToLower())) {
                    foreach(KeyValuePair<string,object> ckvp in ControllerDictionary[ctkvp.Key]) {
                        if(ckvp.Key.ToLower().Contains(name.ToLower())) {
                            cl.Add(ckvp.Value);
                        }
                    }
                }
                //loop controller sub components
                foreach(KeyValuePair<string,object> ckvp in ControllerDictionary[ctkvp.Key]) {
                    if(ckvp.Key.ToLower().Contains(name.ToLower())) {
                        cl.Add(ckvp.Value);
                    }
                }
            }
            if(cl.Count > 0) {
                //if exact match return that object
                foreach(object c in cl) {
                    if(c.GetType().ToString() == name) {
                        return c;
                    }
                }
                if(cl.Count == 1) {
                    return cl[0];
                }
                return cl;
            }
            return null;
        }

        protected virtual void View_KeyPress(object sender,KeyPressEventArgs e) {
            if(((MpLogMenuSearchBox)Find("MpLogMenuSearchBox")).Focused) {
                return;
            }
            ((MpLogMenuSearchBox)Find("MpLogMenuSearchBox")).AppendText(e.KeyChar.ToString());
            ((MpLogMenuSearchBox)Find("MpLogMenuSearchBox")).Focus();
        }

        protected void View_Click(object sender,EventArgs e) {
            Console.WriteLine("MpController view clicked w/ sender: " + sender.ToString());
        }
    }
}
