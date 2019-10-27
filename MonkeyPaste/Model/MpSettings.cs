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
            SetSetting("LogScreenHeightRatio",0.3f,MpSettingValueType.Float);
            SetSetting("LogPanelBgColor",MpColorPallete.Blue,MpSettingValueType.Color);
            SetSetting("LogFont","Calibri",MpSettingValueType.String);
            SetSetting("LogPanelTileFontSize",10.0f,MpSettingValueType.Float);
            SetSetting("LogResizeHandleHeight",5,MpSettingValueType.Int);
            SetSetting("LogPadRatio",0.001f,MpSettingValueType.Float);

            SetSetting("LogMenuHeightRatio",0.1f,MpSettingValueType.Float);
            SetSetting("LogMenuSearchFont","Calibri",MpSettingValueType.String);
            SetSetting("LogMenuSearchFontSizeRatio",0.85f,MpSettingValueType.Float);

            SetSetting("TileChooserPadHeightRatio",0.02f,MpSettingValueType.Float);
            SetSetting("TileChooserBgColor1",MpColorPallete.Blue,MpSettingValueType.Color);
            SetSetting("TileChooserBgColor2",MpColorPallete.DarkGreen,MpSettingValueType.Color);
            SetSetting("TileChooserBgColor3",MpColorPallete.LightBlue,MpSettingValueType.Color);

            SetSetting("TileTitleFontRatio",0.9f,MpSettingValueType.Float);
            SetSetting("TileTitleFont","Consolas",MpSettingValueType.String);
            SetSetting("TileTitleHeightRatio",0.2f,MpSettingValueType.Float);
            SetSetting("TileTitleFontColor",Color.Black,MpSettingValueType.Color);

            SetSetting("TileDetailFontSizeRatio",0.7f,MpSettingValueType.Float);
            SetSetting("TileDetailFont","Consolas",MpSettingValueType.String);
            SetSetting("TileDetailFontColor",Color.White,MpSettingValueType.Color);
            SetSetting("TileDetailTitlePad",15,MpSettingValueType.Int);

            SetSetting("TileMenuHeightRatio",0.1f,MpSettingValueType.Float);
            SetSetting("TileMenuFont","Consolas",MpSettingValueType.String);
            SetSetting("TileMenuFontRatio",0.6f,MpSettingValueType.Float);
            SetSetting("TileMenuColor",Color.FromArgb(50,0,0,0),MpSettingValueType.Color);

            SetSetting("TileFocusColor",MpColorPallete.Red,MpSettingValueType.Color);
            SetSetting("TileUnfocusColor",Color.White,MpSettingValueType.Color);
            SetSetting("TileBorderThickness",10,MpSettingValueType.Int);
            SetSetting("TileColor1",MpColorPallete.Yellow,MpSettingValueType.Color);
            SetSetting("TileColor2",MpColorPallete.Blue,MpSettingValueType.Color);
            SetSetting("TileIconBorderColor",MpColorPallete.DarkGreen,MpSettingValueType.Color);
            SetSetting("TileIconBorderRatio",0.03f,MpSettingValueType.Float);
            SetSetting("TileBorderRadius",10,MpSettingValueType.Int);
            SetSetting("TileFont","Consolas",MpSettingValueType.String);
            SetSetting("TileMinLineCount",10,MpSettingValueType.Int);
            SetSetting("TilePadWidthRatio",0.04f,MpSettingValueType.Float);

            SetSetting("SearchTextBoxWidthRatio",0.15f,MpSettingValueType.Float);
            SetSetting("SearchTextBoxHeightRatio",0.15f,MpSettingValueType.Float);
            SetSetting("SearchTextBoxFont","Consolas",MpSettingValueType.String);
            SetSetting("SearchTextBoxFontSizeRatio",0.85f,MpSettingValueType.Float);
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
            try {
                object val;
                SettingDictionary.TryGetValue(key,out val);
                return val;
            }
            catch(Exception ex) {
                Console.WriteLine("Settings error, no value for key: " + key+"\nActual Error: "+ex.ToString());
            }
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
