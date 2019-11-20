
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
        private static int _PanelCount = 0;

        public MpTileChooserPanel TileChooserPanel { get; set; } = new MpTileChooserPanel(0);

        public List<MpTilePanelController> TileControllerList { get; set; } = new List<MpTilePanelController>();        
        
        public static MpKeyboardHook LeftHook, RightHook;

        public int PanelId { get; set; } = 0;

        public MpSelectedTileBorderPanelController SelectedTileBorderPanelController { get; set; }

        public MpTilePanelController SelectedTilePanelController { get; set; }

        private int _scrollAccumulator = 0;

        private Timer _focusTimer;

        private bool _isInitialLoad = true;

        public MpTileChooserPanelController(MpController Parent,List<MpCopyItem> copyItemList) : base(Parent) {
            PanelId = ++_PanelCount;
           
            TileChooserPanel = new MpTileChooserPanel(PanelId) {
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                BackColor = Properties.Settings.Default.LogPanelBgColor,
                AutoSize = false
            };
            TileChooserPanel.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;

            SelectedTileBorderPanelController = new MpSelectedTileBorderPanelController(this,PanelId);
            TileChooserPanel.Controls.Add(SelectedTileBorderPanelController.TileBorderPanel);

            _focusTimer = new Timer();
            _focusTimer.Interval = 10;
            _focusTimer.Tick += delegate (object sender,EventArgs e) {
                ScrollTiles(MpSingletonController.Instance.ScrollWheelDelta);
            };
            _focusTimer.Start();

            Update();

            foreach(MpCopyItem ci in copyItemList) {
                AddNewCopyItemPanel(ci);
            }
            _isInitialLoad = false;
            Update();
            Link(new List<MpIView> { TileChooserPanel});
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
            SelectedTileBorderPanelController.Update();

            TileChooserPanel.Invalidate();
        }
        //private MpTilePanelController _SelectedTilePanelController {
        //    get {
        //        if(SelectedTileIdx >= 0) {
        //            return TileControllerList[SelectedTileIdx];
        //        }
        //        return null;
        //    }
        //    set {
        //        SelectedTileIdx = GetCopyItemTileIdx(value.TileId);
        //    }
        //}
        //public MpTilePanelController SelectedTilePanelController {
        //    get {
        //        return _SelectedTilePanelController;
        //    }
        //    set {
        //        _SelectedTilePanelController = value;
        //    }
        //}

        //private int _selectedTileIdx { get; set; } = -1;
        //public int SelectedTileIdx {
        //    get {
        //        return _selectedTileIdx;
        //    }
        //    set {
        //        if(_selectedTileIdx == value) {
        //            Console.WriteLine("Warning, selecting same copy item");
        //            return;
        //        }
        //        if(TileControllerList == null || TileControllerList.Count == 0) {
        //            Console.WriteLine("Warning, attempting to set active panel to an empty panel set");
        //            return;
        //        }
        //        if(value < 0) {
        //            value = TileControllerList.Count - 1;
        //        }
        //        if(value >= TileControllerList.Count) {
        //            value = 0;
        //        }

        //        if(_selectedTileIdx >= 0) {
        //            TileControllerList[_selectedTileIdx].SetFocus(false);
        //        }

        //        TileControllerList[value].SetFocus(true);
        //        int pw = TileChooserPanel.Width;
        //        TileControllerList[value].Update();
        //        int tr = TileControllerList[value].TilePanel.Right;
        //        int tl = TileControllerList[value].TilePanel.Left;
        //        int ox = 0;
        //        if(tr > pw) {
        //            ox = -(tr - pw);
        //        }
        //        else if(tl < 0) {
        //            ox = -tl;
        //        }
        //        if(ox != 0) {
        //            foreach(MpTilePanelController citc in TileControllerList) {
        //                Point l = citc.TilePanel.Location;
        //                l.X += ox;
        //                citc.TilePanel.Location = l;
        //            }
        //        }
        //        _selectedTileIdx = value;

        //        if(SelectedTileBorderPanelController == null) {
        //            SelectedTileBorderPanelController = new MpSelectedTileBorderPanelController(this,PanelId);
        //            TileChooserPanel.Controls.Add(SelectedTileBorderPanelController.TileBorderPanel);
        //        }

        //        //Update();

        //        Console.WriteLine("Active tile changed to " + SelectedTilePanelController.TileId);
        //    }
        //}
        //public MpCopyItem GetSelectedCopyItem() {
        //    if(_selectedTileIdx >= 0) {
        //        return TileControllerList[_selectedTileIdx].CopyItem;
        //    }
        //    return null;
        //}
        public void ActivateHotKeys() {
            if(RightHook != null) {
                DeactivateHotKeys();
            }
            RightHook = new MpKeyboardHook();
            RightHook.KeyPressed += _rightHook_KeyPressed;
            RightHook.RegisterHotKey(ModifierKeys.None,Keys.Right);
            RightHook.RegisterHotKey(ModifierKeys.None,Keys.Tab);

            LeftHook = new MpKeyboardHook();
            LeftHook.KeyPressed += _leftHook_KeyPressed;
            LeftHook.RegisterHotKey(ModifierKeys.None,Keys.Left);
            LeftHook.RegisterHotKey(ModifierKeys.Shift,Keys.Tab);

            TileChooserPanel.Focus();
        }
        public void DeactivateHotKeys() {
            if(RightHook != null) {
                RightHook.UnregisterHotKey();
                RightHook = null;
            }
            if(LeftHook != null) {
                LeftHook.UnregisterHotKey();
                LeftHook = null;
            }
        }

        private void _rightHook_KeyPressed(object sender,KeyPressedEventArgs e) {
            SelectedTilePanelController = GetNextTilePanelController(SelectedTilePanelController);
            _showSelectedTilePanelController();
            Update();
        }
        private void _leftHook_KeyPressed(object sender,KeyPressedEventArgs e) {
            SelectedTilePanelController = GetPreviousTilePanelController(SelectedTilePanelController);
            _showSelectedTilePanelController();
            Update();
        }
        private void _showSelectedTilePanelController() {
            if(SelectedTilePanelController == null) {
                return;
            }
            int pw = TileChooserPanel.Width;
            int tr = SelectedTilePanelController.TilePanel.Right;
            int tl = SelectedTilePanelController.TilePanel.Left;
            int ox = 0;
            int p = 20;
            if(tr > pw) {
                ox = -(tr - pw)-p;
            }
            else if(tl < 0) {
                ox = -tl+p;
            }
            if(ox != 0) {
                ScrollTiles(ox,true);
            }
        }
        private MpTilePanelController GetNextTilePanelController(MpTilePanelController tpc) {
            List<MpTilePanelController> visibleTileControllerList = GetVisibleTilePanelControllerList();
            int nextTileIdx = GetVisibleTilePanelControllerIdx(tpc) + 1;
            if(nextTileIdx >= visibleTileControllerList.Count) {
                nextTileIdx = 0;
            }
            return visibleTileControllerList[nextTileIdx];
        }
        private MpTilePanelController GetPreviousTilePanelController(MpTilePanelController tpc) {
            List<MpTilePanelController> visibleTileControllerList = GetVisibleTilePanelControllerList();
            int previousTileIdx = GetVisibleTilePanelControllerIdx(tpc) - 1;
            if(previousTileIdx < 0) {
                previousTileIdx = visibleTileControllerList.Count-1;
            }
            return visibleTileControllerList[previousTileIdx];
        }
        public List<MpTilePanelController> GetVisibleTilePanelControllerList() {
            List<MpTilePanelController> visibleTilePanelControllerList = new List<MpTilePanelController>();
            foreach(MpTilePanelController tpc in TileControllerList) {
                if(tpc.TilePanel.Visible) {
                    visibleTilePanelControllerList.Add(tpc);
                }
            }
            return visibleTilePanelControllerList;
        }
        public int GetVisibleTilePanelControllerIdx(MpTilePanelController tpc) {
            List<MpTilePanelController> visibleTilePanelControllerList = GetVisibleTilePanelControllerList();
            for(int i = 0;i < visibleTilePanelControllerList.Count;i++) {
                if(tpc == visibleTilePanelControllerList[i]) {
                    return i;
                }
            }
            return -1;
        }
        private int GetTilePanelControllerIdx(MpTilePanelController tpc) {
            for(int i = 0;i < TileControllerList.Count;i++) {
                if(tpc == TileControllerList[i]) {
                    return i;
                }
            }
            return -1;
        }
        public void ScrollTiles(int deltaX,bool forceValue = false) {
            if(deltaX == 0) {
                return;
            }
            if(forceValue) {
                MpTilePanelController.OffsetX += deltaX;
            } else {
                MpTilePanelController.OffsetX += (int)((float)deltaX * Properties.Settings.Default.ScrollDampner);
            }
            MpSingletonController.Instance.ScrollWheelDelta = 0;
            var visibleTileControllerList = GetVisibleTilePanelControllerList();
            int tp = (int)(Properties.Settings.Default.TileChooserPadHeightRatio * TileChooserPanel.Bounds.Height);
            int rx = (visibleTileControllerList.Count-1) * TileChooserPanel.Height + tp;
            int cx = TileChooserPanel.Width - tp - TileChooserPanel.Height;

            if(MpTilePanelController.OffsetX > tp) {
                MpTilePanelController.OffsetX = tp;
            } else if(MpTilePanelController.OffsetX < cx - rx && rx > cx) {
                MpTilePanelController.OffsetX = cx - rx + tp;
            }
            foreach(MpTilePanelController tpc in visibleTileControllerList) {
                tpc.Update();
            }
            SelectedTileBorderPanelController.Update();
        }
        public void ShowTiles() {
            foreach(MpTilePanelController t in TileControllerList) {
                t.TilePanel.Visible = true;
            }
        }
        public void HideTiles() {
            foreach(MpTilePanelController t in TileControllerList) {
                t.TilePanel.Visible = true;
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
                    //TileControllerList[i].SortOrder = searchStr == string.Empty ?  TileControllerList[i].TileId:filteredTileIdxList.IndexOf(i);
                    TileControllerList[i].Update();
                    vcount++;
                }
                else {
                    TileControllerList[i].TilePanel.Visible = false;
                }
            }
            if(vcount == 0) {
                SelectedTileBorderPanelController.TileBorderPanel.Visible = false;
            } else {
                Sort("TileId",false);
                SelectedTileBorderPanelController.TileBorderPanel.Visible = true;
                SelectedTilePanelController = GetVisibleTilePanelControllerList()[0];
            }
            Update();
            _showSelectedTilePanelController();
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
        public void AddNewCopyItemPanel(MpCopyItem ci) {
            if(Properties.Settings.Default.IsAppendModeActive) {
                if(MpSingletonController.Instance.AppendItem == null && ci.copyItemTypeId == MpCopyItemType.Text) {
                    MpSingletonController.Instance.AppendItem = ci;
                }
                if(MpSingletonController.Instance.AppendItem != null) {
                    MpSingletonController.Instance.AppendItem.SetData((string)MpSingletonController.Instance.AppendItem.GetData() + Environment.NewLine + (string)ci.GetData());
                    ((MpTileControlRichTextBox)SelectedTilePanelController.TileControlController.ItemControl).AppendText(Environment.NewLine + (string)ci.GetData());                    
                }
            } else {
                ci.WriteToDatabase();
                MpTilePanelController newTileController = new MpTilePanelController(TileControllerList.Count,PanelId,ci,this);
                TileControllerList.Add(newTileController);
                TileChooserPanel.Controls.Add(newTileController.TilePanel);
                if(!_isInitialLoad) {
                    MpTilePanelController.OffsetX = 0;
                }
                SelectedTilePanelController = newTileController;
            }
            SelectedTilePanelController.Update();
            SelectedTileBorderPanelController.TileBorderPanel.Visible = true;
            Sort("TileId",false);
            Update();
        }
        private void Sort(string sortBy,bool ascending) {
            if(ascending) {
                TileControllerList = TileControllerList.OrderBy(x => MpTypeHelper.GetPropertyValue(x,sortBy)).ToList();
            } else {
                TileControllerList = TileControllerList.OrderByDescending(x => MpTypeHelper.GetPropertyValue(x,sortBy)).ToList();
            }
        }
    }

    public enum MpTileSortType {
        TileId = 0,
        CopyApp,
        PasteApp,
        Title,
        ClipType,
        Content,
        ClipLength
    };
}