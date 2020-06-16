using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
//using Gdk;

namespace MpWinFormsApp {
    public class MpCompatibility {
        public static bool IsRunningOnMono() {
            return Type.GetType("Mono.Runtime") != null;
        }
        /*public static Gdk.ModifierType GetGdkModifierFromWin(ModifierKeys modifier) {
            switch(modifier) {
                case ModifierKeys.None:
                    return ModifierType.None;
                case ModifierKeys.Alt:
                    return ModifierType.Mod1Mask;
                case ModifierKeys.Control:
                    return ModifierType.ControlMask;
                case ModifierKeys.Shift:
                    return ModifierType.ShiftMask;
                default:
                    Console.WriteLine("Error converting modifierkeys to gdk for key: " + (uint)modifier);
                    break;
            }
            return (ModifierType)0;
        }*/
    }
}
