using Avalonia.Media;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace iosKeyboardTest.iOS {
    public static class KeyboardPalette {

        #region Properties
        static bool IsDark { get; set; }

        #region Common
        public static string Transparent = "#00000000";

        const byte DEF_BG_ALPHA = 150;
        const byte DEF_SP_BG_ALPHA = 150;
        const byte DEF_PU_BG_ALPHA = 255;
        const byte DEF_CC_BG_ALPHA = 200;
        const byte DEF_FG_ALPHA = 255;
        
        public static byte BG_ALPHA = DEF_BG_ALPHA;
        static byte PU_BG_ALPHA = DEF_PU_BG_ALPHA;
        static byte CC_BG_ALPHA = DEF_CC_BG_ALPHA;
        static byte FG_ALPHA = DEF_FG_ALPHA;
        static byte SP_BG_ALPHA = DEF_SP_BG_ALPHA;

        static string BG_A => BG_ALPHA.ToString("X2");
        static string PU_BG_A => PU_BG_ALPHA.ToString("X2");
        static string SP_BG_A => SP_BG_ALPHA.ToString("X2");
        static string FG_A => FG_ALPHA.ToString("X2");
        static string CC_BG_A => CC_BG_ALPHA.ToString("X2");
        #endregion

        #region Light
        static string BgHex_light => $"#{BG_A}FFFFFF";
        static string FgHex_light => $"#{FG_A}000000";
        static string FgHex2_light => $"#{FG_A}696969";
        static string DefaultKeyBgHex_light => $"#{BG_A}C0C0C0";
        static string HoldBgHex_light => $"#{PU_BG_A}FAFAD2";
        static string HoldFocusBgHex_light => $"#{PU_BG_A}F0E68C";
        static string HoldFgHex_light => $"#{FG_A}000000";
        static string PressedBgHex_light => $"#{BG_A}DCDCDC";
        static string SpecialKeyBgHex_light => $"#{SP_BG_A}C7FFE3";
        static string SpecialKeyPressedBgHex_light => $"#{SP_BG_A}38FF9C";
        static string PrimarySpecialKeyBgHex_light => $"#{SP_BG_A}87CEFA";
        static string PrimarySpecialKeyPressedBgHex_light => $"#{SP_BG_A}93CAEC";
        static string ShiftBgHex_light => $"#{SP_BG_A}6495ED";
        static string ShiftFgHex_light => $"#{FG_A}6495ED";
        static string MenuBgHex_light => $"#{BG_A}CCCCCC";
        static string CursorControlBgHex_light => $"#{BG_A}141414";
        static string CursorControlFgHex_light => $"#{FG_A}FFFFFF";
        #endregion

        #region Dark
        static string BgHex_dark => $"#{BG_A}000000";
        static string DefaultKeyBgHex_dark => $"#{BG_A}C0C0C0";
        static string FgHex_dark => $"#{FG_A}FFFFFF";
        static string FgHex2_dark => $"#{FG_A}DCDCDC";
        static string HoldBgHex_dark => $"#{PU_BG_A}FFD700";
        static string HoldFocusBgHex_dark => $"#{PU_BG_A}FFA500";
        static string HoldFgHex_dark => $"#{FG_A}000000";
        static string PressedBgHex_dark => $"#{BG_A}808080";
        static string SpecialKeyBgHex_dark => $"#{SP_BG_A}696969";
        static string SpecialKeyPressedBgHex_dark => $"#{SP_BG_A}B3B3B3";
        static string PrimarySpecialKeyBgHex_dark => $"#{SP_BG_A}0000CD";
        static string PrimarySpecialKeyPressedBgHex_dark => $"#{SP_BG_A}5C5CFF";
        static string ShiftBgHex_dark => $"#{SP_BG_A}00FFFF";
        static string ShiftFgHex_dark => $"#{FG_A}00FFFF";
        static string MenuBgHex_dark => $"#{BG_A}C0C0C0";
        static string CursorControlBgHex_dark => $"#{BG_A}141414";
        static string CursorControlFgHex_dark => $"#{FG_A}FFFFFF";
        #endregion

        #region Working Set
        public static string BgHex => IsDark ? BgHex_dark : BgHex_light;
        public static string DefaultKeyBgHex => IsDark ? DefaultKeyBgHex_dark : DefaultKeyBgHex_light;
        public static string FgHex => IsDark ? FgHex_dark:FgHex_light;        
        public static string FgHex2 => IsDark ? FgHex2_dark:FgHex2_light;        
        public static string HoldBgHex => IsDark ? HoldBgHex_dark:HoldBgHex_light;        
        public static string HoldFocusBgHex => IsDark ? HoldFocusBgHex_dark:HoldFocusBgHex_light;        
        public static string HoldFgHex => IsDark ? HoldFgHex_dark:HoldFgHex_light;        
        public static string PressedBgHex => IsDark ? PressedBgHex_dark:PressedBgHex_light;        
        public static string SpecialKeyBgHex => IsDark ? SpecialKeyBgHex_dark:SpecialKeyBgHex_light;        
        public static string SpecialKeyPressedBgHex => IsDark ? SpecialKeyPressedBgHex_dark: SpecialKeyPressedBgHex_light;        
        public static string PrimarySpecialKeyBgHex => IsDark ? PrimarySpecialKeyBgHex_dark:PrimarySpecialKeyBgHex_light;        
        public static string PrimarySpecialKeyPressedBgHex => IsDark ? PrimarySpecialKeyPressedBgHex_dark : PrimarySpecialKeyPressedBgHex_light;        
        public static string ShiftBgHex => IsDark ? ShiftBgHex_dark:ShiftBgHex_light;       
        public static string ShiftFgHex => IsDark ? ShiftFgHex_dark:ShiftFgHex_light;       
        public static string MenuBgHex => IsDark ? MenuBgHex_dark:MenuBgHex_light;        
        public static string CursorControlBgHex => IsDark ? CursorControlBgHex_dark:CursorControlBgHex_light;        
        public static string CursorControlFgHex => IsDark ? CursorControlFgHex_dark:CursorControlFgHex_light;  
        #endregion
        #endregion

        public static void SetTheme(
            bool isDark, 
            byte? bga = default, 
            byte? fga = default, 
            byte? spa = default, 
            byte? pua = default, 
            byte? cca = default) {
            IsDark = isDark;

            if(bga is byte bga_val) {
                BG_ALPHA = bga_val;
            } else {
                BG_ALPHA = DEF_BG_ALPHA;
            }
            if(fga is byte fga_val) {
                FG_ALPHA = fga_val;
            } else {
                FG_ALPHA = DEF_FG_ALPHA;
            }
            if(cca is byte cca_val) {
                CC_BG_ALPHA = cca_val;
            } else {
                CC_BG_ALPHA = DEF_CC_BG_ALPHA;
            }
            if(spa is byte spa_val) {
                SP_BG_ALPHA = spa_val;
            } else {
                SP_BG_ALPHA = DEF_SP_BG_ALPHA;
            }
        }
        public static void PrintPalette() {
            var sb = new StringBuilder();
            foreach(var prop in typeof(KeyboardPalette).GetProperties()) {
                if(prop.GetValue(null) is not SolidColorBrush scb) {
                    continue;
                }
                uint intVal = ((uint)scb.Color.A << 24) | ((uint)scb.Color.R << 16) | ((uint)scb.Color.G << 8) | (uint)scb.Color.B;
                string hex = $"#{intVal.ToString("x8", CultureInfo.InvariantCulture).ToUpper()}";
                sb.AppendLine($"public static string {prop.Name.Replace("Brush", "Hex")} {{ get; }} = \"{hex}\";");
            }
            Debug.Write(sb.ToString());
        }
    }
}
