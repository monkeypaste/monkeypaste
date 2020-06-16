using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWinFormsClassLibrary { 
    public class MpSingleton {        
        private static readonly Lazy<MpSingleton> lazy = new Lazy<MpSingleton>(() => new MpSingleton());
        public static MpSingleton Instance { get { return lazy.Value; } }

        public MpScreenManager ScreenManager { get; set; } = new MpScreenManager();
    }
}
