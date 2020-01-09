using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTagPanelController : MpController,IDisposable {
        public MpTagPanel TagPanel { get; set; }
        public MpTag Tag { get; set; }

        public MpTagTextBoxController TagTextBoxController { get; set; }
        public MpTagButtonController TagButtonController { get; set; }

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
            TagPanel = new MpTagPanel() {
                AutoSize = false,
                Radius = 5,
                BorderThickness = 0,
                BackColor = Tag.MpColor.Color == null ? MpHelperSingleton.Instance.GetRandomColor() : Tag.MpColor.Color,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            TagTextBoxController = new MpTagTextBoxController(this,Tag.TagName,TagPanel.BackColor,_isEdit);
            TagPanel.Controls.Add(TagTextBoxController.TagTextBox);

            TagButtonController = new MpTagButtonController(this,_isEdit);
            TagPanel.Controls.Add(TagButtonController.TagButton);
            TagButtonController.ButtonClickedEvent += LogMenuTileTokenButtonController_ButtonClickedEvent;
        }
        private void LogMenuTileTokenButtonController_ButtonClickedEvent(object sender,EventArgs e) {
            if(_isEdit) {
                CreateTag();
                _isEdit = false;
            } else {
                Dispose();
            }
            ((MpTagChooserPanelController)Parent).Update();
        }

        public override void Update() {
            //tile token chooser panel rect
            Rectangle ttcpr = ((MpTagChooserPanelController)Parent).TagChooserPanel.Bounds;
            int thisTagIdx = ((MpTagChooserPanelController)Parent).TagPanelControllerList.IndexOf(this);
            if(thisTagIdx < 0) {
                return;
            }
            //previous tag rect
            Rectangle ptr = thisTagIdx == 0 ? Rectangle.Empty:((MpTagChooserPanelController)Parent).TagPanelControllerList[thisTagIdx-1].TagPanel.Bounds;

            //token panel height
            float tph = (float)ttcpr.Height*Properties.Settings.Default.LogMenuTileTokenPanelHeightRatio;
            //token chooser pad
            int tcp = ttcpr.Height - (int)(tph);
            Font f = new Font(Properties.Settings.Default.LogMenuTileTokenFont,(float)ttcpr.Height-(float)(tcp*1.0f),GraphicsUnit.Pixel);

            //text size
            Size ts = TextRenderer.MeasureText(TagTextBoxController.TagTextBox.Text,f);

            TagPanel.Size = new Size(ts.Width,(int)tph-tcp);
            TagPanel.Location = new Point(ptr.Right+tcp,tcp);
            
            TagButtonController.Update();
            TagTextBoxController.Update();

            TagPanel.Size = new Size(TagTextBoxController.TagTextBox.Width + (int)tph,TagPanel.Height);

            TagButtonController.Update(); //LogMenuTileTokenButtonController.LogMenuTileTokenButton.BringToFront();

            TagPanel.Invalidate();
        }
        public void CreateTag() {
            bool isDuplicate = false;
            foreach(MpTagPanelController ttpc in ((MpTagChooserPanelController)Parent).TagPanelControllerList) {
                if(ttpc.TagTextBoxController.TagTextBox.Text.ToLower() == TagTextBoxController.TagTextBox.Text.ToLower() && ttpc != this) {
                    isDuplicate = true;
                }
            }
            if(TagTextBoxController.TagTextBox.Text == string.Empty || isDuplicate) {
                Console.WriteLine("MpLogMenuTileTokenAddTokenTextBoxController Warning, add invalidation to ui for duplicate/empty tag in CreateToken()");
                return;
            }
            Tag.TagName = TagTextBoxController.TagTextBox.Text;
            Tag.WriteToDatabase();

            TagTextBoxController.TagTextBox.ReadOnly = true;
            TagButtonController.TagButton.Image = Properties.Resources.close2;
            TagButtonController.TagButton.DefaultImage = Properties.Resources.close2;
            TagButtonController.TagButton.OverImage = Properties.Resources.close;
            TagButtonController.TagButton.DownImage = Properties.Resources.close;
            ((MpTagChooserPanelController)Parent).AddTagTextBoxController.AddTagTextBox.Visible = true;
            ((MpTagChooserPanelController)Parent).AddTagTextBoxController.AddTagTextBox.Text = string.Empty;
            ((MpTagChooserPanelController)Parent).AddTagTextBoxController.AddTagTextBox.Focus();
            Update();
        }

        public void Dispose() {
            TagPanel.Visible = false;
            ((MpTagChooserPanelController)Parent).TagPanelControllerList.Remove(this);
            ((MpTagChooserPanelController)Parent).TagChooserPanel.Controls.Remove(TagPanel);
            TagPanel.Dispose();
            Tag.DeleteFromDatabase();
        }
    }
}
