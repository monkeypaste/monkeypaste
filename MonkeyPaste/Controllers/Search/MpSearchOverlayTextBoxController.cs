using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpSearchOverlayTextBoxController : MpController {
        private string _placeHolderText = "";

        public TextBox SearchTextBox { get; set; }

        public MpSearchOverlayTextBoxController(MpController Parent) : base(Parent) {
            SearchTextBox = new TextBox() {
                BackColor = Color.White,
                Multiline = false,
                ForeColor = Color.DarkGray,                
                Text = _placeHolderText,
                TextAlign = HorizontalAlignment.Left
            };

            Link(new List<object> { SearchTextBox});
        }


        private void SearchTextBox_LostFocus(object sender,EventArgs e) {
            SearchTextBox.Text = _placeHolderText;
            UpdateView();
        }

        private void SearchTextBox_GotFocus(object sender,EventArgs e) {
            //if(SearchTextBox.Text == _placeHolderText) {
            //    SearchTextBox.Text = string.Empty;
            //    SearchTextBox.ForeColor = Color.Black;
            //}
        }

        public override void UpdateView() {
            //search overlay rect
            Rectangle sor = Rectangle.Empty;

            //search textbox width
            int stbw = (int)((float)sor.Width * (float)MpSingletonController.Instance.GetSetting("SearchTextBoxWidthRatio"));
            //search textbox height
            int stbh = (int)((float)sor.Height * (float)MpSingletonController.Instance.GetSetting("SearchTextBoxHeightRatio"));
            //search form center
            Point sfc = new Point((int)(sor.Width/2)-(int)(stbw/2),(int)(sor.Height/2)-(int)(stbh/2));

            SearchTextBox.SetBounds(sfc.X,sfc.Y,stbw,stbh);
            float fontSize = (float)MpSingletonController.Instance.GetSetting("SearchTextBoxFontSizeRatio") * stbh;
            FontStyle fs = SearchTextBox.Focused ? FontStyle.Regular : FontStyle.Italic;
            SearchTextBox.Font = new Font((string)MpSingletonController.Instance.GetSetting("SearchTextBoxFont"),fontSize,fs,GraphicsUnit.Pixel);
        }
    }
}
