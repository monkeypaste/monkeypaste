//using Foundation;

//namespace MonkeyPaste.Avalonia {
//    public static class MpAvAppleScripts {
//        public const string Active_window_title_script =
//@"
//global frontApp, frontAppName, windowTitle

//set windowTitle to """"
//tell application ""System Events""
//    set frontApp to first application process whose frontmost is true
//    set frontAppName to name of frontApp
//    tell process frontAppName
//        tell (1st window whose value of attribute ""AXMain"" is true)
//            set windowTitle to value of attribute ""AXTitle""
//        end tell
//    end tell
//end tell

//return {frontAppName, windowTitle}
//";

//        public static string ExecuteAppleScript(string script) {
//            NSDictionary error = null;
//            NSAppleScript scr = new NSAppleScript(script);
//            NSAppleEventDescriptor result = scr.ExecuteAndReturnError(out error);
//            string val = result.StringValue;
//            return val;
//        }
//    }
//}
