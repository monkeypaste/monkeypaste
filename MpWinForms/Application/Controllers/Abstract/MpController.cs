using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MpWinFormsApp {
    public abstract class MpController : MpTreeNode {
        private static Dictionary<string,object> _ControllerDictionary { get; set; } = new Dictionary<string,object>();

        public MpController(MpController parentController) : base(parentController) {
            int uid = 0;
            while(_ControllerDictionary.ContainsKey(this.GetType().ToString()+"_"+uid)) {
                uid++;
            }
            _ControllerDictionary.Add(this.GetType().ToString()+"_"+uid, this);
        }
        public MpController() : this(null) {}        

        public static object Find(Type type) {
            return Find(type.ToString());
        }
        public static object Find(string name) {
            foreach(KeyValuePair<string, object> ctkvp in _ControllerDictionary) {
                if(ctkvp.Key.ToLower().Contains(name.ToLower())) {
                    return ctkvp.Value;
                }
            }
            return null;
        }
        public virtual void ActivateHotKeys() { }
        public virtual void DeactivateHotKeys() { }
        public virtual void Update() {}
        public virtual void DefineEvents() {}
        public virtual Rectangle GetBounds() { return Rectangle.Empty; }
    }
}
