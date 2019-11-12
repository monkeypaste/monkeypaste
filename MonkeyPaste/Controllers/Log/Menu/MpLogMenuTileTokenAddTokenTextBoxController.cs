using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpLogMenuTileTokenAddTokenTextBoxController:MpController {
        public MpLogMenuTileTokenAddTokenTextBox LogMenuTileTokenAddTokenTextBox { get; set; }
        public bool IsEditing { get; set; } = false;

        private MpKeyboardHook _enterHook;
        public MpLogMenuTileTokenAddTokenTextBoxController(MpController parentController) : base(parentController) {
            LogMenuTileTokenAddTokenTextBox = new MpLogMenuTileTokenAddTokenTextBox() {
                ReadOnly = false,
                Multiline = false,
                WordWrap = false,
                Font = new Font(
                    Properties.Settings.Default.LogMenuTileTokenFont,
                    ((MpLogMenuTileTokenChooserPanelController)Parent).LogMenuTileTokenChooserPanel.Height*
                        Properties.Settings.Default.LogMenuTileTokenPanelHeightRatio,
                    GraphicsUnit.Pixel),
                Size = new Size(((MpLogMenuTileTokenChooserPanelController)Parent).LogMenuTileTokenChooserPanel.Height*15,((MpLogMenuTileTokenChooserPanelController)Parent).LogMenuTileTokenChooserPanel.Height),
                BackColor = Color.LimeGreen// ((MpLogMenuTileTokenChooserPanelController)Parent).LogMenuTileTokenChooserPanel.BackColor
            };
            LogMenuTileTokenAddTokenTextBox.ForeColor = MpHelperSingleton.Instance.IsBright(LogMenuTileTokenAddTokenTextBox.BackColor) ? Color.Black : Color.White;
            LogMenuTileTokenAddTokenTextBox.Enter += LogMenuTileTokenAddTokenTextBox_Enter;
            LogMenuTileTokenAddTokenTextBox.Leave += LogMenuTileTokenAddTokenTextBox_Leave;
            _enterHook = new MpKeyboardHook();
            _enterHook.KeyPressed += _enterHook_KeyPressed;
        }
        private void CreateToken() {
            bool isDuplicate = false;
            foreach(MpLogMenuTileTokenPanelController ttpc in ((MpLogMenuTileTokenChooserPanelController)Parent).TileTokenPanelControllerList) {
                if(ttpc.LogMenuTileTokenTextBoxController.LogMenuTileTokenTextBox.Text == LogMenuTileTokenAddTokenTextBox.Text) {
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
        private void LogMenuTileTokenAddTokenTextBox_Leave(object sender,EventArgs e) {
            if(_enterHook.IsRegistered()) {
                _enterHook.UnregisterHotKey();
                CreateToken();
            }
            if(!MpLogFormController.EnterHook.IsRegistered()) {
                MpLogFormController.EnterHook.RegisterHotKey(ModifierKeys.None,Keys.Enter);
            }
            
            IsEditing = false;
        }

        private void LogMenuTileTokenAddTokenTextBox_Enter(object sender,EventArgs e) {
            if(MpLogFormController.EnterHook.IsRegistered()) {
                MpLogFormController.EnterHook.UnregisterHotKey();
            }
            if(!_enterHook.IsRegistered()) {
                _enterHook.RegisterHotKey(ModifierKeys.None,Keys.Enter);
            }
            IsEditing = true;
        }

        private void _enterHook_KeyPressed(object sender,KeyPressedEventArgs e) {
            CreateToken();
        }

        private void LogMenuTileTokenAddTokenTextBox_KeyUp(object sender,System.Windows.Forms.KeyEventArgs e) {
            IsEditing = true;
            LogMenuTileTokenAddTokenTextBox.Size = new Size(LogMenuTileTokenAddTokenTextBox.Height * LogMenuTileTokenAddTokenTextBox.Text.Length,LogMenuTileTokenAddTokenTextBox.Height);
        }
 
        public override void Update() {
            //log menu tile token chooser panel rect
            Rectangle lmttcpr = ((MpLogMenuTileTokenChooserPanelController)Parent).LogMenuTileTokenChooserPanel.Bounds;
            //last tile token rect
            int lastTokenIdx = ((MpLogMenuTileTokenChooserPanelController)Parent).TileTokenPanelControllerList.Count - 1;
            Rectangle lttr = lastTokenIdx < 0 ? Rectangle.Empty: ((MpLogMenuTileTokenChooserPanelController)Parent).TileTokenPanelControllerList[lastTokenIdx].LogMenuTileTokenPanel.Bounds;

            LogMenuTileTokenAddTokenTextBox.Font = new Font(
                    Properties.Settings.Default.LogMenuTileTokenFont,
                    ((MpLogMenuTileTokenChooserPanelController)Parent).LogMenuTileTokenChooserPanel.Height *
                        Properties.Settings.Default.LogMenuTileTokenPanelHeightRatio,
                    GraphicsUnit.Pixel);
            LogMenuTileTokenAddTokenTextBox.Location = new Point(lttr.Right + 10,0);

        }
    }
}
