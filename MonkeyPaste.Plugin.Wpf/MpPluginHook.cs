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

        #region MpIPlugin Implementation

        public string GetManifest() {
            return "Yo Test Plugin Here";
        }

        #endregion

        #region MpIPluginComponent Implementation

        public void Create(object dobj) {
            //dobj will contain an array of data matching supplied formats
            if (dobj == null) {
                return;
            }
            if (dobj is object[] dobjParts) {
                if (dobjParts.Length > 0) {
                    _dobj = (dobjParts[0] as string) + "test";
                }
            }

        }

        #endregion

        #region MpIClipboardItemPluginComponent Implementation
        public object GetDataObject() {
            return _dobj;
        }

        public string[] GetHandledDataFormats() {
            return new string[] {
                "Text"
            };
        }
        #endregion
    }
}
