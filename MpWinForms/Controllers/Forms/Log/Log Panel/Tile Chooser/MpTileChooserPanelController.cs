using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using VisualEffects.Animations.Effects;
using VisualEffects.Easing;

namespace MonkeyPaste {
    public class MpTileChooserPanelController : MpController,INotifyPropertyChanged    {
        public ObservableCollection<MpTilePanelController> TileControllerList { get; set; } = new ObservableCollection<MpTilePanelController>();

        public Panel TileChooserPanel { get; set; }

        private MpTilePanelController _selectedTilePanelController = null;
        public MpTilePanelController SelectedTilePanelController {
            get {
                return _selectedTilePanelController;
            }
            set {
                if (_selectedTilePanelController != value) {
                    _selectedTilePanelController = value;
                }
            }
        }

        public ObservableCollection<MpTilePanelController> FocusedTilePanelControllerList = new ObservableCollection<MpTilePanelController>();
        
        private MpTileChooserPanelStateType _tileChooserStateType = MpTileChooserPanelStateType.None;
        public MpTileChooserPanelStateType TileChooserStateType {
            get {
                return _tileChooserStateType;
            }
        }

        public delegate void StateChanged(object sender, MpTileChooserPanelStateChangedEventArgs e);
        public event StateChanged StateChangedEvent;

        public event PropertyChangedEventHandler PropertyChanged {
            add {
                ((INotifyPropertyChanged)FocusedTilePanelControllerList).PropertyChanged += value;
            }

            remove {
                ((INotifyPropertyChanged)FocusedTilePanelControllerList).PropertyChanged -= value;
            }
        }

        private bool _isDragging = false;
        private Point _lastDragLoc = Point.Empty;
        private int _offsetX = 0;

        private bool _isInitialLoad = true;
        private bool _isShiftDown = false;
        private bool _isControlDown = false;

        private IKeyboardMouseEvents _mouseHook;
       
        public MpTileChooserPanelController(MpController Parent) : base(Parent) {
            TileChooserPanel = new Panel() {
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                BackColor = Properties.Settings.Default.TileChooserBgColor,
                Bounds = GetBounds(),
                AutoSize = false
            };
            TileChooserPanel.DoubleBuffered(true);

            DefineEvents();
        }
        
        public override void DefineEvents() {
            TileChooserPanel.MouseWheel += (s, e) => {
                _offsetX += (int)((float)e.Delta / 50.0f);
                Update();
            };

            MpApplication.Instance.DataModel.CopyItemList.CollectionChanged += (s, e) => {
                foreach (MpCopyItem ci in e.NewItems) {
                    AddNewCopyItemPanel(ci);
                }
            };
            ((MpLogFormPanelController)Parent).LogMenuPanelController.LogSubMenuPanelController.TileTagChooserPanelController.PropertyChanged += (s, e) => {
                MpTag selectedTag = ((MpLogFormPanelController)Parent).LogMenuPanelController.LogSubMenuPanelController.TileTagChooserPanelController.SelectedTagPanelController.Tag;
                foreach (MpTilePanelController tpc in TileControllerList) {
                    if(selectedTag.IsLinkedWithCopyItem(tpc.CopyItem)) {
                        tpc.Show();
                    } else {
                        tpc.Hide();
                    }
                }
            };
            TileChooserPanel.VisibleChanged += (s, e) => {
                Console.WriteLine("Visibility chaged");
            };
        }
        public override Rectangle GetBounds() {
            //log form panel rect
            Rectangle lfpr = ((MpLogFormPanelController)Parent).GetBounds();
            //logform pad
            int lfp = (int)(lfpr.Width * Properties.Settings.Default.LogPadRatio);
            //logformmenu height
            int lfmh = ((MpLogFormPanelController)Parent).LogMenuPanelController.LogMenuPanel.Bounds.Height;
            //tile chooser offset 
            int tco = ((MpTreeViewPanelController)Find(typeof(MpTreeViewPanelController))).TreeViewPanel.Right;

            return new Rectangle(lfp + tco + _offsetX, lfp + lfmh, lfpr.Width - (lfp * 2), lfpr.Height - lfmh - (lfp * 2));
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
                ((MpLogFormPanelController)Parent).LogMenuPanelController.LogSubMenuPanelController.TileTagChooserPanelController.GetHistoryTagPanelController().Tag.LinkWithCopyItem(ci);
                
                MpTilePanelController newTileController = new MpTilePanelController(ci, this);
                newTileController.TileContentController.ScrollPanelController.VScrollbarPanelController.ScrollStartEvent += (s, e) => {
                    SelectTile(newTileController);
                    SetState(MpTileChooserPanelStateType.Scrolling);
                };
                newTileController.TileContentController.ScrollPanelController.HScrollbarPanelController.ScrollStartEvent += (s, e) => {
                    SelectTile(newTileController);
                    SetState(MpTileChooserPanelStateType.Scrolling);
                };
                newTileController.TileContentController.ScrollPanelController.VScrollbarPanelController.ScrollEndEvent += (s, e) => {
                    SetState(MpTileChooserPanelStateType.None);
                };
                newTileController.TileContentController.ScrollPanelController.HScrollbarPanelController.ScrollEndEvent += (s, e) => {
                    SetState(MpTileChooserPanelStateType.None);
                };
                newTileController.TileContentController.ScrollPanelController.TileContentControlController.TileContentControl.MouseDown += (s, e) => {
                    //tile chooser rect
                    Rectangle tcr = TileChooserPanel.RectangleToScreen(TileChooserPanel.Bounds);
                    if (TileChooserPanel.Bounds.Contains(e.Location)) {
                        _isDragging = true;
                        _lastDragLoc = e.Location;
                    }
                };
                newTileController.TileContentController.ScrollPanelController.TileContentControlController.TileContentControl.MouseMove += (s, e) => {
                    if (_isDragging) {
                        _offsetX += (e.Location.X - _lastDragLoc.X);
                        if (_offsetX > 0) {
                            _offsetX = 0;
                        }
                        Update();
                        _lastDragLoc = e.Location;
                    }
                };
                newTileController.TileContentController.ScrollPanelController.TileContentControlController.TileContentControl.MouseUp += (s, e) => {
                    _isDragging = false;
                };
                var childControls = newTileController.TilePanel.GetAll<Control>();
                foreach (Control c in childControls) {
                    c.MouseEnter += (s, e) => newTileController.Hover();
                    c.MouseLeave += (s, e) => newTileController.Focus(newTileController.IsFocused);
                    c.MouseClick += (s, e) => SelectTile(newTileController);
                }
                newTileController.TileContentController.ScrollPanelController.ShowScrollbars();
                TileControllerList.Insert(0,newTileController);
                TileChooserPanel.Controls.Add(newTileController.TilePanel);
                
                SelectTile(newTileController);
            }
            //Sort("CopyDateTime", false);
        }
        public override void Update() {
            _isInitialLoad = false;
            TileChooserPanel.Bounds = GetBounds();

            foreach(MpTilePanelController citc in TileControllerList) {
                citc.Update();
            }

            TileChooserPanel.Invalidate();
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
        private void SetState(MpTileChooserPanelStateType newState) {            
            _tileChooserStateType = newState;
            if(StateChangedEvent != null) {
                StateChangedEvent(this, new MpTileChooserPanelStateChangedEventArgs(_tileChooserStateType));
            }
            Update();
        }
        public bool ProcessKeyInput(Keys keyData) {
             if (keyData == Keys.Enter || keyData == Keys.Return) {
                Console.WriteLine("Enter key pressed");
                if (SelectedTilePanelController != null) {
                    ((MpLogFormController)Parent.Parent).HideLogForm();
                    MpApplication.Instance.DataModel.ClipboardManager.PasteCopyItem(SelectedTilePanelController.CopyItem);
                }
                return true;
            } else if (keyData == Keys.Tab) {
                Console.WriteLine("Tab key pressed");
                SelectNextTile();
                return true;
            } else if (keyData == (Keys.Shift | Keys.Tab)) {
                Console.WriteLine("Tab shift pressed");
                SelectPreviousTile();
                return true;
            }
            return false;
        }
        public void SelectNextTile() {
            SelectTile(GetNextTilePanelController(SelectedTilePanelController));
            //Update();
        }
        public void SelectPreviousTile() {
            SelectTile(GetPreviousTilePanelController(SelectedTilePanelController));
           // Update();
        }
        public void SelectTile(MpTilePanelController tpc,bool appendSelection = false) {
            //ignore if same tile
            if(tpc == SelectedTilePanelController) {
                return;
            }
            //unselect previous tile
            if(SelectedTilePanelController != null) {
                SelectedTilePanelController.Focus(false);
            }

            SelectedTilePanelController = tpc;
            SelectedTilePanelController.Focus(true);

            ShowSelectedTilePanelController();
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
                _offsetX += ox;
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
        public void DeleteFocusedTile() {
            //tile to delete
            var ttd = SelectedTilePanelController;
            //new focused tile
            MpTilePanelController nft = null;
            var vtl = GetVisibleTilePanelControllerList();
            int dtpcIdx = vtl.IndexOf(ttd);
            if(ttd == SelectedTilePanelController) {
                if(dtpcIdx == vtl.Count - 1) {
                    if(vtl.Count > 1) {
                        nft = vtl[dtpcIdx - 1];
                    }
                } else {
                    nft = vtl[dtpcIdx + 1];
                }
            }
            TileControllerList.Remove(ttd);
            TileChooserPanel.Controls.Remove(ttd.TilePanel);
            //Sort("CopyItemId",false);
            ttd.CopyItem.DeleteFromDatabase();            
            ttd.TilePanel.Dispose();
            ttd = null;
            
            if(nft != null) {
                SelectTile(nft);
            }
        }
        public override void ActivateHotKeys() {
            
            // _ctrlTabHook, _ctrlShiftTabHook
        }
        public override void DeactivateHotKeys() {
           

            //_ctrlTabHook, _ctrlShiftTabHook,
        }
        public void Sort(string sortBy,bool ascending) {
            if(ascending) {
                TileControllerList.OrderBy(x => MpTypeHelper.GetPropertyValue(x.CopyItem, sortBy));
            } else {
                TileControllerList.OrderByDescending(x => MpTypeHelper.GetPropertyValue(x.CopyItem, sortBy));
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