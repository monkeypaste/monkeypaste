using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpApplicationController : MpController {
        private MpTaskbarIconController _taskbarController;
        private object _context;

        public MpApplicationController(object appContext,MpController Parent) : base(Parent) {
            _context = appContext;
            _taskbarController = new MpTaskbarIconController(_context,this);            
        }
        public override void Update() {
            //no view do nothing
        }
    }
}
