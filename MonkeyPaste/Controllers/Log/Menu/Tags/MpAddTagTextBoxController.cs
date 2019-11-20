using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpAddTagTextBoxController:MpController {
        public MpLogMenuTileTokenAddTokenTextBox LogMenuTileTokenAddTokenTextBox { get; set; }

        private MpKeyboardHook _enterHook;

        public MpAddTagTextBoxController(MpController parentController) : base(parentController) {
            LogMenuTileTokenAddTokenTextBox = new MpLogMenuTileTokenAddTokenTextBox(this) {
                ReadOnly = false,
                Multiline = false,
                WordWrap = false,
                BorderStyle = BorderStyle.None,
                BackColor = ((MpTagChooserPanelController)Parent).TagChooserPanel.BackColor
            };
            LogMenuTileTokenAddTokenTextBox.ForeColor = MpHelperSingleton.Instance.IsBright(LogMenuTileTokenAddTokenTextBox.BackColor) ? Color.Black : Color.White;

            LogMenuTileTokenAddTokenTextBox.Enter += LogMenuTileTokenAddTokenTextBox_Enter;
            LogMenuTileTokenAddTokenTextBox.Leave += LogMenuTileTokenAddTokenTextBox_Leave;
            LogMenuTileTokenAddTokenTextBox.KeyPress += LogMenuTileTokenAddTokenTextBox_KeyPress;

            _enterHook = new MpKeyboardHook();
            _enterHook.KeyPressed += _enterHook_KeyPressed;
        }    
        public override void Update() {
            //log menu tile token chooser panel rect
            Rectangle lmttcpr = ((MpTagChooserPanelController)Parent).TagChooserPanel.Bounds;
            //last tile token rect
            int lastTokenIdx = ((MpTagChooserPanelController)Parent).TagPanelControllerList.Count - 1;
            Rectangle lttr = lastTokenIdx < 0 ? Rectangle.Empty : ((MpTagChooserPanelController)Parent).TagPanelControllerList[lastTokenIdx].TagPanel.Bounds;

            LogMenuTileTokenAddTokenTextBox.Font = new Font(
                    Properties.Settings.Default.LogMenuTileTokenFont,
                    (lmttcpr.Height * Properties.Settings.Default.LogMenuTileTokenPanelHeightRatio) - 10.0f,
                    GraphicsUnit.Pixel
            );
            LogMenuTileTokenAddTokenTextBox.Location = new Point(lttr.Right + 10,3);
            LogMenuTileTokenAddTokenTextBox.Size = lttr == Rectangle.Empty ? new Size(lmttcpr.Width,lmttcpr.Height - 10) : new Size(lmttcpr.Width - lttr.Right+ 5,lttr.Height-10);
        }
        private void CreateTag() {
            bool isDuplicate = false;
            foreach(MpTagPanelController ttpc in ((MpTagChooserPanelController)Parent).TagPanelControllerList) {
                if(ttpc.TagTextBoxController.TagTextBox.Text.ToLower() == LogMenuTileTokenAddTokenTextBox.Text.ToLower()) {
                    isDuplicate = true;
                }
            }
            if(LogMenuTileTokenAddTokenTextBox.Text == string.Empty || isDuplicate) {
                Console.WriteLine("MpLogMenuTileTokenAddTokenTextBoxController Warning, add invalidation to ui for duplicate/empty tag in CreateToken()");
                return;
            }
            ((MpTagChooserPanelController)Parent).TagPanelControllerList.Add(new MpTagPanelController(Parent,LogMenuTileTokenAddTokenTextBox.Text,MpHelperSingleton.Instance.GetRandomColor(),MpTagType.Custom));
            ((MpTagChooserPanelController)Parent).TagChooserPanel.Controls.Add(((MpTagChooserPanelController)Parent).TagPanelControllerList[((MpTagChooserPanelController)Parent).TagPanelControllerList.Count - 1].TagPanel);

            LogMenuTileTokenAddTokenTextBox.Text = string.Empty;
            ((MpTagChooserPanelController)Parent).Update();
        }
        private void LogMenuTileTokenAddTokenTextBox_KeyPress(object sender,KeyPressEventArgs e) {
            //old size
            Size os = LogMenuTileTokenAddTokenTextBox.Size;
            //new size
            Size ns = TextRenderer.MeasureText(LogMenuTileTokenAddTokenTextBox.Text,LogMenuTileTokenAddTokenTextBox.Font);

            LogMenuTileTokenAddTokenTextBox.Size = new Size(Math.Max(os.Width,ns.Width),Math.Max(os.Height,ns.Height));
        }
        private void LogMenuTileTokenAddTokenTextBox_Leave(object sender,EventArgs e) {
            if(_enterHook.IsRegistered()) {
                _enterHook.UnregisterHotKey();
                //CreateToken();
            }
            if(!MpLogFormController.EnterHook.IsRegistered()) {
                MpLogFormController.EnterHook.RegisterHotKey(ModifierKeys.None,Keys.Enter);
            }
        }

        private void LogMenuTileTokenAddTokenTextBox_Enter(object sender,EventArgs e) {
            if(MpLogFormController.EnterHook.IsRegistered()) {
                MpLogFormController.EnterHook.UnregisterHotKey();
            }
            if(!_enterHook.IsRegistered()) {
                _enterHook.RegisterHotKey(ModifierKeys.None,Keys.Enter);
            }
        }

        private void _enterHook_KeyPressed(object sender,KeyPressedEventArgs e) {
            CreateTag();
            ((MpLogMenuPanelController)((MpTagChooserPanelController)Parent).Parent).LogMenuSearchTextBoxController.SearchTextBox.Focus();
        }       
    }
}
