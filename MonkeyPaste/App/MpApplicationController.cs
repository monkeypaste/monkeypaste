using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpApplicationController : MpController {
        public MpTaskbarIconController TaskbarController { get; set; }
        private object _context;

        public MpApplicationController(object appContext,MpController Parent) : base(Parent) {
            _context = appContext;
            TaskbarController = new MpTaskbarIconController(_context,this);            
        }
        public override void Update() {
            //no view do nothing
        }
    }
}
