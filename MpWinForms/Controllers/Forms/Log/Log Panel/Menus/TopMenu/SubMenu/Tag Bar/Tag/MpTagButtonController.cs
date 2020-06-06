using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTagButtonController : MpController {
        public MpButton TagButton { get; set; }

        public MpTagButtonController(MpController parentController) : base(parentController) {
            TagButton = new MpButton() {                    
                Margin = new Padding(3),
                Padding = Padding.Empty,
                BackColor = Properties.Settings.Default.TagAddTagButtonColor,
                BorderStyle = BorderStyle.None,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = Properties.Resources.add2,
                DefaultImage = Properties.Resources.add2,
                OverImage = Properties.Resources.add,
                DownImage = Properties.Resources.add
            };
            TagButton.DoubleBuffered(true);

            DefineEvents();
        }
        public override void DefineEvents() {
            TagButton.MouseHover += (s, e) => {
                TagButton.Image = TagButton.OverImage;
            };
            TagButton.MouseLeave += (s, e) => {
                TagButton.Image = TagButton.DefaultImage;
            };
        }
        public override Rectangle GetBounds() {
            //tag chooser panel rect
            Rectangle tcpr = ((MpTagChooserPanelController)Parent).GetBounds();
            //token panel height
            int tph = (int)((float)tcpr.Height * Properties.Settings.Default.TagPanelHeightRatio);
            //token chooser pad
            int tcp = tcpr.Height - (int)tph;

            return new Rectangle(tcpr.Right-tph-tcp,tcp,tph,tph);
        }
        public override void Update() {
            TagButton.Bounds = GetBounds();
            TagButton.BringToFront();

            TagButton.Invalidate();
        }
    }
}
