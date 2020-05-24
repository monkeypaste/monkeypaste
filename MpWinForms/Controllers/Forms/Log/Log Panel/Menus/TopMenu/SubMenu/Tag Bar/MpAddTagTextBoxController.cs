using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpAddTagTextBoxController:MpController {
        public TextBox AddTagTextBox { get; set; }

        public MpTagPanelController NewTagPanelController { get; set; }

        //private MpKeyboardHook _enterHook;
        private bool _typingTag = false;

        public MpAddTagTextBoxController(MpController parentController) : base(parentController) {
            AddTagTextBox = new TextBox() {
                ReadOnly = false,
                Multiline = false,
                WordWrap = false,
                BorderStyle = BorderStyle.None,
                BackColor = ((MpTagChooserPanelController)Parent).TagChooserPanel.BackColor
            };
            AddTagTextBox.DoubleBuffered(true);
            AddTagTextBox.ForeColor = MpHelperSingleton.Instance.IsBright(AddTagTextBox.BackColor) ? Color.Black : Color.White;

            AddTagTextBox.KeyUp += AddTagTextBox_KeyUp;
        }


           public override void Update() {
            //log menu tile token chooser panel rect
            Rectangle lmttcpr = ((MpTagChooserPanelController)Parent).TagChooserPanel.Bounds;
            //last tile token rect
            int lastTokenIdx = ((MpTagChooserPanelController)Parent).TagPanelControllerList.Count - 1;
            Rectangle lttr = lastTokenIdx < 0 ? Rectangle.Empty : ((MpTagChooserPanelController)Parent).TagPanelControllerList[lastTokenIdx].TagPanel.Bounds;

            AddTagTextBox.Font = new Font(
                    Properties.Settings.Default.LogMenuTileTokenFont,
                    (lmttcpr.Height * Properties.Settings.Default.LogMenuTileTokenPanelHeightRatio)/* - 10.0f*/,
                    GraphicsUnit.Pixel
            );
            AddTagTextBox.Location = new Point(lttr.Right + 10,3);
            AddTagTextBox.Size = lttr == Rectangle.Empty ? new Size(lmttcpr.Width,lmttcpr.Height - 10) : new Size(lmttcpr.Width - lttr.Right+ 5,lttr.Height-10);

            if(NewTagPanelController != null) {
                NewTagPanelController.Update();
            }

            AddTagTextBox.Invalidate();
        }
        
        private void AddTagTextBox_KeyUp(object sender,KeyEventArgs e) {
            if(_typingTag) {
                return;
            } else {
                _typingTag = true;
            }
            //old size
            Size os = AddTagTextBox.Size;
            //new size
            Size ns = TextRenderer.MeasureText(AddTagTextBox.Text,AddTagTextBox.Font);

            AddTagTextBox.Size = new Size(Math.Max(os.Width,ns.Width),Math.Max(os.Height,ns.Height));
            if(AddTagTextBox.Text.Length > 0) {
                NewTagPanelController = new MpTagPanelController((MpController)Parent,AddTagTextBox.Text,MpHelperSingleton.Instance.GetRandomColor(),MpTagType.Custom);
                ((MpTagChooserPanelController)Parent).TagChooserPanel.Controls.Add(NewTagPanelController.TagPanel);
                ((MpTagChooserPanelController)Parent).TagPanelControllerList.Add(NewTagPanelController);                
                NewTagPanelController.TagTextBoxController.TagTextBox.Focus();
                NewTagPanelController.TagTextBoxController.TagTextBox.DeselectAll();
                NewTagPanelController.TagTextBoxController.TagTextBox.SelectionStart = NewTagPanelController.TagTextBoxController.TagTextBox.Text.Length;
                NewTagPanelController.TagTextBoxController.TagTextBox.KeyUp += TagTextBox_KeyUp;
                AddTagTextBox.Visible = false;

                //((MpLogFormController)Find("MpLogFormController")).DeactivateHotKeys();
                //_enterHook = new MpKeyboardHook();
                //_enterHook.RegisterHotKey(ModifierKeys.None,Keys.Enter);
                //_enterHook.KeyPressed += _enterHook_KeyPressed;

                Update();                
            }
            _typingTag = false;
        }

        private void TagTextBox_KeyUp(object sender,KeyEventArgs e) {
            if(NewTagPanelController.TagTextBoxController.TagTextBox.Text == string.Empty) {
                ((MpTagChooserPanelController)Parent).TagChooserPanel.Controls.Remove(NewTagPanelController.TagPanel);
                ((MpTagChooserPanelController)Parent).TagPanelControllerList.Remove(NewTagPanelController);
                NewTagPanelController = null;
                AddTagTextBox.Visible = true;
                AddTagTextBox.Text = string.Empty;
                AddTagTextBox.Focus();
            } 
            ((MpTagChooserPanelController)Parent).Update();
        }

        //private void _enterHook_KeyPressed(object sender,KeyPressedEventArgs e) {
        //    NewTagPanelController.CreateTag();

        //    ((MpTagChooserPanelController)Parent).Update();

        //    _enterHook.UnregisterHotKey();
        //    _enterHook.Dispose();
        //    _enterHook = null;

        //    ((MpLogFormController)Find("MpLogFormController")).ActivateHotKeys();
        //}
    }
}
