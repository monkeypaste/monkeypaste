
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTileChooserPanelController : MpController {
        public MpTileChooserPanel TileChooserPanel { get; set; } = new MpTileChooserPanel(0);

        public List<MpTilePanelController> TileControllerList { get; set; } = new List<MpTilePanelController>();
        
        private MpTilePanelController _selectedTileController {
            get {
                if(SelectedTileIdx >= 0) {
                    return TileControllerList[SelectedTileIdx];
                }
                return null;
            }
            set {
                SelectedTileIdx = GetCopyItemTileIdx(value.TileId);
            }
        }
        public MpTilePanelController SelectedTileController {
            get {
                return _selectedTileController;
            }
            set {
                _selectedTileController = value;
            }
        }

        private int _selectedTileIdx { get; set; } = -1;
        public int SelectedTileIdx {
            get {
                return _selectedTileIdx;
            }
            set {
                if(_selectedTileIdx == value) {
                    Console.WriteLine("Warning, selecting same copy item");
                    return;
                }
                if(TileControllerList == null || TileControllerList.Count == 0) {
                    Console.WriteLine("Warning, attempting to set active panel to an empty panel set");
                    return;
                }
                if(value < 0) {
                    value = TileControllerList.Count - 1;
                }
                if(value >= TileControllerList.Count) {
                    value = 0;
                }

                if(_selectedTileIdx >= 0) {
                    TileControllerList[_selectedTileIdx].SetFocus(false);
                }

                TileControllerList[value].SetFocus(true);
                int pw = TileChooserPanel.Width;
                int tr = TileControllerList[value].TilePanel.Right;
                int tl = TileControllerList[value].TilePanel.Left;
                int ox = 0;
                if(tr > pw) {
                    ox = -(tr - pw);
                }
                else if(tl < 0) {
                    ox = -tl;
                }
                if(ox != 0) {
                    foreach(MpTilePanelController citc in TileControllerList) {
                        Point l = citc.TilePanel.Location;
                        l.X += ox;
                        citc.TilePanel.Location = l;
                    }
                }
                _selectedTileIdx = value;

                Console.WriteLine("Active tile changed to " + _selectedTileIdx);
            }
        }
        private MpRoundedPanel _tileBorderPanel;

        private MpKeyboardHook _leftHook, _rightHook;

        private static int _PanelCount = 0;

        private int _panelId = 0;

        private int _scrollAccumulator = 0;
        private Timer _focusTimer;

        public MpTileChooserPanelController(MpController Parent) : base(Parent) {
            _panelId = ++_PanelCount;
           
            TileChooserPanel = new MpTileChooserPanel(_panelId) {
                BackColor = Properties.Settings.Default.LogPanelBgColor,
                AutoSize = false
            };
            TileChooserPanel.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;

            _tileBorderPanel = new MpRoundedPanel() {
                BackColor = Properties.Settings.Default.TileFocusColor,
                AutoSize = false,
                BorderColor = Properties.Settings.Default.TileFocusColor,
                Thickness = Properties.Settings.Default.TileBorderThickness,
                Radius = Properties.Settings.Default.TileBorderRadius
            };

            _focusTimer = new Timer();
            _focusTimer.Interval = 10;
            _focusTimer.Tick += _focusTimer_Tick;
            _focusTimer.Start();

            MpSingletonController.Instance.GetMpData().AddOnDataListChangeListener(this);
            Update();
            foreach(MpCopyItem ci in MpSingletonController.Instance.GetMpData().GetMpCopyItemList()) {
                AddNewCopyItemPanel(ci);
            }
            SelectedTileIdx = TileControllerList.Count - 1;
            Update();
            Link(new List<MpIView> { (MpIView)TileChooserPanel });
        }
        public override void Update() {
            //logform rect
            Rectangle lfr = ((MpLogFormController)Parent).LogForm.Bounds;
            //logform drag handle height
            int lfdhh = Properties.Settings.Default.LogResizeHandleHeight;
            //logform pad
            int lfp = (int)(lfr.Width * Properties.Settings.Default.LogPadRatio);
            //logformmenu height
            int lfmh = (int)((float)lfr.Height * Properties.Settings.Default.LogMenuHeightRatio);

            TileChooserPanel.SetBounds(lfp,lfp + lfdhh + lfmh,lfr.Width - (lfp * 2),lfr.Height - lfdhh - lfmh);

            foreach(MpTilePanelController citc in TileControllerList) {
                citc.Update();
            }
            if(TileControllerList.Count > 0) {
                _tileBorderPanel.Size = new Size(TileControllerList[0].TilePanel.Width + Properties.Settings.Default.TileBorderThickness,TileControllerList[0].TilePanel.Height + Properties.Settings.Default.TileBorderThickness);
            }
            
            //TileChooserPanel.Refresh();
        }
        private int GetTileIdxBySortOrder(int sortOrder) {
            for(int i = 0;i < TileControllerList.Count;i++) { 
                if(TileControllerList[i].SortOrder == sortOrder) {
                    return i;
                }
            }
            return -1;
        }
        private void _focusTimer_Tick(object sender,EventArgs e) {
            ScrollTiles(MpSingletonController.Instance.ScrollWheelDelta);
        }
        public MpCopyItem GetSelectedCopyItem() {
            if(_selectedTileIdx >= 0) {
                return TileControllerList[_selectedTileIdx].CopyItem;
            }
            return null;
        }
        public void ActivateHotKeys() {
            if(_rightHook != null) {
                DeactivateHotKeys();
            }
            _rightHook = new MpKeyboardHook();
            _rightHook.KeyPressed += _rightHook_KeyPressed;
            _rightHook.RegisterHotKey(ModifierKeys.None,Keys.Right);
            _rightHook.RegisterHotKey(ModifierKeys.None,Keys.Tab);

            _leftHook = new MpKeyboardHook();
            _leftHook.KeyPressed += _leftHook_KeyPressed;
            _leftHook.RegisterHotKey(ModifierKeys.None,Keys.Left);
            _leftHook.RegisterHotKey(ModifierKeys.Shift,Keys.Tab
                );

            TileChooserPanel.Focus();
            //SelectedTileIdx = TileControllerList.Count - 1;
            //SetActiveTile(CopyItemTileControllerList.Count - 1);
        }
        public void DeactivateHotKeys() {
            if(_rightHook != null) {
                _rightHook.UnregisterHotKey();
                _rightHook = null;
            }
            if(_leftHook != null) {
                _leftHook.UnregisterHotKey();
                _leftHook = null;
            }
            /*if(CopyItemTileControllerList.Count > 0 && _selectedCopyItemIdx >= 0) {
                CopyItemTileControllerList[_selectedCopyItemIdx].SetFocus(false);
            }
            SetActivePanel(CopyItemTileControllerList.Count - 1);*/
        }

        
        private void _rightHook_KeyPressed(object sender,KeyPressedEventArgs e) {
            SelectedTileIdx -= 1;
           // SetActiveTile(_selectedCopyItemIdx - 1);
        }
        private void _leftHook_KeyPressed(object sender,KeyPressedEventArgs e) {
            SelectedTileIdx++;
           // SetActiveTile(_selectedCopyItemIdx + 1);
        }
        public void ScrollTiles(int deltaX) {
            _scrollAccumulator += deltaX;
            if(Math.Abs(_scrollAccumulator) > 10) {
                int deltaSelectedIdx = _scrollAccumulator > 0 ? 1 : -1;
                SelectedTileIdx += deltaSelectedIdx;
                //SetActiveTile(_selectedCopyItemIdx + deltaSelectedIdx);
                _scrollAccumulator = 0;
                MpSingletonController.Instance.ScrollWheelDelta = 0;
            }            
        }
        public void OnFormResize(Rectangle newBounds) {
            if(TileChooserPanel != null) {
                TileChooserPanel.Bounds = newBounds;
                Update();
            }
        }
        public int GetVisibleTileCount() {
            int count = 0;
            foreach(MpTilePanelController tpc in TileControllerList) {
                if(tpc.TilePanel.Visible) {
                    count++;
                }
            }
            return count;
        }
        public void FilterTiles(string searchStr) {
            List<int> filteredTileIdxList = new List<int>();
            //search ci's from newest to oldest for filterstr, adding idx to list
            for(int i = TileControllerList.Count - 1;i >= 0;i--) {
                //when search string is empty add each item to list so all shown
                if(searchStr == string.Empty) {
                    filteredTileIdxList.Add(i);
                    continue;
                }
                MpCopyItem ci = TileControllerList[i].CopyItem;
                if(ci.Title.ToLower().Contains(searchStr.ToLower()) || ci.App.SourcePath.ToLower().Contains(searchStr.ToLower())) {
                    filteredTileIdxList.Add(i); //filteredTileIdxList = AddFilterIdx(filteredTileIdxList,i);
                    continue;
                }
                if(ci.copyItemTypeId == MpCopyItemType.Image) {
                    continue;
                }
                if(ci.copyItemTypeId == MpCopyItemType.Text) {
                    if(((string)ci.GetData()).ToLower().Contains(searchStr.ToLower())) {
                        filteredTileIdxList.Add(i); //filteredTileIdxList = AddFilterIdx(filteredTileIdxList,i);
                    }
                }
                else if(ci.copyItemTypeId == MpCopyItemType.FileList) {
                    foreach(string p in (string[])ci.GetData()) {
                        if(p.ToLower().Contains(searchStr.ToLower())) {
                            filteredTileIdxList.Add(i); //filteredTileIdxList = AddFilterIdx(filteredTileIdxList,i);
                        }
                    }
                }
            }
            //only show tiles w/ an idx in list
            int vcount = 0;
            for(int i = TileControllerList.Count - 1;i >= 0;i--) {
                if(filteredTileIdxList.Contains(i)) {
                    TileControllerList[i].TilePanel.Visible = true;
                    TileControllerList[i].SortOrder = searchStr == string.Empty ?  TileControllerList[i].TileId:filteredTileIdxList.IndexOf(i);
                    TileControllerList[i].Update();
                }
                else {
                    TileControllerList[i].TilePanel.Visible = false;
                }
            }  
        }
        private List<int> AddFilterIdx(List<int> idxList,int newIdx) {
            foreach(int idx in idxList) {
                if(idx == newIdx) {
                    return idxList;
                }
            }
            idxList.Add(newIdx);
            return idxList;
        }
        public void CopyItemCollection_CollectionChanged(object sender,NotifyCollectionChangedEventArgs e) {
            if(e.NewItems != null) {
                foreach(MpCopyItem ci in e.NewItems) {
                    AddNewCopyItemPanel(ci);
                }
            }
            if(e.OldItems != null) {
                foreach(MpCopyItem ci in e.OldItems) {
                    RemoveCopyItemPanel(ci);                  
                }
            }
        }        
        public int GetCopyItemTileIdx(int copyItemTileId) {
            for(int i = 0;i < TileControllerList.Count;i++) {
                if(TileControllerList[i].TileId == copyItemTileId) {
                    return i;
                }
            }
            return -1;
        }
        private void RemoveCopyItemPanel(MpCopyItem ci) {
            List<MpTilePanelController> toRemove = new List<MpTilePanelController>();
            foreach(MpTilePanelController citc in TileControllerList) {
                if(citc.CopyItem == ci) {
                    toRemove.Add(citc);
                }
            }
            foreach(MpTilePanelController toRemoveCitc in toRemove) {
                TileControllerList.Remove(toRemoveCitc);
                TileChooserPanel.Controls.Remove(toRemoveCitc.TilePanel);
            }
        }
        private void AddNewCopyItemPanel(MpCopyItem ci) {
            MpTilePanelController newTileController = new MpTilePanelController(TileControllerList.Count,_panelId,ci,this);
            TileControllerList.Add(newTileController);
            TileChooserPanel.Controls.Add(newTileController.TilePanel);
            newTileController.activeChangedEvent += NewTileController_activeChangedEvent;
            Update();
        }

        private void NewTileController_activeChangedEvent(object sender,bool isActive) {
            if(isActive) {
                _tileBorderPanel.Location = new Point(((MpTilePanelController)sender).TilePanel.Location.X - Properties.Settings.Default.TileBorderThickness,((MpTilePanelController)sender).TilePanel.Location.Y - Properties.Settings.Default.TileBorderThickness);
                _tileBorderPanel.Size = new Size(((MpTilePanelController)sender).TilePanel.Width + Properties.Settings.Default.TileBorderThickness,((MpTilePanelController)sender).TilePanel.Height + Properties.Settings.Default.TileBorderThickness);
                if(!TileChooserPanel.Controls.Contains(_tileBorderPanel)) {
                    TileChooserPanel.Controls.Add(_tileBorderPanel);
                }
            }
        }
        /*private void OnTileClick(object sender,MouseEventArgs e) {
   //At this level if anywhere of the tile is clicked it becomes selected
   MpTilePanelController clickedTileController = null;
   foreach(MpTilePanelController citc in TileControllerList) {
       if(citc.TilePanel.RectangleToScreen(citc.TilePanel.ClientRectangle).Contains(e.Location)) {
           clickedTileController = citc;
       }
   }
   if(clickedTileController != null) {

       SelectedTileController = clickedTileController;
   }
}
private void OnTileDoubleClick(object sender,MouseEventArgs e) {
   //if a tile is doubleclicked it is automatically pasted and select3ed as a side effect
   MpTilePanelController clickedTileController = null;
   foreach(MpTilePanelController citc in TileControllerList) {
       if(citc.TilePanel.RectangleToScreen(citc.TilePanel.ClientRectangle).Contains(e.Location)) {
           clickedTileController = citc;
       }
   }
   if(clickedTileController != null) {


       SelectedTileController = clickedTileController;
       ((MpLogFormController)Parent).PasteCopyItem();
   }

}   */
    }
}
