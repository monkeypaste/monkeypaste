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

            DefineEvents();
        }
        public override void DefineEvents() {
            AddTagTextBox.KeyUp += (s, e) => {
                if (_typingTag) {
                    return;
                } else {
                    _typingTag = true;
                }
                //old size
                Size os = AddTagTextBox.Size;
                //new size
                Size ns = TextRenderer.MeasureText(AddTagTextBox.Text, AddTagTextBox.Font);

                AddTagTextBox.Size = new Size(Math.Max(os.Width, ns.Width), Math.Max(os.Height, ns.Height));
                if (AddTagTextBox.Text.Length > 0) {
                    NewTagPanelController = new MpTagPanelController((MpController)Parent, AddTagTextBox.Text, MpHelperSingleton.Instance.GetRandomColor(), MpTagType.Custom);
                    ((MpTagChooserPanelController)Parent).TagChooserPanel.Controls.Add(NewTagPanelController.TagPanel);
                    ((MpTagChooserPanelController)Parent).TagPanelControllerList.Add(NewTagPanelController);
                    NewTagPanelController.EditableLabelController.TextBox.Focus();
                    NewTagPanelController.EditableLabelController.TextBox.DeselectAll();
                    NewTagPanelController.EditableLabelController.TextBox.SelectionStart = NewTagPanelController.EditableLabelController.TextBox.Text.Length;
                    NewTagPanelController.EditableLabelController.TextBox.KeyUp += (s1, e1) => {
                        if (NewTagPanelController.EditableLabelController.TextBox.Text == string.Empty) {
                            ((MpTagChooserPanelController)Parent).TagChooserPanel.Controls.Remove(NewTagPanelController.TagPanel);
                            ((MpTagChooserPanelController)Parent).TagPanelControllerList.Remove(NewTagPanelController);
                            NewTagPanelController = null;
                            AddTagTextBox.Visible = true;
                            AddTagTextBox.Text = string.Empty;
                            AddTagTextBox.Focus();
                        }
                        ((MpTagChooserPanelController)Parent).Update();
                    };
                    AddTagTextBox.Visible = false;

                    //((MpLogFormController)Find("MpLogFormController")).DeactivateHotKeys();
                    //_enterHook = new MpKeyboardHook();
                    //_enterHook.RegisterHotKey(ModifierKeys.None,Keys.Enter);
                    //_enterHook.KeyPressed += _enterHook_KeyPressed;

                    Update();
                }
                _typingTag = false;
            };
        }
        public Font GetFont() {
            //log menu tile token chooser panel rect
            Rectangle lmttcpr = ((MpTagChooserPanelController)Parent).TagChooserPanel.Bounds;

            return new Font(
                    Properties.Settings.Default.TagFont,
                    (lmttcpr.Height * Properties.Settings.Default.TagPanelHeightRatio),
                    GraphicsUnit.Pixel
            );
        }
        public override Rectangle GetBounds() {
            //log menu tile tag chooser panel rect
            Rectangle lmttcpr = ((MpTagChooserPanelController)Parent).TagChooserPanel.Bounds;
            //last tile tag rect
            int lastTagIdx = ((MpTagChooserPanelController)Parent).TagPanelControllerList.Count - 1;
            Rectangle lttr = lastTagIdx < 0 ? Rectangle.Empty : ((MpTagChooserPanelController)Parent).TagPanelControllerList[lastTagIdx].TagPanel.Bounds;
            
            Size tagSize = lttr == Rectangle.Empty ? new Size(lmttcpr.Width, lmttcpr.Height - 10) : new Size(lmttcpr.Width - lttr.Right + 5, lttr.Height - 10);
            return new Rectangle(lttr.Right + 10, 3, tagSize.Width, tagSize.Height);
        }
        public override void Update() {
            AddTagTextBox.Font = GetFont();
            AddTagTextBox.Bounds = GetBounds();

            if(NewTagPanelController != null) {
                NewTagPanelController.Update();
            }

            AddTagTextBox.Invalidate();
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
