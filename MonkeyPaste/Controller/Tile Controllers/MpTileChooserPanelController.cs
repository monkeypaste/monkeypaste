
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
        public MpTileChooserPanel TileChooserPanel { get; set; } = new MpTileChooserPanel();

        public List<MpTilePanelController> TileControllerList { get; set; } = new List<MpTilePanelController>();

        public Color TileColor1 { get; set; } = Color.Yellow;

        public Color TileColor2 { get; set; } = Color.Cyan;

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

        private MpKeyboardHook _leftHook, _rightHook;

        private static int _PanelCount = 0;

        private int _panelId = 0;

        private int _scrollAccumulator = 0;
        private Timer _focusTimer;

        public MpTileChooserPanelController(MpController parentController) : base(parentController) {
            _panelId = ++_PanelCount;
            TileColor1 = (Color)MpSingletonController.Instance.GetSetting("TileColor1");
            TileColor2 = (Color)MpSingletonController.Instance.GetSetting("TileColor2");                     

            TileChooserPanel = new MpTileChooserPanel() {
                BackColor = (Color)MpSingletonController.Instance.GetSetting("LogPanelBgColor"),
                AutoSize = false,
                //Bounds = new Rectangle(0,sb.Height - h,sb.Width,h)
            };
            TileChooserPanel.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;

            _focusTimer = new Timer();
            _focusTimer.Interval = 10;
            _focusTimer.Tick += _focusTimer_Tick;
            _focusTimer.Start();

            MpSingletonController.Instance.GetMpData().AddOnDataListChangeListener(this);
            UpdateBounds();
            foreach(MpCopyItem ci in MpSingletonController.Instance.GetMpData().GetMpCopyItemList()) {
                AddNewCopyItemPanel(ci);
            }
        }
        public override void UpdateBounds() {
            //logform rect
            Rectangle lfr = ((MpLogFormController)ParentController).LogForm.Bounds;
            //logform drag handle height
            int lfdhh = (int)MpSingletonController.Instance.Settings.GetSetting("LogResizeHandleHeight");
            //logform pad
            int lfp = (int)(lfr.Width * (float)MpSingletonController.Instance.Settings.GetSetting("LogPadRatio"));

            TileChooserPanel.SetBounds(lfp,lfp + lfdhh,lfr.Width - (lfp * 2),lfr.Height - lfdhh);

            foreach(MpTilePanelController citc in TileControllerList) {
                citc.UpdateBounds();
            }

            TileChooserPanel.Refresh();
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
            MpSingletonController.Instance.SetKeyboardHook(MpInputCommand.SelectRightTile,_rightHook);

            _leftHook = new MpKeyboardHook();
            _leftHook.KeyPressed += _leftHook_KeyPressed;
            _leftHook.RegisterHotKey(ModifierKeys.None,Keys.Left);
            _leftHook.RegisterHotKey(ModifierKeys.Shift,Keys.Tab
                );
            MpSingletonController.Instance.SetKeyboardHook(MpInputCommand.SelectLeftTile,_leftHook);

            TileChooserPanel.Focus();
            SelectedTileIdx = TileControllerList.Count - 1;
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
                UpdateBounds();
            }
        }
        public void FilterTiles(string filterStr) {

        }
        public void SearchStrCollection_CollectionChanged(object sender,NotifyCollectionChangedEventArgs e) {
            if(e.NewItems != null) {
                FilterTiles((string)e.NewItems[0]);
            }
            Console.WriteLine("Searching for: " + (string)e.NewItems[0]);
        }
        public void CopyItemCollection_CollectionChanged(object sender,NotifyCollectionChangedEventArgs e) {
            if(e.NewItems != null) {
                foreach(MpCopyItem ci in e.NewItems) {
                    AddNewCopyItemPanel(ci);
                }
            }
            if(e.OldItems != null) {
                List<MpTilePanelController> toRemove = new List<MpTilePanelController>();
                foreach(MpCopyItem ci in e.OldItems) {
                    foreach(MpTilePanelController citc in TileControllerList) {
                        if(citc.CopyItem == ci) {
                            toRemove.Add(citc);
                        }
                    }                    
                }
                foreach(MpTilePanelController toRemoveCitc in toRemove) {
                    TileControllerList.Remove(toRemoveCitc);
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

        private void AddNewCopyItemPanel(MpCopyItem ci) {
            MpTilePanelController newTileController = new MpTilePanelController(TileControllerList.Count,ci,this);
            TileControllerList.Add(newTileController);
            TileChooserPanel.Controls.Add(newTileController.TilePanel);            
            UpdateBounds();
        }
        private void OnTileClick(object sender,MouseEventArgs e) {
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
                ((MpLogFormController)ParentController).PasteCopyItem();
            }
        }   
        protected override void View_KeyPress(object sender,KeyPressEventArgs e) {
            base.View_KeyPress(sender,e);
        }
    }
}
