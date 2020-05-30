using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpSplashFormController : MpController {
        public Form SplashForm { get; set; }
        public PictureBox SplashIcon { get; set; }

        public MpSplashFormController(MpController p) : base(p) {
            SplashForm = new Form() {
                AutoSize = false,
                AutoScaleMode = AutoScaleMode.Dpi,
                Size = new Size(500, 500),
                StartPosition = FormStartPosition.CenterScreen,
                FormBorderStyle = FormBorderStyle.None
            };
            SplashIcon = new PictureBox() {
                BackColor = Color.Transparent,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = Properties.Resources.monkey3,
                Location = Point.Empty,
                Size = new Size(500, 500)
            };
            SplashForm.Controls.Add(SplashIcon);
        }
        public void ShowSplash() {
            SplashForm.Show();
        }
        public void HideSplash() {
            SplashForm.Hide();
        }
        public override void Update() {

        }
    }
}
