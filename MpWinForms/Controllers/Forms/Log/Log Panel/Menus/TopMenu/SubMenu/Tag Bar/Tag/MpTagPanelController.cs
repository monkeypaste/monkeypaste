using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public enum MpTagPanelState {
        Unselected = 0,
        Selected
    }

    public class MpTagPanelController : MpController,IDisposable {
        //public MpTagTextBoxController TagTextBoxController { get; set; }
        //public MpTagLabelController TagLabelController { get; set; }

        public MpEditableLabelController EditableLabelController { get; set; }

        public Panel TagPanel { get; set; }
        public MpTag Tag { get; set; }
        
        private MpTagPanelState _tagPanelState { get; set; }
        public MpTagPanelState TagPanelState {
            get {
                return _tagPanelState;
            }
        }
        private bool _isEdit = false;
        
        public MpTagPanelController(MpController parentController,MpTag tag) : base(parentController) {
            Tag = tag;
            Init();
        }
        public MpTagPanelController(MpController parentController,string tagText,Color tagColor,MpTagType tagType) : base(parentController) {
            _isEdit = true;
            Tag = new MpTag(tagText,tagColor,tagType);
            Init();            
        }
        private void Init() {
            TagPanel = new Panel() {
                AutoSize = false,
                //Radius = 5,
                //BorderThickness = 0,
                BackColor = Tag.TagColor.Color == null ? MpHelperSingleton.Instance.GetRandomColor() : Tag.TagColor.Color,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            TagPanel.DoubleBuffered(true);

            //TagTextBoxController = new MpTagTextBoxController(this,Tag.TagName,TagPanel.BackColor,_isEdit);
            //TagPanel.Controls.Add(TagTextBoxController.TagTextBox);
            //TagTextBoxController.TagTextBox.Visible = _isEdit;

            //TagLabelController = new MpTagLabelController(this,Tag.TagName,TagPanel.BackColor,_isEdit);
            //TagLabelController.TagLinkLabel.Visible = !_isEdit;

            //TagPanel.Controls.Add(TagLabelController.TagLinkLabel);            
            EditableLabelController = new MpEditableLabelController(
                MpHelperSingleton.Instance.IsBright(Tag.TagColor.Color) ? Color.Black : Color.White, 
                Tag.TagColor.Color, 
                false, 
                Tag.TagName, 
                this,
                true,
                Properties.Settings.Default.TagFontSizeRatio,
                Properties.Settings.Default.TagFont
            );
            TagPanel.Controls.Add(EditableLabelController.TextBox);
            TagPanel.Controls.Add(EditableLabelController.Label);

            UnselectTag();

            DefineEvents();
        }
        public override void DefineEvents() {        
        }
        private void TagPanel_Click(object sender,EventArgs e) {
            if(e.GetType() == typeof(MouseEventArgs)) {
                //for right clicks always show delete context menu 
                if(((MouseEventArgs)e).Button == MouseButtons.Right) {
                    Console.WriteLine("Right mouse clicked on tag: " + Tag.TagName);
                }
            } 
        }
        
        public override Rectangle GetBounds() {
            //tag chooser panel rect
            Rectangle tcpr = ((MpTagChooserPanelController)Parent).GetBounds();
            int thisTagIdx = ((MpTagChooserPanelController)Parent).TagPanelControllerList.IndexOf(this);
            if (thisTagIdx < 0) {
                return Rectangle.Empty;
            }
            //previous tag rect
            Rectangle ptr = thisTagIdx == 0 ? Rectangle.Empty : ((MpTagChooserPanelController)Parent).TagPanelControllerList[thisTagIdx - 1].GetBounds();

            //tag panel height
            float tph = (float)tcpr.Height * Properties.Settings.Default.TagPanelHeightRatio;

            float fontSize = tph * Properties.Settings.Default.TagFontSizeRatio;
            //tag chooser pad
            int tcp = (int)(((float)tcpr.Height - tph)/2.0f);
            Font f = new Font(Properties.Settings.Default.TagFont, fontSize, GraphicsUnit.Pixel);

            //text size
            Size ts = TextRenderer.MeasureText(Tag.TagName, f);

            return new Rectangle(ptr.Right+tcp, tcp, ts.Width, (int)tph);
        }
        public override void Update() {
            TagPanel.Bounds = GetBounds();

            EditableLabelController.Update();

            TagPanel.Invalidate();
        }
        public void SelectTag() {
            TagPanel.BackColor = Tag.TagColor.Color;

            EditableLabelController.Label.BackColor = Tag.TagColor.Color;
            EditableLabelController.Label.ForeColor = MpHelperSingleton.Instance.IsBright(Tag.TagColor.Color) ? Color.Black : Color.White;

            SetTagState(MpTagPanelState.Selected);
        }
        public void UnselectTag() {
            TagPanel.BackColor = Color.Black;
            EditableLabelController.Label.BackColor = Color.Black;
            EditableLabelController.Label.ForeColor = Color.White;

            SetTagState(MpTagPanelState.Unselected);
        }
        private void SetTagState(MpTagPanelState newState) {  
            _tagPanelState = newState;
            Update();
        }
        //private void LogMenuTileTokenButtonController_ButtonClickedEvent(object sender,EventArgs e) {
        //    if(_isEdit) {
        //        CreateTag();
        //    }
        //    else if(TagPanelState == MpTagPanelState.Inactive) {
        //        SetTagState(MpTagPanelState.Selected);
        //    }
        //    else if(TagPanelState == MpTagPanelState.Selected) {
        //        SetTagState(MpTagPanelState.Inactive);
        //    } 
        //    ((MpTagChooserPanelController)Parent).Update();
        //}
        public void Dispose() {
            TagPanel.Visible = false;
            ((MpTagChooserPanelController)Parent).TagPanelControllerList.Remove(this);
            ((MpTagChooserPanelController)Parent).TagChooserPanel.Controls.Remove(TagPanel);
            
            TagPanel.Dispose();
        }
    }
}
