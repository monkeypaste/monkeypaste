using MonkeyPaste;
using System.IO;
using MonkeyPaste.Common.Plugin; 
using MonkeyPaste.Common; 
using MonkeyPaste.Common.Wpf;
using System.Reflection;

namespace MpWpfApp {
    public class MpWpfDbInfo : MonkeyPaste.MpIDbInfo {

        public string DbName => "mp.db";
        public string DbPath => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), DbName);
    }
}