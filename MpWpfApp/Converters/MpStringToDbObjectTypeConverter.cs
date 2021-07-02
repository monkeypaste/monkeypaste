using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpStringToDbObjectTypeConverter : MonkeyPaste.MpIDbStringToDbObjectTypeConverter {
        public Type Convert(string typeString) {
            if(string.IsNullOrEmpty(typeString)) {
                throw new Exception(@"typeString is null or empty");
            }
            if(!typeString.Contains(".")) {
                throw new Exception(@"typeString must be namespace qualified");
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
                MpConsole.Instance.WriteLine(@"Unknown type: " + typeString);
                MpConsole.Instance.WriteLine(@"With exception: " + ex);
            }
            return null;
        }
    }
}
