using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public enum MpTagPanelState {
        Inactive = 0,
        Selected,
        Focused
    }

    public class MpTagPanelController : MpController,IDisposable {
        public MpTagPanel TagPanel { get; set; }
        public MpTag Tag { get; set; }

        public MpTagTextBoxController TagTextBoxController { get; set; }
        public MpTagLabelController TagLabelController { get; set; }

        public MpTagButtonController TagButtonController { get; set; }

        public MpTagPanelState TagPanelState { get; set; }

        private bool _isEdit = false;
        private bool _isLinkClick = false;
        private Color _tagColor;

        public MpTagPanelController(MpController parentController,MpTag tag) : base(parentController) {
            Tag = tag;
            Init();
        }
        public MpTagPanelController(MpController parentController,string tagText,Color tagColor,MpTagType tagType) : base(parentController) {
            _isEdit = true;
            _tagColor = tagColor;
            Tag = new MpTag(tagText,_tagColor,tagType);
            Init();            
        }
        private void Init() {
            TagPanel = new MpTagPanel() {
                AutoSize = false,
                //Radius = 5,
                //BorderThickness = 0,
                BackColor = Tag.MpColor.Color == null ? MpHelperSingleton.Instance.GetRandomColor() : Tag.MpColor.Color,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            TagPanel.Click += TagPanel_Click;
            
            _tagColor = TagPanel.BackColor;

            TagTextBoxController = new MpTagTextBoxController(this,Tag.TagName,TagPanel.BackColor,_isEdit);
            TagPanel.Controls.Add(TagTextBoxController.TagTextBox);
            TagTextBoxController.TagTextBox.Visible = _isEdit;
            TagTextBoxController.TagTextBox.Click += TagPanel_Click;

            TagLabelController = new MpTagLabelController(this,Tag.TagName,TagPanel.BackColor,_isEdit);
            TagLabelController.TagLinkLabel.Visible = !_isEdit;
            TagLabelController.TagLinkLabel.LinkClicked += TagLinkLabel_LinkClick;
            TagLabelController.TagLinkLabel.LinkClicked += TagPanel_Click;
            TagLabelController.TagLinkLabel.Click += TagPanel_Click;

            TagPanel.Controls.Add(TagLabelController.TagLinkLabel);
            
            TagButtonController = new MpTagButtonController(this,_isEdit);
            TagPanel.Controls.Add(TagButtonController.TagButton);
            TagButtonController.ButtonClickedEvent += LogMenuTileTokenButtonController_ButtonClickedEvent;
            TagButtonController.TagButton.Click += TagPanel_Click;
            TagButtonController.TagButton.Visible = false;
            
            TagPanelState = MpTagPanelState.Inactive;
        }

        private void TagPanel_Click(object sender,EventArgs e) {
            if(e.GetType() == typeof(MouseEventArgs)) {
                //for right clicks always show delete context menu 
                if(((MouseEventArgs)e).Button == MouseButtons.Right) {
                    Console.WriteLine("Right mouse clicked on tag: " + Tag.TagName);
                }
            } 
        }

        private void TagLinkLabel_LinkClick(object sender,EventArgs e) {
            _isLinkClick = true;
            if(((MouseEventArgs)e).Button == MouseButtons.Left) {
                SetTagState(TagPanelState == MpTagPanelState.Focused ? MpTagPanelState.Inactive : MpTagPanelState.Focused,true);
            }
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
            TagLabelController.Update();

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
            TagLabelController.TagLinkLabel.Text = Tag.TagName;
            Tag.WriteToDatabase();

            TagTextBoxController.TagTextBox.Visible = false;
            TagLabelController.TagLinkLabel.Visible = true;

            TagButtonController.TagButton.Visible = true;
            TagButtonController.TagButton.Image = Properties.Resources.close2;
            TagButtonController.TagButton.DefaultImage = Properties.Resources.close2;
            TagButtonController.TagButton.OverImage = Properties.Resources.close;
            TagButtonController.TagButton.DownImage = Properties.Resources.close;
            ((MpTagChooserPanelController)Parent).AddTagTextBoxController.AddTagTextBox.Visible = true;
            ((MpTagChooserPanelController)Parent).AddTagTextBoxController.AddTagTextBox.Text = string.Empty;
            ((MpTagChooserPanelController)Parent).AddTagTextBoxController.AddTagTextBox.Focus();

            SetTagState(MpTagPanelState.Selected);

            _isEdit = false;
            Update();
        }

        public void SetTagState(MpTagPanelState newState,bool isTemporary = false) {
            var tileChooserController = ((MpTileChooserPanelController)Find("MpTileChooserPanelController"));
            var tagChooserController = ((MpTagChooserPanelController)Find("MpTagChooserPanelController"));
            var ci = ((MpLogFormPanelController)Find("MpLogFormPanelController")).TileChooserPanelController.SelectedTilePanelController.CopyItem;
            bool wasLastFocusedTag = false;

            if(newState == MpTagPanelState.Selected || newState == MpTagPanelState.Focused) {
                TagPanel.BackColor = _tagColor;

                TagLabelController.TagLinkLabel.BackColor = _tagColor;
                TagLabelController.TagLinkLabel.LinkColor = MpHelperSingleton.Instance.IsBright(Tag.MpColor.Color) ? Color.Black : Color.White;    
                
                TagButtonController.TagButton.BackColor = _tagColor;
                TagButtonController.TagButton.Image = Properties.Resources.minus2;
                TagButtonController.TagButton.DefaultImage = Properties.Resources.minus2;
                TagButtonController.TagButton.OverImage = Properties.Resources.minus;
                TagButtonController.TagButton.DownImage = Properties.Resources.minus;

                TagButtonController.TagButton.Visible = true;
                //when this tag is focused ensure that all unfocused selected tags for copy item are TEMPORARILY disabled
                if(newState == MpTagPanelState.Focused) {
                    TagButtonController.TagButton.Visible = false;
                    if(tagChooserController != null && tagChooserController.TagPanelControllerList != null && tagChooserController.TagPanelControllerList.Count > 0) {
                        foreach(MpTagPanelController tpc in tagChooserController.TagPanelControllerList) {
                            if(tpc == this) {
                                continue;
                            }
                            else if(tpc.TagPanelState != MpTagPanelState.Focused) {
                                tpc.SetTagState(MpTagPanelState.Inactive,true);
                            }
                        }
                    }
                }

                if(!isTemporary) {
                    Tag.LinkWithCopyItem(ci);
                }
            } else {
                TagPanel.BackColor = Color.Black;
                TagLabelController.TagLinkLabel.BackColor = Color.Black;
                TagLabelController.TagLinkLabel.LinkColor = Color.White;

                TagButtonController.TagButton.BackColor = Color.Black;
                TagButtonController.TagButton.Image = Properties.Resources.add4;
                TagButtonController.TagButton.DefaultImage = Properties.Resources.add4;
                TagButtonController.TagButton.OverImage = Properties.Resources.add3;
                TagButtonController.TagButton.DownImage = Properties.Resources.add3;
                TagButtonController.TagButton.Visible = true;

                //when this tag WAS focused then unfocus it and if no more tags are focused return to standard state of selected copy item
                if(TagPanelState == MpTagPanelState.Focused) {
                    TagButtonController.TagButton.Visible = false;
                    wasLastFocusedTag = true;
                    if(tagChooserController != null && tagChooserController.TagPanelControllerList != null && tagChooserController.TagPanelControllerList.Count > 0) {
                        foreach(MpTagPanelController tpc in tagChooserController.TagPanelControllerList) {
                            if(tpc == this) {
                                continue;
                            }
                            else if(tpc.TagPanelState == MpTagPanelState.Focused) {
                                wasLastFocusedTag = false;
                            }

                        }
                    }                    
                }

                if(!isTemporary) {
                    Tag.UnlinkWithCopyItem(ci);
                }
            }
            if(wasLastFocusedTag) {
                foreach(MpTagPanelController tpc in tagChooserController.TagPanelControllerList) {
                    if(tpc != this)  {
                        tpc.SetTagState(tpc.Tag.IsLinkedWithCopyItem(ci) == true ? MpTagPanelState.Selected : MpTagPanelState.Inactive,true);
                    }
                }

                foreach(MpTilePanelController tpc in tileChooserController.TileControllerList) {
                    tpc.TilePanel.Visible = true;
                }
            } else if(newState == MpTagPanelState.Focused) {
                foreach(MpTilePanelController tpc in tileChooserController.TileControllerList) {
                    tpc.TilePanel.Visible = false;
                }
            }

            if(newState == MpTagPanelState.Focused || TagPanelState == MpTagPanelState.Focused) {
                foreach(MpTilePanelController tpc in tileChooserController.TileControllerList) {
                    bool showTile = Tag.IsLinkedWithCopyItem(tpc.CopyItem) || newState == MpTagPanelState.Inactive;
                    if(TagPanelState == MpTagPanelState.Focused) {
                        foreach(MpTagPanelController tgpc in tagChooserController.GetFocusedTagList()) {
                            if(tgpc == this) {
                                continue;
                            }
                            else if(tgpc.TagPanelState == MpTagPanelState.Focused && !tgpc.Tag.IsLinkedWithCopyItem(tpc.CopyItem)) {
                                showTile = false;
                            }
                        }
                    }

                    tpc.TilePanel.Visible = showTile;
                }
                tileChooserController.Update();
            }

            TagPanelState = newState;
            Update();
        }
        private void LogMenuTileTokenButtonController_ButtonClickedEvent(object sender,EventArgs e) {
            if(_isEdit) {
                CreateTag();
            }
            else if(TagPanelState == MpTagPanelState.Inactive) {
                SetTagState(MpTagPanelState.Selected);
            }
            else if(TagPanelState == MpTagPanelState.Selected) {
                SetTagState(MpTagPanelState.Inactive);
            }
            ((MpTagChooserPanelController)Parent).Update();
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
