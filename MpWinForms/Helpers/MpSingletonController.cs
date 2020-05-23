using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpSingletonController {
        //singleton innstance
        private static readonly Lazy<MpSingletonController> lazy = new Lazy<MpSingletonController>(() => new MpSingletonController());
        public static MpSingletonController Instance { get { return lazy.Value; } }
        
        //members

        

        public int ScrollWheelDelta { get; set; } = 0;
       

        public MpCopyItem AppendItem { get; set; }
        //public bool InAppendMode { get; set; } = false;

        public bool InCopySelectionMode { get; set; } = false;

        public bool InRightClickPasteMode { get; set; } = false;

        private bool _ignoreNextClipboardEvent;

        private object _appContext;

        public Random Rand { get; set; }

        public float TileTitleFontSize { get; set; }


        public MpSingletonController() {
            Rand = new Random(Convert.ToInt32(DateTime.Now.Second));
        }
        public void ScrollWheelListener(object sender, MouseEventArgs e) {
            ScrollWheelDelta += e.Delta;
        }
    }
}
