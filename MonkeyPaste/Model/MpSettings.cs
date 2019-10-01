using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpSettings {
        private Dictionary<string,object> _settingDictionary = new Dictionary<string,object>();

        public MpSettings() {
            SetSetting("LogPanelDefaultVisibleTileCount",5);
            SetSetting("LogPanelDefaultHeightRatio",0.3f);
            SetSetting("LogPanelDefaultTilePadRatio",0.02f);
            SetSetting("LogPanelBgColor",MpColorPallete.LightBlue);
            SetSetting("LogPanelTileFontFace","Calibri");
            SetSetting("LogPanelTileFontSize",10.0f);
            SetSetting("LogPanelTileTitleRatio",0.2f);
            SetSetting("LogPanelTileTitleFontFace","Calibri");
            SetSetting("LogPanelTileTitleFontSize",18.0f);
            SetSetting("LogPanelTileActiveColor",MpColorPallete.Red);
            SetSetting("LogPanelTileColor1",MpColorPallete.DarkGreen);
            SetSetting("LogPanelTileColor2",MpColorPallete.LightGreen);
            SetSetting("LogPanelTileTitleTextBoxBgColor",MpColorPallete.Yellow);
            SetSetting("LogPanelTileTitleIconBorderColor",MpColorPallete.Blue);
        }

        public void SetSetting(string key,object val) {
            if(_settingDictionary.Keys.Contains(key)) {
                _settingDictionary.Remove(key);
            }
            _settingDictionary.Add(key,val);
            Console.WriteLine("Set setting key: " + key + " value: " + val.ToString());
        }

        public object GetSetting(string key) {
            if(_settingDictionary.ContainsKey(key)) {
                object val;
                _settingDictionary.TryGetValue(key,out val);
                return val;
            }
            Console.WriteLine("Settings error, no value for key: " + key);
            return null;
        }
    }
}
