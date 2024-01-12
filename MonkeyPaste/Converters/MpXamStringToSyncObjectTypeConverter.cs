using MonkeyPaste.Common.Plugin;
using System;

namespace MonkeyPaste {
    public class MpXamStringToSyncObjectTypeConverter : MpIStringToSyncObjectTypeConverter {
        public Type Convert(string typeString) {
            if (string.IsNullOrEmpty(typeString)) {
                throw new Exception(@"typeString is null or empty");
            }
            if (!typeString.Contains(".")) {
                typeString = "MonkeyPaste." + typeString;
            }
            if (!typeString.ToLower().StartsWith(@"mpwpfapp") &&
               !typeString.ToLower().StartsWith(@"monkeypaste")) {
                throw new Exception(@"typestring must be from a known namespace");
            }
            try {
                if (typeString.ToLower().StartsWith(@"mpwpfapp")) {
                    typeString = typeString.Replace(@"MpWpfApp", @"MonkeyPaste");
                }
                var asm = typeof(MpXamStringToSyncObjectTypeConverter).Assembly;
                return asm.GetType(typeString);
            }
            catch (Exception ex) {
                MpConsole.WriteLine(@"Unknown type: " + typeString);
                MpConsole.WriteLine(@"With exception: " + ex);
            }
            return null;
        }
    }
}
