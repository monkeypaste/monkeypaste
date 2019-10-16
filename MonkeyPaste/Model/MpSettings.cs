using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public enum MpSettingValueType {
        None,
        Int,
        Float,
        String,
        Color
    }

    public class MpSettings {
        private Dictionary<string,object> _settingDictionary { get; set; }
        public Dictionary<string,object> SettingDictionary { get { return _settingDictionary; } set { _settingDictionary = value; } }

        private Dictionary<string,MpSettingValueType> _settingValueTypeDictionary { get; set; }
        public Dictionary<string,MpSettingValueType> SettingValueTypeDictionary { get { return _settingValueTypeDictionary; } set { _settingValueTypeDictionary = value; } }

        public MpSettings() {
            SettingDictionary = new Dictionary<string,object>();
            SettingValueTypeDictionary = new Dictionary<string,MpSettingValueType>();

            Reset();
        }
        public void Reset() {
            SetSetting("MaxDbPasswordAttempts",3,MpSettingValueType.Int);
            //SetSetting("LogPanelDefaultVisibleTileCount",5,MpSettingValueType.Int);
            SetSetting("LogScreenHeightRatio",0.3f,MpSettingValueType.Float);
            SetSetting("LogPanelBgColor",MpColorPallete.LightBlue,MpSettingValueType.Color);
            SetSetting("LogFont","Calibri",MpSettingValueType.String);
            SetSetting("LogPanelTileFontSize",10.0f,MpSettingValueType.Float);

            SetSetting("TileMenuHeightRatio",0.2f,MpSettingValueType.Float);
            SetSetting("TileMenuFont","Impact",MpSettingValueType.String);
            SetSetting("TileMenuFontRatio",0.75f,MpSettingValueType.Float);
            SetSetting("TileMenuColor",MpColorPallete.LightGreen,MpSettingValueType.Color);

            SetSetting("TileActiveColor",MpColorPallete.Red,MpSettingValueType.Color);
            SetSetting("TileColor1",MpColorPallete.DarkGreen,MpSettingValueType.Color);
            SetSetting("TileColor2",MpColorPallete.Yellow,MpSettingValueType.Color);
            SetSetting("TileIconBorderColor",MpColorPallete.Blue,MpSettingValueType.Color);
            SetSetting("TileBorderRadius",10,MpSettingValueType.Int);
            SetSetting("TileFont","OpenSans",MpSettingValueType.String);
            SetSetting("TileMinLineCount",10,MpSettingValueType.Int);
            SetSetting("TileOuterPadScreenWidthRatio",0.02f,MpSettingValueType.Float);
            SetSetting("TileInnerPadTileWidthRatio",0.04f,MpSettingValueType.Float);
        }
        public void SetSetting(string key,object val,MpSettingValueType svt) {
            if(SettingDictionary.Keys.Contains(key)) {
                SettingDictionary.Remove(key);
            }
            SettingDictionary.Add(key,val);
            if(SettingValueTypeDictionary.Keys.Contains(key)) {
                SettingValueTypeDictionary.Remove(key);
            }
            SettingValueTypeDictionary.Add(key,svt);

            Console.WriteLine("Set setting key: " + key + " value: " + val.ToString());
        }

        public object GetSetting(string key) {
            if(SettingDictionary.ContainsKey(key)) {
                object val;
                SettingDictionary.TryGetValue(key,out val);
                return val;
            }
            Console.WriteLine("Settings error, no value for key: " + key);
            return null;
        }
        public MpSettingValueType GetSettingValueType(string key) {
            if(SettingValueTypeDictionary.ContainsKey(key)) {
                MpSettingValueType val;
                SettingValueTypeDictionary.TryGetValue(key,out val);
                return val;
            }
            Console.WriteLine("Settings error, no value for key: " + key);
            return MpSettingValueType.None;
        }
    }
}
