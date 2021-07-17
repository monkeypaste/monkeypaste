using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpWpfStringToDbObjectTypeConverter : MonkeyPaste.MpIStringToSyncObjectTypeConverter {
        public Type Convert(string typeString) {
            if(string.IsNullOrEmpty(typeString)) {
                throw new Exception(@"typeString is null or empty");
            }
            if(!typeString.Contains(".")) {
                typeString = "MpWpfApp." + typeString;
            }
            if(!typeString.ToLower().StartsWith(@"mpwpfapp") &&
               !typeString.ToLower().StartsWith(@"monkeypaste")) {
                throw new Exception(@"typestring must be from a known namespace");
            }
            try {
                if (typeString.ToLower().StartsWith(@"monkeypaste")) {
                    typeString = typeString.Replace(@"MonkeyPaste", @"MpWpfApp");
                } 
                var asm = typeof(MpDbObject).Assembly;
                return asm.GetType(typeString);
            }
            catch(Exception ex) {
                MonkeyPaste.MpConsole.WriteLine(@"Unknown type: " + typeString);
                MonkeyPaste.MpConsole.WriteLine(@"With exception: " + ex);
            }
            return null;
        }
    }
}
