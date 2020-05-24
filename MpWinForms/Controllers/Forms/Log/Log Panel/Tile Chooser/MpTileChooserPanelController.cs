using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using VisualEffects.Animations.Effects;
using VisualEffects.Easing;

namespace MonkeyPaste {

    public class MpTileChooserPanelController : MpControlController    {
        public List<MpTilePanelController> TileControllerList { get; set; } = new List<MpTilePanelController>();

        public MpTileChooserPanel TileChooserPanel { get; set; } = new MpTileChooserPanel(0);

        public MpTilePanelController SelectedTilePanelController {
            get {
                return _selectedTilePanelController;
            }
            set {
                _selectedTilePanelController = value;
                if (_selectedTilePanelController != null) {
                    _selectedTilePanelController.SetState(MpTilePanelStateType.Selected);
                    ((MpTagChooserPanelController)Find("MpTagChooserPanelController")).UpdateTagListState(_selectedTilePanelController.CopyItem);
                }
            }
        }
        //public MpTilePanelController DragTilePanelController { get; set; } = null;

        private MpTilePanelController _selectedTilePanelController = null;

        private Timer _focusTimer;

        private bool _isInitialLoad = true;

        private MpTileChooserPanelStateType _tileChooserStateType = MpTileChooserPanelStateType.None;
        public MpTileChooserPanelStateType TileChooserStateType {
            get {
                return _tileChooserStateType;
            }
        }
        public delegate void StateChanged(object sender, MpTileChooserPanelStateChangedEventArgs e);
        public event StateChanged StateChangedEvent;

        public MpTileChooserPanelController(MpController Parent) : base(Parent) {
            // TODO Scale tile chooser for multiple chooser panels
            TileChooserPanel = new MpTileChooserPanel(0) {
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                BackColor = Properties.Settings.Default.LogPanelBgColor,
                Bounds = GetBounds(),
                AutoSize = false
            };
            TileChooserPanel.DoubleBuffered(true);
            TileChooserPanel.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;

            MpCommandManager.Instance.ClipboardCommander.ClipboardChangedEvent += ClipboardController_ClipboardChangedEvent;

            _focusTimer = new Timer();
            _focusTimer.Interval = 10;
            _focusTimer.Tick += delegate (object sender,EventArgs e) {
                //ScrollTiles(MpSingletonController.Instance.ScrollWheelDelta);
            };
            _focusTimer.Start();

            foreach(MpCopyItem ci in MpAppManager.Instance.DataModel.CopyItemList) {
                AddNewCopyItemPanel(ci);
            }
            Update();
            //FilterTiles(string.Empty);
            _isInitialLoad = false;
        }
        public override Rectangle GetBounds() {
            //log form panel rect
            Rectangle lfpr = ((MpLogFormPanelController)Parent).GetBounds();
            //logform pad
            int lfp = (int)(lfpr.Width * Properties.Settings.Default.LogPadRatio);
            //logformmenu height
            int lfmh = ((MpLogFormPanelController)Parent).LogMenuPanelController.LogMenuPanel.Bounds.Height;
            //tile chooser offset 
            int tco = ((MpTreeViewPanelController)Find("MpTreeViewPanelController")).TreeViewPanel.Right;

            return new Rectangle(lfp + tco, lfp + lfmh, lfpr.Width - (lfp * 2), lfpr.Height - lfmh - (lfp * 2));
        }

        public void AddNewCopyItemPanel(MpCopyItem ci) {
            if (Properties.Settings.Default.IsAppendModeActive) {
                if (MpSingletonController.Instance.AppendItem == null && ci.CopyItemType == MpCopyItemType.Text) {
                    MpSingletonController.Instance.AppendItem = ci;
                }
                if (MpSingletonController.Instance.AppendItem != null) {
                    MpSingletonController.Instance.AppendItem.SetData((string)MpSingletonController.Instance.AppendItem.GetData() + Environment.NewLine + (string)ci.GetData());
                    SelectedTilePanelController.TileContentController.ScrollPanelController.TileContentControlController.UpdateItem(MpSingletonController.Instance.AppendItem);
                }
            }
            else {
                ci.WriteToDatabase();
                MpTag historyTag = null;
                foreach(MpTag t in MpAppManager.Instance.DataModel.TagList) {
                    if(t.TagName == "History") {
                        historyTag = t;
                    }
                }
                if(historyTag != null) {
                    historyTag.LinkWithCopyItem(ci);
                }

                MpTilePanelController newTileController = new MpTilePanelController(ci, this);
                newTileController.CloseButtonClickedEvent += TilePanelController_CloseButtonClickedEvent;
                newTileController.ExpandButtonClickedEvent += TIlePanelController_ExpandButtonClickedEvent;
                newTileController.TileContentController.ScrollPanelController.VScrollbarPanelController.ScrollStartEvent += (s, e) => {
                    SetState(MpTileChooserPanelStateType.Scrolling);
                };
                newTileController.TileContentController.ScrollPanelController.HScrollbarPanelController.ScrollStartEvent += (s, e) => {
                    SetState(MpTileChooserPanelStateType.Scrolling);
                };
                newTileController.TileContentController.ScrollPanelController.VScrollbarPanelController.ScrollEndEvent += (s, e) => {
                    SetState(MpTileChooserPanelStateType.None);
                };
                newTileController.TileContentController.ScrollPanelController.HScrollbarPanelController.ScrollEndEvent += (s, e) => {
                    SetState(MpTileChooserPanelStateType.None);
                };
                TileControllerList.Add(newTileController);
                TileChooserPanel.Controls.Add(newTileController.TilePanel);
                if(!_isInitialLoad) {
                    Update();
                }
                MpCopyItem.TotalCopyItemCount = TileControllerList.Count;
                //MpTilePanelController.OffsetX += MpTilePanelController.TilePanelSize.Width;
            }

            //Sort("CopyItemId",false);
            //ScrollTiles(480);
        }

        public override void Update() {
            TileChooserPanel.Bounds = GetBounds();

            foreach(MpTilePanelController citc in TileControllerList) {
                citc.Update();
            }
            //SelectedTilePanelController.Update();

            TileChooserPanel.Invalidate();
        }
        private void ClipboardController_ClipboardChangedEvent(object sender, MpCopyItem copyItem) {
            AddNewCopyItemPanel(copyItem);
            SelectTile(TileControllerList[TileControllerList.Count - 1]);
        }
        public void BeginAnimation() {
            int t = 500;
            int d = 50;

            if(TileControllerList.Count > 0) {
                int fy = SelectedTilePanelController.TilePanel.Location.Y;
                SelectedTilePanelController.TilePanel.Location = new Point(SelectedTilePanelController.TilePanel.Location.X, TileChooserPanel.Bottom);
                SelectedTilePanelController.TilePanel.Invalidate();
                int idx = TileControllerList.IndexOf(SelectedTilePanelController);
                //SelectedTilePanelController.TilePanel.Animate(new YLocationEffect(),EasingFunctions.SineEaseOut,fy,t,idx*d);
            }
            foreach(MpTilePanelController tpc in GetVisibleTilePanelControllerList()) {
                int fy = tpc.TilePanel.Location.Y;
                tpc.TilePanel.Location = new Point(tpc.TilePanel.Location.X, TileChooserPanel.Bottom);
                tpc.TilePanel.Invalidate();
                int idx = TileControllerList.IndexOf(tpc);
                tpc.AnimateTileY(fy, t, idx * d, EasingFunctions.CircEaseIn);
            }
        }
        public void SetState(MpTileChooserPanelStateType newState) {
            
            _tileChooserStateType = newState;
            StateChangedEvent(this, new MpTileChooserPanelStateChangedEventArgs(_tileChooserStateType));
            Update();
        }
        public void SelectNextTile() {
            SelectedTilePanelController = GetNextTilePanelController(SelectedTilePanelController);
            ShowSelectedTilePanelController();
            Update();
        }
        public void SelectPreviousTile() {
            SelectedTilePanelController = GetPreviousTilePanelController(SelectedTilePanelController);
            ShowSelectedTilePanelController();
            Update();
        }
        public void SelectTile(MpTilePanelController tpc) {
            //ignore if same tile
            if(tpc == SelectedTilePanelController) {
                return;
            }
            //unselect previous tile
            if(SelectedTilePanelController != null) {
                SelectedTilePanelController.SetState(MpTilePanelStateType.Unselected);
            }

            SelectedTilePanelController = tpc;
            SelectedTilePanelController.TilePanel.Focus();
            SelectedTilePanelController.SetState(MpTilePanelStateType.Selected);
            Update();
        }
        private void ShowSelectedTilePanelController() {
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
        public MpTilePanelController GetTilePanelControllerFromTitleAtLocation(Point p) {
            MpTilePanelController clickedTileController = null;
            foreach (MpTilePanelController citc in TileControllerList) {
                Rectangle tileRect = citc.TileTitlePanelController.TileTitlePanel.RectangleToScreen(citc.TileTitlePanelController.TileTitlePanel.ClientRectangle);
                if (tileRect.Contains(p) || citc.TileTitlePanelController.TileTitlePanel.ClientRectangle.Contains(p)) {
                    clickedTileController = citc;
                }
            }
            return clickedTileController;
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
        public void ScrollTiles(int deltaX,bool forceValue = false) {
            if(deltaX == 0) {
                return;
            }
            //if(forceValue) {
            //    MpTilePanelController.OffsetX += deltaX;
            //} else {
            //    MpTilePanelController.OffsetX += deltaX > 0.0f ? MpTilePanelController.TilePanelSize.Width : -MpTilePanelController.TilePanelSize.Width;//(int)((float)deltaX * Properties.Settings.Default.ScrollDampner);
            //}
            //MpSingletonController.Instance.ScrollWheelDelta = 0;
            //var visibleTileControllerList = GetVisibleTilePanelControllerList();
            //int tp = (int)(Properties.Settings.Default.TileChooserPadHeightRatio * TileChooserPanel.Bounds.Height);
            //int rx = (visibleTileControllerList.Count-1) * TileChooserPanel.Height + tp;
            //int cx = TileChooserPanel.Width - tp - TileChooserPanel.Height;

            //if(MpTilePanelController.OffsetX > tp) {
            //    MpTilePanelController.OffsetX = tp;
            //} else if(MpTilePanelController.OffsetX < cx - rx && rx > cx) {
            //    MpTilePanelController.OffsetX = cx - rx + tp;
            //}
            //foreach(MpTilePanelController tpc in visibleTileControllerList) {
            //    tpc.Update();
            //}
            MpSingletonController.Instance.ScrollWheelDelta = 0;
            int minx = -GetVisibleTileCount() * MpTilePanelController.TilePanelSize.Width;
            MpTilePanelController.OffsetX += deltaX;
            if(MpTilePanelController.OffsetX > 0)
            {
                MpTilePanelController.OffsetX = 0;
            } else if(MpTilePanelController.OffsetX < minx)
            {
                MpTilePanelController.OffsetX = minx;
            }
            Update();
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
                if(ci.Title.ToLower().Contains(searchStr.ToLower()) || ci.App.AppPath.ToLower().Contains(searchStr.ToLower())) {
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
            Sort("CopyItemId",false);

            var visibleTileControllerList = GetVisibleTilePanelControllerList();
            SelectedTilePanelController = (visibleTileControllerList == null || visibleTileControllerList.Count == 0) ? null:visibleTileControllerList[0];

            Update();
            ShowSelectedTilePanelController();
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
    public class MpTileChooserPanelStateChangedEventArgs : EventArgs {
        public MpTileChooserPanelStateType NewState { get; set; }
        public MpTileChooserPanelStateChangedEventArgs(MpTileChooserPanelStateType newState) {
            NewState = newState;
        }
    }
    public enum MpTileChooserPanelStateType {
        None = 0,
        Hidden,
        Disabled,
        Unselected,
        Scrolling,
        Dragging,
        HoverSelected,
        HoverUnselected,
        Selected
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