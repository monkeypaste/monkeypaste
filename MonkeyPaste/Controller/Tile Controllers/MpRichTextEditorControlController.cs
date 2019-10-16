using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using RTFEditor;

namespace MonkeyPaste {
    public class MpRichTextEditorControlController : MpController {
        private MpRichTextEditorPanel _richTextEditorPanel { get; set; }
        public MpRichTextEditorPanel RichTextEditorPanel { get { return _richTextEditorPanel; } set { _richTextEditorPanel = value; } }

        private RTFBox _rtfBoxWpfUserControl { get; set; }
        public RTFBox RtfBoxWpfUserControl { get { return _rtfBoxWpfUserControl; } set { _rtfBoxWpfUserControl = value; } }

        public MpRichTextEditorControlController(string rt,MpController parent) : base(parent) {
            RichTextEditorPanel = new MpRichTextEditorPanel();

            ElementHost host = new ElementHost();
            host.Dock = System.Windows.Forms.DockStyle.Fill;

            RtfBoxWpfUserControl = new RTFBox(RichTextEditorPanel.Handle);
            //RtfBoxWpfUserControl.ToolBarOben.Visibility = System.Windows.Visibility.Collapsed;
            host.Child = RtfBoxWpfUserControl;

            
            RichTextEditorPanel.Controls.Add(host);
            RtfBoxWpfUserControl.SetRTF(rt);
        }

        public override void UpdateBounds() {
            throw new NotImplementedException();
        }

        protected override void View_KeyPress(object sender,KeyPressEventArgs e) {
            base.View_KeyPress(sender,e);
        }
    }
}
