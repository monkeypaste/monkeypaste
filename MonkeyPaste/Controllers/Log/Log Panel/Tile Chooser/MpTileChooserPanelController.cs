using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MonkeyPaste {

    public class MpTileChooserPanelController : MpController {
        private static int _PanelCount = 0;

        public MpTileChooserPanel TileChooserPanel { get; set; } = new MpTileChooserPanel(0);

        public List<MpTilePanelController> TileControllerList { get; set; } = new List<MpTilePanelController>();
        
        public int PanelId { get; set; } = 0;

        //public MpSelectedTileBorderPanelController SelectedTileBorderPanelController { get; set; }
        
        private MpTilePanelController _selectedTilePanelController = null;
        public MpTilePanelController SelectedTilePanelController {
            get {
                return _selectedTilePanelController;
            }
            set {
                _selectedTilePanelController = value;
                if(_selectedTilePanelController != null) {
                    _selectedTilePanelController.SetState(MpTilePanelState.Selected);
                    ((MpTagChooserPanelController)Find("MpTagChooserPanelController")).UpdateTagListState(_selectedTilePanelController.CopyItem);
                }
            }
        }
        public MpTilePanelController DragTilePanelController { get; set; } = null;
        public Point DragTilePanelStartLocation { get; set; }
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
                       

            _focusTimer = new Timer();
            _focusTimer.Interval = 10;
            _focusTimer.Tick += delegate (object sender,EventArgs e) {
                ScrollTiles(MpSingletonController.Instance.ScrollWheelDelta);
            };
            _focusTimer.Start();

            foreach(MpCopyItem ci in copyItemList) {
                AddNewCopyItemPanel(ci);
            }

            _isInitialLoad = false;

            Link(new List<MpIView> { TileChooserPanel});
        }
        public override void Update() {
            //log form panel rect
            Rectangle lfpr = ((MpLogFormPanelController)Parent).LogFormPanel.Bounds;
            //logform pad
            int lfp = (int)(lfpr.Width * Properties.Settings.Default.LogPadRatio);
            //logformmenu height
            int lfmh = ((MpLogFormPanelController)Parent).LogMenuPanelController.LogMenuPanel.Bounds.Height;
            //tile chooser offset 
            int tco = ((MpTreeViewPanelController)Find("MpTreeViewPanelController")).TreeViewPanel.Right;

            TileChooserPanel.SetBounds(lfp+tco,lfp + lfmh,lfpr.Width - (lfp * 2),lfpr.Height - lfmh - (lfp*2));

            foreach(MpTilePanelController citc in TileControllerList) {
                citc.Update();
            }
            //SelectedTileBorderPanelController.Update();

            TileChooserPanel.Invalidate();
        }
        public MpTilePanelController GetTilePanelControllerAtLocation(Point p) {
            MpTilePanelController clickedTileController = null;
            foreach(MpTilePanelController citc in TileControllerList) {
                Rectangle tileRect = citc.TilePanel.RectangleToScreen(citc.TilePanel.ClientRectangle);
                if(tileRect.Contains(p) || citc.TilePanel.ClientRectangle.Contains(p)) {
                    clickedTileController = citc;
                }
            }
            return clickedTileController;
        }
        
        public void SelectNextTile() {
            SelectedTilePanelController = GetNextTilePanelController(SelectedTilePanelController);
            _showSelectedTilePanelController();
            Update();
        }
        public void SelectPreviousTile() {
            SelectedTilePanelController = GetPreviousTilePanelController(SelectedTilePanelController);
            _showSelectedTilePanelController();
            Update();
        }
        public void SelectTile(MpTilePanelController tpc) {
            SelectedTilePanelController = tpc;
            SelectedTilePanelController.TilePanel.Focus();
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
            if(visibleTileControllerList.Count == 0) {
                return null;

            }
            int nextTileIdx = GetVisibleTilePanelControllerIdx(tpc) + 1;
            if(nextTileIdx >= visibleTileControllerList.Count) {
                nextTileIdx = 0;
            }
            return visibleTileControllerList[nextTileIdx];
        }
        private MpTilePanelController GetPreviousTilePanelController(MpTilePanelController tpc) {
            List<MpTilePanelController> visibleTileControllerList = GetVisibleTilePanelControllerList();
            if(visibleTileControllerList.Count == 0) {
                return null;
            }
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
                MpTilePanelController.OffsetX += deltaX > 0.0f ? MpTilePanelController.TilePanelSize.Width : -MpTilePanelController.TilePanelSize.Width;//(int)((float)deltaX * Properties.Settings.Default.ScrollDampner);
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
                    filteredTileIdxList.Add(i);
                    continue;
                }
                if(ci.CopyItemType == MpCopyItemType.Image) {
                    continue;
                }
                if(ci.CopyItemType == MpCopyItemType.Text) {
                    if(((string)ci.GetData()).ToLower().Contains(searchStr.ToLower())) {
                        filteredTileIdxList.Add(i);
                    }
                }
                else if(ci.CopyItemType == MpCopyItemType.FileList) {
                    foreach(string p in (string[])ci.GetData()) {
                        if(p.ToLower().Contains(searchStr.ToLower())) {
                            filteredTileIdxList.Add(i);
                        }
                    }
                }
            }
            //only show tiles w/ an idx in list
            int vcount = 0;
            for(int i = TileControllerList.Count - 1;i >= 0;i--) {
                if(filteredTileIdxList.Contains(i)) {
                    TileControllerList[i].TilePanel.Visible = true;
                    TileControllerList[i].Update();
                    vcount++;
                }
                else {
                    TileControllerList[i].TilePanel.Visible = false;
                }
            }
            //if(vcount == 0) {
            //    SelectedTileBorderPanelController.TileBorderPanel.Visible = false;
            //} else {
            //Sort("CopyItemId",false);
            //    SelectedTileBorderPanelController.TileBorderPanel.Visible = true;
            //    SelectedTilePanelController = GetVisibleTilePanelControllerList()[0];
            //}
            Sort("CopyItemId",false);
            SelectedTilePanelController = GetVisibleTilePanelControllerList()[0];
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
        private void DeleteCopyItemPanel(MpTilePanelController tpc) {
            var vtl = GetVisibleTilePanelControllerList();
            int dtpcIdx = vtl.IndexOf(tpc);
            if(tpc == SelectedTilePanelController) {
                if(dtpcIdx == vtl.Count - 1) {
                    if(vtl.Count == 1) {
                        SelectedTilePanelController = null;
                    } else {
                        SelectedTilePanelController = vtl[dtpcIdx - 1];
                    }
                } else {
                    SelectedTilePanelController = vtl[dtpcIdx + 1];
                }
            }
            TileControllerList.Remove(tpc);
            MpCopyItem.TotalCopyItemCount = TileControllerList.Count;
            TileChooserPanel.Controls.Remove(tpc.TilePanel);
            Sort("CopyItemId",false);
            tpc.CopyItem.DeleteFromDatabase();            
            tpc.TilePanel.Dispose();
            tpc = null;
            Update();
        }
        public void AddNewCopyItemPanel(MpCopyItem ci) {
            if(Properties.Settings.Default.IsAppendModeActive) {
                if(MpSingletonController.Instance.AppendItem == null && ci.CopyItemType == MpCopyItemType.Text) {
                    MpSingletonController.Instance.AppendItem = ci;
                }
                if(MpSingletonController.Instance.AppendItem != null) {
                    MpSingletonController.Instance.AppendItem.SetData((string)MpSingletonController.Instance.AppendItem.GetData() + Environment.NewLine + (string)ci.GetData());
                    ((MpTileControlRichTextBox)SelectedTilePanelController.TileControlController.ItemControl).AppendText(Environment.NewLine + (string)ci.GetData());                    
                }
            } else {
                ci.WriteToDatabase();
                MpTilePanelController newTileController = new MpTilePanelController(TileControllerList.Count,PanelId,ci,this);
                //newTileController.CopyDateTime = ci.CopyDateTime;
                newTileController.CloseButtonClickedEvent += TilePanelController_CloseButtonClickedEvent;
                newTileController.ExpandButtonClickedEvent += TIlePanelController_ExpandButtonClickedEvent;
                TileControllerList.Add(newTileController);
                TileChooserPanel.Controls.Add(newTileController.TilePanel);
                //if(!_isInitialLoad) {
                //    MpTilePanelController.OffsetX = 0;
                //    SelectedTilePanelController = newTileController;
                //}
                MpCopyItem.TotalCopyItemCount = TileControllerList.Count;
            }
            //if(SelectedTilePanelController != null) {
            //    SelectedTilePanelController.Update();
            //    SelectedTileBorderPanelController.TileBorderPanel.Visible = true;
            //}
            Sort("CopyItemId",false);
            MpTilePanelController.OffsetX = 0;
            //ScrollTiles(-MpTilePanelController.OffsetX,true);
            ScrollTiles(0);
            Update();
        }

        private void TIlePanelController_ExpandButtonClickedEvent(object sender,EventArgs e) {
            throw new NotImplementedException();
        }

        private void TilePanelController_CloseButtonClickedEvent(object sender,EventArgs e) {
            DeleteCopyItemPanel((MpTilePanelController)sender);
        }

        public void Sort(string sortBy,bool ascending) {
            if(ascending) {
                TileControllerList = TileControllerList.OrderBy(x => MpTypeHelper.GetPropertyValue(x.CopyItem,sortBy)).ToList();
            } else {
                TileControllerList = TileControllerList.OrderByDescending(x => MpTypeHelper.GetPropertyValue(x.CopyItem,sortBy)).ToList();
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