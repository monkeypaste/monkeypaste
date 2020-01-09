using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpGlobalInputManager : IDisposable {
        private static readonly Lazy<MpGlobalInputManager> lazy = new Lazy<MpGlobalInputManager>(() => new MpGlobalInputManager());
        public static MpGlobalInputManager Instance { get { return lazy.Value; } }

        private IKeyboardMouseEvents _hook;
        private IKeyboardMouseEvents _keyHook;

        public delegate void MouseClicked(object sender,MouseEventArgs e);
        public event MouseClicked MouseClickedEvent;

        public delegate void MouseMove(object sender,MouseEventExtArgs e);
        public event MouseMove MouseMoveEvent;

        public delegate void KeyPressed(object sender,KeyPressEventArgs e);
        public event KeyPressed KeyPressedEvent;

        public delegate void MethodHandler();

        private Dictionary<Combination,Action> _assignmentDictionary = new Dictionary<Combination,Action>();

        public Action ActionMouseTopScreenEdge; 
        public Action ActionMouseOverLogFormPanelTop;

        public Action ActionKeyboardToggleLog;
        public Action ActionKeyboardHideLog;

        public Action ActionLogFormResizeBegin;
        public Action ActionLogFormResizing;
        public Action ActionLogFormResizeEnd;

        public Action ActionKeyboardNextControl;
        public Action ActionKeyboardPreviousControl;

        public Action ActionKeyboardPreviousTile;
        public Action ActionKeyboardNextTile;
        
        public Action ActionKeyboardDeleteTile;

        public Action ActionKeyboardPasteTile;
        
        public Action ActionKeyboardToggleAppendMode;
        public Action ActionKeyboardToggleAutoCopyMode;
        public Action ActionKeyboardToggleAppEnabled;

        public Action ActionKeyboardToggleSettings;


        public MpGlobalInputManager() {
            _hook = Hook.GlobalEvents();
            //_hook.MouseClick += delegate (object sender,MouseEventArgs e) {
            //    MouseClickedEvent?.Invoke(sender,e);
            //};
            //_hook.MouseMoveExt += delegate (object sender,MouseEventExtArgs e) {
            //    MouseMoveEvent?.Invoke(sender,e);
            //};
            //_hook.KeyPress += delegate (object sender,KeyPressEventArgs e) {
                
            //    KeyPressedEvent?.Invoke(sender,e);
            //};
        }
        public void AddKeyboardAction(string combination,ref Action action,Action actionHandler) {
            if(!_assignmentDictionary.Contains<KeyValuePair<Combination,Action>>(new KeyValuePair<Combination, Action>(Combination.FromString(combination),action))) {
                action = actionHandler;
                _assignmentDictionary.Add(Combination.FromString(combination),action);
                Hook.GlobalEvents().OnCombination(_assignmentDictionary);
            }          
            //
        }
        public void AddMouseMoveAction(EventHandler<MouseEventExtArgs> actionHandler) {            
            _hook.MouseMoveExt += actionHandler;
        }
        public void AddMouseClickAction(MouseEventHandler actionHandler) {
            _hook.MouseClick += actionHandler;
        }
        public void RemoveKeyboardAction(string combination) {
            if(_assignmentDictionary.ContainsKey(Combination.FromString(combination))) {
                _assignmentDictionary.Remove(Combination.FromString(combination));
                Hook.GlobalEvents().OnCombination(_assignmentDictionary);
            }
            //
        }
        public void RemoveMouseMoveAction(EventHandler<MouseEventExtArgs> actionHandler) {
            _hook.MouseMoveExt -= actionHandler;
        }
        public void RemoveMouseClickAction(MouseEventHandler actionHandler) {
            _hook.MouseClick -= actionHandler;
        }

        private void _keyHook_KeyPressed(object sender,KeyPressEventArgs e) {
            KeyPressedEvent(sender,e);
        }
        private void _mouseHook_MouseClick(object sender,MouseEventArgs e) {
            MouseClickedEvent(sender,e);
            //Console.WriteLine("Clicked");
            //if(LogForm == null || !LogForm.Visible) {
            //    return;
            //}
            //MpTilePanelController clickedTileController = null;
            //foreach(MpTilePanelController citc in TileChooserPanelController.TileControllerList) {
            //    Rectangle tileRect = citc.TilePanel.RectangleToScreen(citc.TilePanel.ClientRectangle);
            //    if(tileRect.Contains(e.Location) || citc.TilePanel.ClientRectangle.Contains(e.Location)) {
            //        clickedTileController = citc;
            //    }
            //}
            //if(clickedTileController != null) {
            //    if(clickedTileController.CopyItem.copyItemTypeId == MpCopyItemType.Text) {
            //        Console.WriteLine(
            //            "TileId: " +
            //            clickedTileController.TileId +
            //            " clicked with dimensions(px): " +
            //            MpHelperSingleton.Instance.GetTextSize(
            //                (string)clickedTileController.CopyItem.GetData(),
            //                clickedTileController.TileControlController.ItemFont
            //            )
            //        );
            //    }
            //    TileChooserPanelController.SelectedTileController = clickedTileController;
            //    TileChooserPanelController.Update();
            //}
        }
        private void _mouseHook_MouseMoveExt(object sender,MouseEventExtArgs e) {
            MouseMoveEvent(sender,e);
            //Console.WriteLine("Mo0ve");
            //if(LogForm == null || !LogForm.Visible) {
            //    return;
            //}
            //foreach(MpTilePanelController citc in TileChooserPanelController.TileControllerList) {
            //    Rectangle itemControlRect = citc.TilePanel.RectangleToScreen(citc.TileControlController.ItemPanel.ClientRectangle);
            //    if(itemControlRect.Contains(e.Location) || citc.TilePanel.ClientRectangle.Contains(e.Location)) {
            //        citc.TileControlController.TraverseItem(citc.TileControlController.ItemPanel.PointToClient(e.Location));
            //    }
            //}
        }

        public void Dispose() {
            _hook.Dispose();
        }
    }
}
