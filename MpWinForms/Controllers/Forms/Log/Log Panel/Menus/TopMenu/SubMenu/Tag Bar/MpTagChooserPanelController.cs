using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTagChooserPanelController : MpController,INotifyPropertyChanged {
        public List<MpTagPanelController> TagPanelControllerList = new List<MpTagPanelController>();
        public MpTagButtonController AddTagButtonController { get; set; }

        public Panel TagChooserPanel { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private MpTagPanelController _selectedTagPanelController;
        public MpTagPanelController SelectedTagPanelController {
            get {
                return _selectedTagPanelController;
            }
            set {
                if(_selectedTagPanelController != value) {
                    if(_selectedTagPanelController != null) {
                        _selectedTagPanelController.UnselectTag();
                    }
                    _selectedTagPanelController = value;
                    _selectedTagPanelController.SelectTag();
                    PropertyChanged(this, new PropertyChangedEventArgs("SelectedTagPanelController"));
                }
            }
        }

        public MpTagChooserPanelController(MpController parentController,List<MpTag> tagList) : base(parentController) {
            TagChooserPanel = new Panel() {
                AutoSize = false,
                BackColor = Properties.Settings.Default.TagChooserBgColor,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                //BorderThickness = 0,
                //Radius = 10
            };
            TagChooserPanel.DoubleBuffered(true);

            AddTagButtonController = new MpTagButtonController(this);
            TagChooserPanel.Controls.Add(AddTagButtonController.TagButton);

            DefineEvents();
        }
        public override void DefineEvents() {
            MpApplication.Instance.DataModel.TagList.CollectionChanged += (s, e) => {
                foreach(MpTag tag in e.NewItems) {
                    AddTagPanelController(tag);
                }
            };

            AddTagButtonController.TagButton.Click += (s,e) => {
                AddTagPanelController(new MpTag("Untitled", Properties.Settings.Default.TagDefaultColor));
            };
        }
        public override Rectangle GetBounds() {
            //log menu panel rect
            Rectangle lmpr = ((MpLogSubMenuPanelController)Parent).LogSubMenuPanel.Bounds;
            //log menu search textbox rect
            Rectangle lmstr = ((MpLogSubMenuPanelController)Parent).LogMenuSearchTextBoxController.SearchTextBox.Bounds;
            //padding
            int lfp = (int)(lmpr.Height * Properties.Settings.Default.LogPadRatio);
            //int h = (int)((float)lmpr.Height * Properties.Settings.Default.LogMenuTileTokenPanelHeightRatio);
            int h = lmpr.Height - lfp - lfp;
            return new Rectangle(lmstr.Right + 5, lmstr.Y, lmpr.Width - lmstr.Right - 10, (int)((float)lmstr.Height * Properties.Settings.Default.TagPanelHeightRatio));
        }
        public override void Update() {
            TagChooserPanel.Bounds = GetBounds();

            foreach(MpTagPanelController ttpc in TagPanelControllerList) {
                ttpc.Update();
            }
            AddTagButtonController.Update();

            TagChooserPanel.Invalidate();
        }
        public void AddTagPanelController(MpTag tag) {
            tag.WriteToDatabase();

            MpTagPanelController tpc = new MpTagPanelController(this, tag);
            tpc.EditableLabelController.Label.Click += (s, e) => {
                if(tpc.TagPanelState == MpTagPanelState.Unselected) {
                    SelectedTagPanelController = tpc;
                }
            };
            TagPanelControllerList.Add(tpc);
            TagChooserPanel.Controls.Add(tpc.TagPanel);
            
            Update();
        }
        public MpTagPanelController GetHistoryTagPanelController() {
            foreach(MpTagPanelController tpc in TagPanelControllerList) {
                if(tpc.Tag.TagId == Properties.Settings.Default.TagHistoryId) {
                    return tpc;
                }
            }
            return null; 
        }
        public MpTagPanelController GetFavoritesTagPanelController() {
            foreach (MpTagPanelController tpc in TagPanelControllerList) {
                if (tpc.Tag.TagId == Properties.Settings.Default.TagFavoritesId) {
                    return tpc;
                }
            }
            return null;
        }
    }
}
