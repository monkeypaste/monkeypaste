using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CraftSynth.ImageEditor;

namespace MonkeyPaste {
    public class MpImageEditorControlController : MpController {
        private ImageEditorControl _imageEditorControl { get; set; }
        public ImageEditorControl ImageEditorControl { get { return _imageEditorControl; } set { _imageEditorControl = value; } }

        public MpImageEditorControlController(Image img,MpController parent) : base(parent) {
            ImageEditorControl = new ImageEditorControl();
            ImageEditorControl.InitialImage = img;

        }

        public override void UpdateBounds() {
            throw new NotImplementedException();
        }

        protected override void View_KeyPress(object sender,KeyPressEventArgs e) {
            base.View_KeyPress(sender,e);
        }
    }
}
