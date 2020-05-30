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
        public virtual void Update() {}
        public virtual void DefineEvents() {}
        public virtual Rectangle GetBounds() { return Rectangle.Empty; }
    }
}
