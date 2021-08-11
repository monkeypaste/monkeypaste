using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Java.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonkeyPaste.Droid {
    public class MpShellCommand : MpIShellCommand {
        public object Run(string cmd, params object[] args) {
            Runtime.GetRuntime().Exec(cmd);
            if(args == null || args.Length == 0) {
                return null;
            }
            // process args for return here
            return null;
        }
    }
}