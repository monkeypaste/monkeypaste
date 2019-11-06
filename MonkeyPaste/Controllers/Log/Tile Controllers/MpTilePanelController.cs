using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;
using System.Windows.Forms.Integration;

namespace MonkeyPaste {
    public class MpTilePanelController : MpController {     
        //public static int TotalTileCount = 0;

        public int TileId { get; set; } = -1;
        public int SortOrder { get; set; } = 0;
        public MpTilePanel TilePanel { get; set; }
        public MpRoundedPanel BorderPanel { get; set; }

        public MpTileControlController TileControlController { get; set; }
        public MpTileTitlePanelController TileTitlePanelController { get; set; }
        public MpTileMenuPanelController TileMenuPanelController { get; set; }
        public MpTileDetailsPanelController TileDetailsPanelController { get; set; }

        public MpGlassyPanel testPanel { get; set; }
        public MpCopyItem CopyItem { get; set; }

        private MpKeyboardHook _escKeyHook,_spaceKeyHook;
        public MpMouseHook MouseOverItemControlHook { get; set; }

        public delegate void ActiveChanged(object sender,bool isActive);
        public event ActiveChanged activeChangedEvent;

        private bool _isFocused = false;

        public MpTilePanelController(int tileId,int panelId,MpCopyItem ci,MpController Parent) : base(Parent) {
            TileId = tileId;
            SortOrder = TileId;
            CopyItem = ci;

            TilePanel = new MpTilePanel(tileId,panelId) {
                AutoScroll = false,
                AutoSize = false,
                BackColor = MpHelperSingleton.Instance.GetRandomColor(),
                Thickness = Properties.Settings.Default.TileBorderThickness,
                Radius = Properties.Settings.Default.TileBorderRadius
            };
            TilePanel.BorderColor = TilePanel.BackColor;
            TilePanel.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;

            TileTitlePanelController = new MpTileTitlePanelController(tileId,panelId,ci,this);
            TilePanel.Controls.Add(TileTitlePanelController.TileTitlePanel);            

            TileDetailsPanelController = new MpTileDetailsPanelController(tileId,panelId,this);
            TilePanel.Controls.Add(TileDetailsPanelController.TileDetailsPanel);
            
            TileControlController = new MpTileControlController(tileId,panelId,ci,this);
            TilePanel.Controls.Add(TileControlController.ItemPanel);

            TileMenuPanelController = new MpTileMenuPanelController(tileId,panelId,this);
            //TilePanel.Controls.Add(TileMenuPanelController.TileMenuPanel);

            //testPanel = new MpGlassyPanel() {
            //   // FormBorderStyle = FormBorderStyle.None,
            //    //TopLevel = false,
            //    //Opacity = 0.5,
            //    BorderStyle = BorderStyle.None,
            //    //BackgroundImage = Properties.Resources.texture,
            //    Opacity = 55,
            //   // BackgroundImageLayout = ImageLayout.Stretch,
            //    BackColor = Color.Green,
                
            //    AutoSize = false
            //};
            //TilePanel.Controls.Add(testPanel);
            //testPanel.BringToFront();

            MouseOverItemControlHook = new MpMouseHook();
            MouseOverItemControlHook.MouseEvent += _mouseOverItemControlHook_MouseEvent;

            //form = new Form1() {
            //    TopMost = true,
            //    TopLevel = true,
            //    FormBorderStyle = FormBorderStyle.None,
            //    Opacity = 0.5,
            //    Bounds = new Rectangle()
            //};
            //((MpLogFormController)((MpTileChooserPanelController)Parent).Parent).LogForm.Controls.Add(form);
           // form.Show();
            Update();
            Link(new List<MpIView> { TilePanel });
        }


        private void _mouseOverItemControlHook_MouseEvent(object sender,Gma.System.MouseKeyHook.MouseEventExtArgs e) {
            //Console.WriteLine("Chuck chatted too much");

        }
        
        public override void Update() {
            if(TilePanel.Visible == false) {
                return;
            }
            //tile chooser panel rect
            Rectangle tcpr = ((MpTileChooserPanelController)Parent).TileChooserPanel.Bounds;
            //tile padding
            int tp = (int)(Properties.Settings.Default.TileChooserPadHeightRatio * tcpr.Height);
           //tile size
            int ts = tcpr.Height - (int)(tp*2);
            //get tile details rect
            Rectangle tdr = TileDetailsPanelController.TileDetailsPanel.Bounds;
            //get tile title rect
            Rectangle ttr = TileTitlePanelController.TileTitlePanel.Bounds;

            //TilePanel.ResumeLayout(false);

            TilePanel.SetBounds(((SortOrder-1)*tcpr.Height+tp),tp,ts,ts);
            
            TileTitlePanelController.Update();
            TileControlController.Update();
            TileDetailsPanelController.Update();
            TileMenuPanelController.Update();
            Rectangle icr = TileControlController.ItemPanel.Bounds;

            TileMenuPanelController.TileMenuPanel.BringToFront();
        }
        public void ShowMenu() {            
            TileMenuPanelController.TileMenuPanel.Visible = true;
            //TileMenuPanelController.Update();
                //TileMenuPanelController.TileMenuButtonControllerList[0].TileMenuButton.ButtonOver();
                //TileMenuPanelController.TileMenuButtonControllerList[0].TileMenuButton.Refresh();
                //TileMenuPanelController.TileMenuPanel.BringToFront();
        }
        public void HideMenu() {
            TileMenuPanelController.TileMenuPanel.Visible = false;
           // TileMenuPanelController.Update();
        }
        public bool IsFocusesd() {
            return _isFocused;
        }
        public void SetFocus(bool isFocused) {
            if(isFocused) {
                TilePanel.BorderColor = Color.White;
               // TileControlController.ItemControl.Enabled = true;

                MouseOverItemControlHook.RegisterMouseEvent(MpMouseEvent.HitBox,TileControlController.ItemControl.RectangleToScreen(TileControlController.ItemControl.Bounds));

                //ShowMenu();
            } else {
                TilePanel.BorderColor = TilePanel.BackColor;//(Color)MpSingletonController.Instance.GetSetting("TileUnfocusColor");
                //TileControlController.ItemControl.Enabled = false;
                MouseOverItemControlHook.UnregisterMouseEvent();
                //HideMenu();
            }
            _isFocused = isFocused;
            //TileTitlePanelController.TileTitlePanel.BorderColor = TilePanel.BorderColor;
        }
        public void ActivateHotKeys() {}      

        public void DeactivateHotKeys() {}

        
    }   
}
