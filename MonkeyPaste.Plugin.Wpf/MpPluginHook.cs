using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MonkeyPaste.Plugin;

namespace MonkeyPaste.Plugin.Wpf_Template {
    public class MpPluginHook : MpIClipboardItemPluginComponent, MpIPlugin {
        private object _dobj;

        public MpPluginHook() { }

        public void Create(object dobj) {
            //dobj will contain an array of data matching supplied formats
            if(dobj == null) {
                return;
            }
            if(dobj is object[] dobjParts) {
                if(dobjParts.Length > 0) {
                    _dobj = (dobjParts[0] as string) + "test";
                }
            }

        }

        public MpIPluginComponent[] GetComponents() {
            return new MpIPluginComponent[] { this };
        }

        public object GetDataObject() {
            return _dobj;
        }

        public string[] GetHandledDataFormats() {
            return new string[] {
                "Text"
            };
        }

        public string GetName() {
            return "Yo Test Plugin Here";
        }
    }
}
