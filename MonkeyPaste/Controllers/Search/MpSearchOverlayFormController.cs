using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpSearchOverlayFormController : MpController {
        public MpSearchOverlayTextBoxController SearchOverlayTextBoxController { get; set; }
        public MpSearchOverlayForm SearchOverlayForm { get; set; }

        public MpSearchOverlayFormController(MpController parentController) : base(parentController) {
            SearchOverlayForm = new MpSearchOverlayForm() {
                BackColor = Color.FromArgb(0,0,0),
                Opacity = 0.87,
                FormBorderStyle = FormBorderStyle.None,
                AutoSize = false,
                AutoScaleMode = AutoScaleMode.None,
                MinimumSize = new Size(15,200)
            };
            SearchOverlayForm.Load += SearchOverlayForm_Load;
            SearchOverlayForm.FormClosing += SearchOverlayForm_FormClosing;
            SearchOverlayForm.FormClosed += SearchOverlayForm_FormClosed;
            SearchOverlayForm.Leave += SearchOverlayForm_Leave;
            SearchOverlayForm.Deactivate += SearchOverlayForm_Leave;
            SearchOverlayTextBoxController = new MpSearchOverlayTextBoxController(this);
            SearchOverlayForm.Controls.Add(SearchOverlayTextBoxController.SearchTextBox);

            UpdateBounds();

            LinkToViews(new List<object> { SearchOverlayForm });
        }
        private void SearchOverlayForm_Leave(object sender,EventArgs e) {
            HideForm();
        }

        private void SearchOverlayForm_FormClosed(object sender,FormClosedEventArgs e) {
            HideForm();
        }
        private void SearchOverlayForm_Load(object sender,EventArgs e) {
            SearchOverlayForm.Close();
            UpdateBounds();
        }
        private void SearchOverlayForm_FormClosing(object sender,FormClosingEventArgs e) {
            HideForm();
            e.Cancel = true;
        }
        public void ShowForm() {
            UpdateBounds();
            SearchOverlayForm.Show();
            SearchOverlayForm.Activate();
            SearchOverlayForm.BringToFront();
            SearchOverlayTextBoxController.SearchTextBox.Focus();
        }
        public void HideForm() {
            SearchOverlayForm.Hide();
        }
        public override void UpdateBounds() {
            //log form rect
            //Rectangle lfr = ((MpLogFormController)ParentController).LogForm.Bounds;
            //SearchOverlayForm.Bounds = lfr;
            //current screen rect
            Rectangle sr = MpHelperSingleton.Instance.GetScreenBoundsWithMouse();
            int h = MpSingletonController.Instance.CustomLogHeight > 0 ? MpSingletonController.Instance.CustomLogHeight : (int)((float)sr.Height * (float)MpSingletonController.Instance.GetSetting("LogScreenHeightRatio"));
            MpSingletonController.Instance.CustomLogHeight = h;
            SearchOverlayForm.SetBounds(0,sr.Height - h,sr.Width,h);

            SearchOverlayTextBoxController.UpdateBounds();
            SearchOverlayForm.Refresh();
        }
    }
}
