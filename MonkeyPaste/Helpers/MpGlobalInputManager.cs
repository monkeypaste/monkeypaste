using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpGlobalInputManager {
        private static readonly Lazy<MpGlobalInputManager> lazy = new Lazy<MpGlobalInputManager>(() => new MpGlobalInputManager());
        public static MpGlobalInputManager Instance { get { return lazy.Value; } }

        private IKeyboardMouseEvents _mouseHook;
       /* private MpKeyboardHook _keyHook;*/

        public delegate void MouseClicked(object sender,MouseEventArgs e);
        public event MouseClicked MouseClickedEvent;

        public delegate void MouseMove(object sender,MouseEventExtArgs e);
        public event MouseMove MouseMoveEvent;

        public delegate void KeyPressed(object sender,KeyPressedEventArgs e);
        public event KeyPressed KeyPressedEvent;

        private Dictionary<Combination,Action> _assignmentDictionary = new Dictionary<Combination,Action>();

        public Action ActionToggleLogVisibility { get; set; }
        public Action ActionMouseTopScreenEdge { get; set; }
        public Action ActionHideLog { get; set; }
        public Action ActionToggleAppendMode { get; set; }
        public Action ActionSelectLeftTile { get; set; }
        public Action ActionSelectRightTile { get; set; }
        public Action ActionDeleteTile { get; set; }
        public Action ActionPasteTile { get; set; }
        public Action ActionToggleAutoCopyMode { get; set; }
        public Action ActionToggleAppEnabled { get; set; }
        public Action ActionToggleSettings { get; set; }
        
        public MpGlobalInputManager() {
            _mouseHook = Hook.GlobalEvents();

            _mouseHook.MouseClick += delegate (object sender,MouseEventArgs e) {
                MouseClickedEvent?.Invoke(sender,e);
            };
            _mouseHook.MouseMoveExt += delegate (object sender,MouseEventExtArgs e) {
                MouseMoveEvent?.Invoke(sender,e);
            };

            //keyHook = new MpKeyboardHook();
            //_keyHook.KeyPressed += _keyHook_KeyPressed;

            /*var undo = Combination.FromString("Control+Z");
            //var fullScreen = Combination.FromString("Shift+Alt+Enter");
            
            //2. Define actions
            Action actionUndo = DoSomething;
            Action actionFullScreen = () => { Console.WriteLine("You Pressed FULL SCREEN"); };

            void DoSomething() {
                Console.WriteLine("You pressed UNDO");
            }

            //3. Assign actions to key combinations
            var assignment = new Dictionary<Combination,Action> {
                {undo, actionUndo},
                {fullScreen, actionFullScreen}
            };

            //4. Install listener
            Hook.GlobalEvents().OnCombination(assignment);*/
        }
        public void AddKeyboardAction(string combination,Action a) {
            _assignmentDictionary.Add(Combination.FromString(combination),a);
            Hook.GlobalEvents().OnCombination(_assignmentDictionary);
        }
        private void _keyHook_KeyPressed(object sender,KeyPressedEventArgs e) {
            throw new NotImplementedException();
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
    }
}
