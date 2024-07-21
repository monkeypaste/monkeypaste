using Avalonia.Media;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace iosKeyboardTest.iOS {
    public static class KeyboardPalette {

        #region Properties
        static bool IsDark { get; set; }

        #region Light
        static string BgHex_light { get; } = "#FFFFFFFF";
        static string FgHex_light { get; } = "#FF000000";
        static string FgHex2_light { get; } = "#FFA9A9A9";
        static string HoldBgHex_light { get; } = "#FFFAFAD2";
        static string HoldFocusBgHex_light { get; } = "#FFF0E68C";
        static string HoldFgHex_light { get; } = "#FF000000";
        static string PressedBgHex_light { get; } = "#FFDCDCDC";
        static string SpecialKeyBgHex_light { get; } = "#FFF5FFFA";
        static string PrimarySpecialKeyBgHex_light { get; } = "#FF87CEFA";
        static string ShiftHex_light { get; } = "#FF6495ED";
        static string MenuBgHex_light { get; } = "#FFCCCCCC";
        static string CursorControlBgHex_light { get; } = "#96000000";
        static string CursorControlFgHex_light { get; } = "#FF000000";
        static string DefaultKeyBgHex_light { get; } = "#FFC0C0C0";
        #endregion

        #region Dark
        static string BgHex_dark { get; } = "#FF000000";
        static string FgHex_dark { get; } = "#FFFFFFFF";
        static string FgHex2_dark { get; } = "#FFDCDCDC";
        static string HoldBgHex_dark { get; } = "#FFFFD700";
        static string HoldFocusBgHex_dark { get; } = "#FFFFA500";
        static string HoldFgHex_dark { get; } = "#FF000000";
        static string PressedBgHex_dark { get; } = "#FF808080";
        static string SpecialKeyBgHex_dark { get; } = "#FF696969";
        static string PrimarySpecialKeyBgHex_dark { get; } = "#FF0000CD";
        static string ShiftHex_dark { get; } = "#FF00FFFF";
        static string MenuBgHex_dark { get; } = "#FFC0C0C0";
        static string CursorControlBgHex_dark { get; } = "#96141414";
        static string CursorControlFgHex_dark { get; } = "#FFFFFFFF";
        static string DefaultKeyBgHex_dark { get; } = "#FFC0C0C0";
        #endregion

        #region Working Set
        public static string BgHex => IsDark ? BgHex_dark : BgHex_light;        
        public static string FgHex => IsDark ? FgHex_dark:FgHex_light;        
        public static string FgHex2 => IsDark ? FgHex2_dark:FgHex2_light;        
        public static string HoldBgHex => IsDark ? HoldBgHex_dark:HoldBgHex_light;        
        public static string HoldFocusBgHex => IsDark ? HoldFocusBgHex_dark:HoldFocusBgHex_light;        
        public static string HoldFgHex => IsDark ? HoldFgHex_dark:HoldFgHex_light;        
        public static string PressedBgHex => IsDark ? PressedBgHex_dark:PressedBgHex_light;        
        public static string SpecialKeyBgHex => IsDark ? SpecialKeyBgHex_dark:SpecialKeyBgHex_light;        
        public static string PrimarySpecialKeyBgHex => IsDark ? PrimarySpecialKeyBgHex_dark:PrimarySpecialKeyBgHex_light;        
        public static string ShiftHex => IsDark ? ShiftHex_dark:ShiftHex_light;       
        public static string MenuBgHex => IsDark ? MenuBgHex_dark:MenuBgHex_light;        
        public static string CursorControlBgHex => IsDark ? CursorControlBgHex_dark:CursorControlBgHex_light;        
        public static string CursorControlFgHex => IsDark ? CursorControlFgHex_dark:CursorControlFgHex_light;        
        public static string DefaultKeyBgHex => IsDark ? DefaultKeyBgHex_dark:DefaultKeyBgHex_light;
        #endregion
        #endregion

        public static void SetTheme(bool isDark) {
            IsDark = isDark;
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
