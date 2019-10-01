
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
    public class MpCopyItemTileChooserPanelController {
        private MpCopyItemTileChooserPanel _copyItemTileChooserPanel { get; set; }
        public MpCopyItemTileChooserPanel CopyItemTileChooserPanel { get { return _copyItemTileChooserPanel; } set { _copyItemTileChooserPanel = value; } }

        private List<MpCopyItemTileController> _copyItemTileControllerList { get; set; }
        public List<MpCopyItemTileController> CopyItemTileControllerList { get { return _copyItemTileControllerList; } set { _copyItemTileControllerList = value; } }

        private Color _tileColor1 { get; set; }
        public Color TileColor1 { get { return _tileColor1; } set { _tileColor1 = value; } }

        private Color _tileColor2 { get; set; }
        public Color TileColor2 { get { return _tileColor2; } set { _tileColor2 = value; } }

        private MpKeyboardHook _leftHook, _rightHook;

        private static int _PanelCount = 0;

        private int _panelId = 0;
        private int _selectedCopyItemIdx = -1;
        private int _scrollAccumulator = 0;
        private Timer _focusTimer;

        public MpCopyItemTileChooserPanelController() {
            _panelId = ++_PanelCount;
            TileColor1 = (Color)MpSingletonController.Instance.GetSetting("LogPanelTileColor1");
            TileColor2 = (Color)MpSingletonController.Instance.GetSetting("LogPanelTileColor2");

            //Process.GetCurrentProcess().Refresh();
            Rectangle sb = MpScreenController.GetScreenBoundsWithMouse();// Screen.FromHandle(Process.GetCurrentProcess().MainWindowHandle).WorkingArea;
            
            int h = (int)((float)sb.Height * (float)MpSingletonController.Instance.GetSetting("LogPanelDefaultHeightRatio"));

            CopyItemTileControllerList = new List<MpCopyItemTileController>();

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
            Console.WriteLine("Min scroll: " + _copyItemTileChooserPanel.AutoScrollMinSize.ToString());

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
            SetActiveTile(_copyItemTileControllerList.Count - 1);
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
            SetActiveTile(_selectedCopyItemIdx - 1);
        }
        private void _leftHook_KeyPressed(object sender,KeyPressedEventArgs e) {
            SetActiveTile(_selectedCopyItemIdx + 1);
        }
        public void ScrollTiles(int deltaX) {
            _scrollAccumulator += deltaX;
            if(Math.Abs(_scrollAccumulator) > 10) {
                int deltaSelectedIdx = _scrollAccumulator > 0 ? 1 : -1;
                SetActiveTile(_selectedCopyItemIdx + deltaSelectedIdx);
                _scrollAccumulator = 0;
                MpSingletonController.Instance.ScrollWheelDelta = 0;
            }            
        }
        private void SetActiveTile(int newIdx) {
            if(_copyItemTileControllerList == null || _copyItemTileControllerList.Count == 0) {
                Console.WriteLine("Warning, attempting to set active panel to an empty panel set");
                return;
            }
            if(newIdx < 0) {
                newIdx = _copyItemTileControllerList.Count - 1;
            }
            if(newIdx >= _copyItemTileControllerList.Count) {
                newIdx = 0;
            }

            if(_selectedCopyItemIdx >= 0) {
                _copyItemTileControllerList[_selectedCopyItemIdx].SetFocus(false);
            }
            _copyItemTileControllerList[newIdx].SetFocus(true);
            int pw = _copyItemTileChooserPanel.Width;
            int tr = _copyItemTileControllerList[newIdx].CopyItemTilePanel.Right;
            int tl = _copyItemTileControllerList[newIdx].CopyItemTilePanel.Left;
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
            _selectedCopyItemIdx = newIdx;

            Console.WriteLine("Active tile changed to " + _selectedCopyItemIdx);
        }
        public void OnFormResize(Rectangle newBounds) {
            UpdatePanelBounds(newBounds);
        }

        public void CopyItemCollection_CollectionChanged(object sender,NotifyCollectionChangedEventArgs e) {
            if(e.NewItems != null) {
                foreach(MpCopyItem ci in e.NewItems) {
                    AddNewCopyItemPanel(ci);
                }
            }
        }
        public MpCopyItemTileChooserPanel GetCopyItemPanel() {
            return _copyItemTileChooserPanel;
        }
        public void UpdatePanelBounds(Rectangle newBounds) {
            int p = newBounds.Height - (int)((float)newBounds.Height-((float)MpSingletonController.Instance.GetSetting("LogPanelDefaultTilePadRatio")*(float)newBounds.Height));
            _copyItemTileChooserPanel.Bounds = new Rectangle(0,p,newBounds.Width,newBounds.Height - p);
            foreach(MpCopyItemTileController citc in _copyItemTileControllerList) {
                citc.UpdateTileSize(_copyItemTileChooserPanel.Bounds.Height);
            }
        }
        private void AddNewCopyItemPanel(MpCopyItem ci) {
            //shift older items right by one panel
            foreach(MpCopyItemTileController citc in _copyItemTileControllerList) {
                int p = (int)((float)MpSingletonController.Instance.GetSetting("LogPanelDefaultTilePadRatio") * (float)citc.CopyItemTilePanel.Bounds.Width);
                Point np = new Point(citc.CopyItemTilePanel.Location.X + citc.CopyItemTilePanel.Bounds.Width + (p * 2),citc.CopyItemTilePanel.Location.Y);
                citc.CopyItemTilePanel.Location = np;
            }
            //create new tile
            MpCopyItemTileController ncitc = new MpCopyItemTileController(_copyItemTileChooserPanel.Bounds.Height,ci,_copyItemTileControllerList.Count % 2 == 0 ? TileColor1:TileColor2);
            /* switch(ci.copyItemTypeId) {
                 case MpCopyItemType.Text:
                 //case MpCopyItemType.PhoneNumber:
                 //case MpCopyItemType.StreetAddress:
                 //case MpCopyItemType.WebLink:
                 //case MpCopyItemType.Email:
                 case MpCopyItemType.RichText:
                 case MpCopyItemType.HTMLText:
                     ncitc = new MpCopyItemTileController(h,ci);
                     break;
                 case MpCopyItemType.FileList:
                     ncitc = new MpFileListItemTileController(h,ci);
                     break;
                 case MpCopyItemType.Image:
                     ncitc = new MpImageItemTileController(h,ci);
                     break;
                 default:
                     Console.WriteLine("Unknown copy item type: " + ci.copyItemTypeId);
                     break;
             }*/
            if(ncitc != null) {
                CopyItemTileControllerList.Add(ncitc);
                //ncitc.SetFocus(true);
                CopyItemTileChooserPanel.Controls.Add(ncitc.CopyItemTilePanel);
            }
            
            // UpdatePanelBounds(_copyItemPanel.Bounds);
        }
    }
}
