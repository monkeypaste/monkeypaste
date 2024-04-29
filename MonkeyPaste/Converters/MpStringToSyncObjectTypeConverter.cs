using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;

namespace MonkeyPaste {
    public class MpStringToSyncObjectTypeConverter : MpIStringToSyncObjectTypeConverter {
        public Type Convert(string typeString) {
            if (string.IsNullOrEmpty(typeString)) {
                throw new Exception(@"typeString is null or empty");
            }
            if (!typeString.Contains(".")) {
                typeString = $"{typeof(MpStringToSyncObjectTypeConverter).Assembly.GetName().Name}.{typeString}";
            }
            try {
                var asm = typeof(MpStringToSyncObjectTypeConverter).Assembly;
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
