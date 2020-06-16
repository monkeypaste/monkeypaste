using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpInputManager {
        private IKeyboardMouseEvents _mouseHook;
        private MpKeyboardHook _showMainFormHook,_escHook;

        private bool _isLMouseButtonDown = false;
        private int _yDiffThreshold = 20;
        private Point _lastMouseLoc = Point.Empty;
        private Point _startDragLoc = Point.Empty;
        private bool _canResize = false;
        private bool _isResizing  = false;
        private bool _isScrollingTileContent = false;
        private bool _isScrollingTiles = false;
        private MpTilePanelController _tileUnderMouse = null;
        private MpScrollbarPanelController _scrollBarUnderMouse = null;
        private MpScrollbarGripPanelController _scrollGripUnderMouse = null;

        public MpInputManager() {
            _showMainFormHook = new MpKeyboardHook();
            _showMainFormHook.RegisterHotKey(ModifierKeys.Control, Keys.D);
            _showMainFormHook.KeyPressed += (s, e) => LogFormController.ShowLogForm();

            _mouseHook = Hook.GlobalEvents();
            _mouseHook.MouseClick += (s, e) => {
                if(_tileUnderMouse != null) {
                    TileChooserPanelController.SelectTile(_tileUnderMouse);
                }
            };
            _mouseHook.MouseUp += (s, e) => {
                _isLMouseButtonDown = false;
                _isResizing = false;
                _isScrollingTileContent = false;
                _isScrollingTiles = false;
            };
            _mouseHook.MouseDown += (s, e) => {
                if (!LogFormPanelController.LogFormPanel.Visible) {
                    return;
                }
                if(_canResize) {
                    _isResizing = true;
                }
                else {
                    _isResizing = false;
                }
                if(_tileUnderMouse != null) {
                    Rectangle vScrollBarRect = _tileUnderMouse.TileContentController.ScrollPanelController.VScrollbarPanelController.ScrollbarPanel.RectangleToScreen(_tileUnderMouse.TileContentController.ScrollPanelController.VScrollbarPanelController.ScrollbarPanel.Bounds);
                    Rectangle hScrollBarRect = _tileUnderMouse.TileContentController.ScrollPanelController.HScrollbarPanelController.ScrollbarPanel.RectangleToScreen(_tileUnderMouse.TileContentController.ScrollPanelController.HScrollbarPanelController.ScrollbarPanel.Bounds);

                    Rectangle vScrollGripRect = _tileUnderMouse.TileContentController.ScrollPanelController.VScrollbarPanelController.ScrollbarGripControlController.GripPanel.RectangleToScreen(_tileUnderMouse.TileContentController.ScrollPanelController.VScrollbarPanelController.ScrollbarGripControlController.GripPanel.Bounds);
                    Rectangle hScrollGripRect = _tileUnderMouse.TileContentController.ScrollPanelController.HScrollbarPanelController.ScrollbarGripControlController.GripPanel.RectangleToScreen(_tileUnderMouse.TileContentController.ScrollPanelController.HScrollbarPanelController.ScrollbarGripControlController.GripPanel.Bounds);
                    Rectangle tileChooserRect = TileChooserPanelController.TileChooserPanel.RectangleToScreen(TileChooserPanelController.TileChooserPanel.Bounds);
                    if (vScrollGripRect.Contains(e.Location)) {
                        _startDragLoc = e.Location;
                        _isScrollingTileContent = true;
                        _scrollGripUnderMouse = _tileUnderMouse.TileContentController.ScrollPanelController.VScrollbarPanelController.ScrollbarGripControlController;
                        _scrollGripUnderMouse.MouseDown();
                    } else if (hScrollGripRect.Contains(e.Location)) {
                        _startDragLoc = e.Location;
                        _isScrollingTileContent = true;
                        _scrollGripUnderMouse = _tileUnderMouse.TileContentController.ScrollPanelController.HScrollbarPanelController.ScrollbarGripControlController;
                        _scrollGripUnderMouse.MouseDown();
                    } else if(tileChooserRect.Contains(e.Location)) {
                        _startDragLoc = e.Location;
                        _isScrollingTiles = true;
                    }
                }
            };
            _mouseHook.MouseMove += (s, e) => {
                if(!LogFormPanelController.LogFormPanel.Visible) {
                    Rectangle sb = MpSingleton.Instance.ScreenManager.GetScreenWorkingAreaWithMouse();
                    Rectangle hr = new Rectangle(0, 0, sb.Width, Properties.Settings.Default.ShowLogHotRegionSize);
                    if (e.Location.Y < 5) {
                        LogFormController.ShowLogForm();
                    }
                }
                else {
                    if(_escHook == null) {
                        _escHook = new MpKeyboardHook();
                        _escHook.RegisterHotKey(ModifierKeys.None, Keys.Escape);
                        _escHook.KeyPressed += (s1, e1) => {
                            LogFormController.HideLogForm();
                            _escHook.UnregisterHotKey();
                            _escHook.Dispose();
                            _escHook = null;
                        };
                    }
                    if(_isLMouseButtonDown) {
                        if(_isResizing) {
                            //log form panel rect
                            Rectangle lfpr = LogFormPanelController.LogFormPanel.Bounds;
                            int yDiff = -(e.Location.Y - lfpr.Y);
                            if (Math.Abs(yDiff) >= _yDiffThreshold) {
                                if (LogFormPanelController.CustomHeight < LogFormPanelController.MinimumHeight) {
                                    LogFormPanelController.CustomHeight = lfpr.Height;
                                }
                                LogFormPanelController.CustomHeight += yDiff;
                                LogFormPanelController.Update();
                            }
                        } else if(_isScrollingTileContent) {
                            int lastOffset = _scrollGripUnderMouse.Offset;
                            if(_scrollGripUnderMouse.IsHorizontal) {
                                int dx = e.Location.X - _startDragLoc.X;
                                _scrollGripUnderMouse.Offset += dx;
                            } else {
                                int dy = e.Location.Y - _startDragLoc.Y;
                                _scrollGripUnderMouse.Offset += dy;
                            }
                            int scrollAmount = _scrollGripUnderMouse.Offset - lastOffset;
                            Point offset = _tileUnderMouse.TileContentController.ScrollPanelController.TileContentControlController.Offset;
                            if(_scrollGripUnderMouse.IsHorizontal) {
                                offset.X += -scrollAmount;
                            } else {
                                offset.Y += -scrollAmount;
                            }
                            _tileUnderMouse.TileContentController.ScrollPanelController.TileContentControlController.Offset = offset;
                        }
                    }
                    else {
                        _tileUnderMouse = null;
                        Point mp = LogFormPanelController.LogFormPanel.PointToClient(e.Location);
                        if (Math.Abs(mp.Y) < 20) {
                            Cursor.Current = Cursors.SizeNS;
                            _canResize = true;
                        }
                        else {
                            Cursor.Current = Cursors.Default;
                            _canResize = false;
                            foreach (MpTilePanelController citc in TileChooserPanelController.TileControllerList) {
                                Rectangle tileRect = citc.TilePanel.RectangleToScreen(citc.TilePanel.ClientRectangle);
                                if (tileRect.Contains(e.Location)) {
                                    _tileUnderMouse = citc;
                                    bool isMouseMoving = false;
                                    if(e.Location.X != _lastMouseLoc.X || e.Location.Y != _lastMouseLoc.Y) {
                                        isMouseMoving = true;
                                    }
                                    bool isMouseOverScrollBars = false;
                                    Rectangle vScrollRect = citc.TileContentController.ScrollPanelController.VScrollbarPanelController.ScrollbarPanel.RectangleToScreen(citc.TileContentController.ScrollPanelController.VScrollbarPanelController.ScrollbarPanel.Bounds);
                                    Rectangle hScrollRect = citc.TileContentController.ScrollPanelController.HScrollbarPanelController.ScrollbarPanel.RectangleToScreen(citc.TileContentController.ScrollPanelController.HScrollbarPanelController.ScrollbarPanel.Bounds);
                                    if (vScrollRect.Contains(e.Location)) {
                                        isMouseOverScrollBars = true;
                                        _scrollBarUnderMouse = citc.TileContentController.ScrollPanelController.VScrollbarPanelController;                                        
                                    } else if (hScrollRect.Contains(e.Location)) {
                                        isMouseOverScrollBars = true;
                                        _scrollBarUnderMouse = citc.TileContentController.ScrollPanelController.HScrollbarPanelController;
                                    } else {
                                        _scrollBarUnderMouse = null;
                                    }
                                    if(_scrollBarUnderMouse != null) {
                                        Rectangle gripRect = _scrollBarUnderMouse.ScrollbarGripControlController.GripPanel.RectangleToScreen(_scrollBarUnderMouse.ScrollbarGripControlController.GripPanel.Bounds);
                                        if(gripRect.Contains(e.Location)) {
                                            _scrollGripUnderMouse = _scrollBarUnderMouse.ScrollbarGripControlController;
                                            _scrollGripUnderMouse.MouseHover();
                                        } else {
                                            _scrollGripUnderMouse = null;
                                        }
                                    }
                                    citc.Hover(isMouseMoving,isMouseOverScrollBars);
                                }
                                else {
                                    citc.Focus(citc.IsFocused);
                                }
                            }
                        }
                    }
                }
                _lastMouseLoc = e.Location;
            };
        }
        private MpLogFormController LogFormController {
            get {
                return MpApplication.Instance.TaskbarController.LogFormController;
            }
        }
        private MpLogFormPanelController LogFormPanelController {
            get {
                return MpApplication.Instance.TaskbarController.LogFormController.LogFormPanelController;
            }
        }
        private MpTileChooserPanelController TileChooserPanelController {
            get {
                return MpApplication.Instance.TaskbarController.LogFormController.LogFormPanelController.TileChooserPanelController;
            }
        }
    }
}
