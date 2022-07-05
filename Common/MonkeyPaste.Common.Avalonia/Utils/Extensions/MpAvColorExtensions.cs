using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste.Common;
using System.Diagnostics;

namespace MonkeyPaste.Common.Avalonia {
    public static class MpAvColorExtensions {
        public static Color ToAvColor(this string hexColor) {
            if(!hexColor.IsStringHexColor()) {
                return Colors.Transparent;
            }
            return Color.Parse(hexColor);
        }

        public static Brush ToAvBrush(this string hexColor) {            
            return new SolidColorBrush(hexColor.ToAvColor());
        }

        public static string ToHex(this Brush b) {
            if(b is SolidColorBrush scb) {
                if(!scb.ToString().IsStringHexColor()) {
                    Debugger.Break();
                }
                return scb.ToString();
            }
            Debugger.Break();
            return MpSystemColors.Transparent;
        }
    }
}
