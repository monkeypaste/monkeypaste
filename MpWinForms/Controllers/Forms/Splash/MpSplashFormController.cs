using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpSplashFormController : MpController {
        public Form SplashForm { get; set; }

        public MpSplashFormController(MpController p) : base(p) {
            SplashForm = new Form() {
                AutoSize = false,
                AutoScaleMode = AutoScaleMode.Dpi
            };

        }
        public override void Update() {
            throw new NotImplementedException();
        }
    }
}
