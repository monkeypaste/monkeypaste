
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
    public class MpCopyItemTileChooserPanelController : MpController {
        private MpCopyItemTileChooserPanel _copyItemTileChooserPanel { get; set; }
        public MpCopyItemTileChooserPanel CopyItemTileChooserPanel { get { return _copyItemTileChooserPanel; } set { _copyItemTileChooserPanel = value; } }

        private List<MpCopyItemTileController> _copyItemTileControllerList { get; set; }
        public List<MpCopyItemTileController> CopyItemTileControllerList { get { return _copyItemTileControllerList; } set { _copyItemTileControllerList = value; } }

        private Color _tileColor1 { get; set; }
        public Color TileColor1 { get { return _tileColor1; } set { _tileColor1 = value; } }

        private Color _tileColor2 { get; set; }
        public Color TileColor2 { get { return _tileColor2; } set { _tileColor2 = value; } }
        
        private MpCopyItemTileController _selectedCopyItemTileController {
            get {
                if(SelectedCopyItemIdx >= 0) {
                    return CopyItemTileControllerList[SelectedCopyItemIdx];
                }
                return null;
            }
            set {
                SelectedCopyItemIdx = GetCopyItemTileIdx(value.TileId);
            }
        }
        public MpCopyItemTileController SelectedCopyItemTileController {
            get {
                return _selectedCopyItemTileController;
            }
            set {
                _selectedCopyItemTileController = value;
            }
        }

        private int _selectedCopyItemIdx { get; set; }
        public int SelectedCopyItemIdx {
            get {
                return _selectedCopyItemIdx;
            }
            set {
                if(_selectedCopyItemIdx == value) {
                    Console.WriteLine("Warning, selecting same copy item");
                    return;
                }
                if(_copyItemTileControllerList == null || _copyItemTileControllerList.Count == 0) {
                    Console.WriteLine("Warning, attempting to set active panel to an empty panel set");
                    return;
                }
                if(value < 0) {
                    value = _copyItemTileControllerList.Count - 1;
                }
                if(value >= _copyItemTileControllerList.Count) {
                    value = 0;
                }

                /*if(_selectedCopyItemIdx >= 0) {
                    if(_copyItemTileControllerList[_selectedCopyItemIdx].IsEditable) {
                        ((TextBox)_copyItemTileControllerList[_selectedCopyItemIdx].CopyItemControlController.ItemControl).ReadOnly = true;
                    }
                    //_copyItemTileControllerList[_selectedCopyItemIdx].HasFocus = false;
                }
                if(_copyItemTileControllerList[value].IsEditable) {
                    ((TextBox)_copyItemTileControllerList[value].CopyItemControlController.ItemControl).ReadOnly = true;
                }*/
                //_copyItemTileControllerList[value].HasFocus = true;
                int pw = _copyItemTileChooserPanel.Width;
                int tr = _copyItemTileControllerList[value].CopyItemTilePanel.Right;
                int tl = _copyItemTileControllerList[value].CopyItemTilePanel.Left;
                int ox = 0;
                if(tr > pw) {
                    ox = -(tr - pw);
                }
                else if(tl < 0) {
                    ox = -tl;
                }
                if(ox != 0) {
                    foreach(MpCopyItemTileController citc in _copyItemTileControllerList) {
                        Point l = citc.CopyItemTilePanel.Location;
                        l.X += ox;
                        citc.CopyItemTilePanel.Location = l;
                    }
                }
                _selectedCopyItemIdx = value;

                Console.WriteLine("Active tile changed to " + _selectedCopyItemIdx);
            }
        }

        private MpKeyboardHook _leftHook, _rightHook;

        private static int _PanelCount = 0;

        private int _panelId = 0;
        private int _scrollAccumulator = 0;
        private Timer _focusTimer;

        public MpCopyItemTileChooserPanelController(MpController parentController) : base(parentController) {
            CopyItemTileControllerList = new List<MpCopyItemTileController>();

            _selectedCopyItemIdx = -1;
            _panelId = ++_PanelCount;

            TileColor1 = (Color)MpSingletonController.Instance.GetSetting("TileColor1");
            TileColor2 = (Color)MpSingletonController.Instance.GetSetting("TileColor2");

            //Process.GetCurrentProcess().Refresh();
            Rectangle sb = MpHelperSingleton.Instance.GetScreenBoundsWithMouse();// Screen.FromHandle(Process.GetCurrentProcess().MainWindowHandle).WorkingArea;            
            int h = (int)((float)sb.Height * (float)MpSingletonController.Instance.GetSetting("LogScreenHeightRatio"));            

            CopyItemTileChooserPanel = new MpCopyItemTileChooserPanel() {
                BackColor = (Color)MpSingletonController.Instance.GetSetting("LogPanelBgColor"),
                AutoSize = true,
                Bounds = new Rectangle(0,sb.Height - h,sb.Width,h), //new Rectangle(p,sb.Height - h + p,s,s);,
                //Padding = new Padding(p)
            };
            CopyItemTileChooserPanel.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;

            _focusTimer = new Timer();
            _focusTimer.Interval = 10;
            _focusTimer.Tick += _focusTimer_Tick;
            _focusTimer.Start();

            MpSingletonController.Instance.GetMpData().AddOnDataListChangeListener(this);

            foreach(MpCopyItem ci in MpSingletonController.Instance.GetMpData().GetMpCopyItemList()) {
                AddNewCopyItemPanel(ci);
            }
        }

        private void _focusTimer_Tick(object sender,EventArgs e) {
            ScrollTiles(MpSingletonController.Instance.ScrollWheelDelta);
        }

        public MpCopyItem GetSelectedCopyItem() {
            if(_selectedCopyItemIdx >= 0) {
                return _copyItemTileControllerList[_selectedCopyItemIdx].CopyItemControlController.CopyItem;
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

            _copyItemTileChooserPanel.Focus();
            SelectedCopyItemIdx = CopyItemTileControllerList.Count - 1;
            //SetActiveTile(_copyItemTileControllerList.Count - 1);
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
            /*if(_copyItemTileControllerList.Count > 0 && _selectedCopyItemIdx >= 0) {
                _copyItemTileControllerList[_selectedCopyItemIdx].SetFocus(false);
            }
            SetActivePanel(_copyItemTileControllerList.Count - 1);*/
        }        
        private void _rightHook_KeyPressed(object sender,KeyPressedEventArgs e) {
            SelectedCopyItemIdx -= 1;
           // SetActiveTile(_selectedCopyItemIdx - 1);
        }
        private void _leftHook_KeyPressed(object sender,KeyPressedEventArgs e) {
            SelectedCopyItemIdx++;
           // SetActiveTile(_selectedCopyItemIdx + 1);
        }
        public void ScrollTiles(int deltaX) {
            _scrollAccumulator += deltaX;
            if(Math.Abs(_scrollAccumulator) > 10) {
                int deltaSelectedIdx = _scrollAccumulator > 0 ? 1 : -1;
                SelectedCopyItemIdx += deltaSelectedIdx;
                //SetActiveTile(_selectedCopyItemIdx + deltaSelectedIdx);
                _scrollAccumulator = 0;
                MpSingletonController.Instance.ScrollWheelDelta = 0;
            }            
        }
        public void OnFormResize(Rectangle newBounds) {
            UpdatePanelBounds(newBounds);
            CopyItemTileChooserPanel.Refresh();
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
                List<MpCopyItemTileController> toRemove = new List<MpCopyItemTileController>();
                foreach(MpCopyItem ci in e.OldItems) {
                    foreach(MpCopyItemTileController citc in CopyItemTileControllerList) {
                        if(citc.CopyItem == ci) {
                            toRemove.Add(citc);
                        }
                    }                    
                }
                foreach(MpCopyItemTileController toRemoveCitc in toRemove) {
                    CopyItemTileControllerList.Remove(toRemoveCitc);
                }
            }
        }
        public MpCopyItemTileChooserPanel GetCopyItemPanel() {
            return _copyItemTileChooserPanel;
        }
        public int GetCopyItemTileIdx(int copyItemTileId) {
            for(int i = 0;i < CopyItemTileControllerList.Count;i++) {
                if(CopyItemTileControllerList[i].TileId == copyItemTileId) {
                    return i;
                }
            }
            return -1;
        }
        public void UpdatePanelBounds(Rectangle newBounds) {
            int p = newBounds.Height - (int)((float)newBounds.Height-((float)MpSingletonController.Instance.GetSetting("TileOuterPadScreenWidthRatio")*(float)newBounds.Height));
            _copyItemTileChooserPanel.Bounds = new Rectangle(0,p,newBounds.Width,newBounds.Height - p);
            foreach(MpCopyItemTileController citc in _copyItemTileControllerList) {
                citc.UpdateBounds();
            }
        }
        private void AddNewCopyItemPanel(MpCopyItem ci) {
            //shift older items right by one panel
            foreach(MpCopyItemTileController citc in _copyItemTileControllerList) {
                //get tile size offset based of logpanel size times tile ratio
                int p = (int)((float)MpSingletonController.Instance.GetSetting("TileOuterPadScreenWidthRatio") * (float)citc.CopyItemTilePanel.Bounds.Width);
                Point np = new Point(citc.CopyItemTilePanel.Location.X + citc.CopyItemTilePanel.Bounds.Width + (p * 2),citc.CopyItemTilePanel.Location.Y);
                citc.CopyItemTilePanel.Location = np;
                //citc.HasFocus = false;
            }
            //create new tile
            MpCopyItemTileController ncitc = new MpCopyItemTileController(
                _copyItemTileChooserPanel.Bounds.Height,
                ci,_copyItemTileControllerList.Count % 2 == 0 ? TileColor1:TileColor2,
                this
            );

            if(ncitc != null) {
                /*ncitc.CopyItemTilePanel.MouseClick += OnTileClick;
                ncitc.CopyItemTilePanel.MouseDoubleClick += OnTileDoubleClick;
                ncitc.CopyItemControlController.ItemControl.MouseClick += OnTileClick;
                ncitc.CopyItemControlController.ItemControl.MouseDoubleClick += OnTileDoubleClick;
                ncitc.CopyItemTileTitlePanelController.CopyItemTileTitlePanel.MouseClick += OnTileClick;
                ncitc.CopyItemTileTitlePanelController.CopyItemTileTitlePanel.MouseDoubleClick += OnTileDoubleClick;
                ncitc.CopyItemTileTitlePanelController.CopyItemTileTitleIconPanelController.CopyItemTileTitleIconPanel.MouseClick += OnTileClick;
                ncitc.CopyItemTileTitlePanelController.CopyItemTileTitleIconPanelController.CopyItemTileTitleIconPanel.MouseDoubleClick += OnTileDoubleClick;
                ncitc.CopyItemTileTitlePanelController.CopyItemTileTitleIconPanelController.CopyItemTitleIconBox.MouseClick += OnTileClick;*/

                //ncitc.HasFocus = true;
                CopyItemTileControllerList.Add(ncitc);
                //ncitc.SetFocus(true);
                CopyItemTileChooserPanel.Controls.Add(ncitc.CopyItemTilePanel);
            }
            
            UpdatePanelBounds(_copyItemTileChooserPanel.Bounds);
        }

        private void OnTileClick(object sender,MouseEventArgs e) {
            //At this level if anywhere of the tile is clicked it becomes selected
            MpCopyItemTileController clickedTileController = null;
            foreach(MpCopyItemTileController citc in CopyItemTileControllerList) {
                if(citc.CopyItemTilePanel.RectangleToScreen(citc.CopyItemTilePanel.ClientRectangle).Contains(e.Location)) {
                    clickedTileController = citc;
                }
            }
            if(clickedTileController != null) {
                SelectedCopyItemTileController = clickedTileController;
            }
        }
        private void OnTileDoubleClick(object sender,MouseEventArgs e) {
            //if a tile is doubleclicked it is automatically pasted and select3ed as a side effect
            MpCopyItemTileController clickedTileController = null;
            foreach(MpCopyItemTileController citc in CopyItemTileControllerList) {
                if(citc.CopyItemTilePanel.RectangleToScreen(citc.CopyItemTilePanel.ClientRectangle).Contains(e.Location)) {
                    clickedTileController = citc;
                }
            }
            if(clickedTileController != null) {
                SelectedCopyItemTileController = clickedTileController;
                ((MpLogFormController)ParentController).PasteCopyItem();
            }
        }

        public override void UpdateBounds() {
            throw new NotImplementedException();
        }

        protected override void View_KeyPress(object sender,KeyPressEventArgs e) {
            base.View_KeyPress(sender,e);
        }
    }
}
