using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpLogMenuAddTagTextBoxController:MpController {
        public MpLogMenuTileTokenAddTokenTextBox LogMenuTileTokenAddTokenTextBox { get; set; }

        private MpKeyboardHook _enterHook;

        public MpLogMenuAddTagTextBoxController(MpController parentController) : base(parentController) {
            LogMenuTileTokenAddTokenTextBox = new MpLogMenuTileTokenAddTokenTextBox() {
                ReadOnly = false,
                Multiline = false,
                WordWrap = false,
                BorderStyle = BorderStyle.None,
                BackColor = ((MpLogMenuTileTokenChooserPanelController)Parent).LogMenuTileTokenChooserPanel.BackColor
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
            Rectangle lmttcpr = ((MpLogMenuTileTokenChooserPanelController)Parent).LogMenuTileTokenChooserPanel.Bounds;
            //last tile token rect
            int lastTokenIdx = ((MpLogMenuTileTokenChooserPanelController)Parent).TileTokenPanelControllerList.Count - 1;
            Rectangle lttr = lastTokenIdx < 0 ? Rectangle.Empty : ((MpLogMenuTileTokenChooserPanelController)Parent).TileTokenPanelControllerList[lastTokenIdx].LogMenuTileTokenPanel.Bounds;

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
            foreach(MpLogMenuTileTokenPanelController ttpc in ((MpLogMenuTileTokenChooserPanelController)Parent).TileTokenPanelControllerList) {
                if(ttpc.LogMenuTileTokenTextBoxController.LogMenuTileTokenTextBox.Text.ToLower() == LogMenuTileTokenAddTokenTextBox.Text.ToLower()) {
                    isDuplicate = true;
                }
            }
            if(LogMenuTileTokenAddTokenTextBox.Text == string.Empty || isDuplicate) {
                Console.WriteLine("MpLogMenuTileTokenAddTokenTextBoxController Warning, add invalidation to ui for duplicate/empty tag in CreateToken()");
                return;
            }
            ((MpLogMenuTileTokenChooserPanelController)Parent).TileTokenPanelControllerList.Add(new MpLogMenuTileTokenPanelController(Parent,LogMenuTileTokenAddTokenTextBox.Text,MpHelperSingleton.Instance.GetRandomColor()));
            ((MpLogMenuTileTokenChooserPanelController)Parent).LogMenuTileTokenChooserPanel.Controls.Add(((MpLogMenuTileTokenChooserPanelController)Parent).TileTokenPanelControllerList[((MpLogMenuTileTokenChooserPanelController)Parent).TileTokenPanelControllerList.Count - 1].LogMenuTileTokenPanel);
            LogMenuTileTokenAddTokenTextBox.Text = string.Empty;
            ((MpLogMenuTileTokenChooserPanelController)Parent).Update();
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
            ((MpLogMenuPanelController)((MpLogMenuTileTokenChooserPanelController)Parent).Parent).LogMenuSearchTextBoxController.SearchTextBox.Focus();
        }       
    }
}
