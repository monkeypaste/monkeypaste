using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;
using NonInvasiveKeyboardHookLibrary;

namespace MonkeyPaste {
    public class MpLogFormController : MpController {
        public static bool IsFirstLoad = true;

        public MpResizeLogPanelController ResizeLogPanelController { get; set; }
        public MpLogFormPanelController LogFormPanelController { get; set; }
        public MpDragTilePanelController DragTilePanelController { get; set; }

        public MpResizableBorderlessForm LogForm { get; set; }
   

        public MpLogFormController(MpController Parent) : base(Parent) {
            LogForm = new MpResizableBorderlessForm() {
                AutoSize = false,   
                KeyPreview = true,                
                AutoScaleMode = AutoScaleMode.Dpi,
                Bounds = GetBounds(),
                TransparencyKey = Color.Fuchsia,
                BackColor = Color.Fuchsia,
                ShowInTaskbar = false,
                Icon = MpHelperSingleton.Instance.GetIconFromBitmap( Properties.Resources.monkey3)
            };

            LogFormPanelController = new MpLogFormPanelController(this);
            LogForm.Controls.Add(LogFormPanelController.LogFormPanel);

            ResizeLogPanelController = new MpResizeLogPanelController(this);
            LogForm.Controls.Add(ResizeLogPanelController.ResizePanel);

            DefineEvents();
        }
        public override void Update() {            
            LogForm.Bounds = GetBounds();

            ResizeLogPanelController.Update();
            LogFormPanelController.Update();
            LogForm.Invalidate();
        }
        public override void DefineEvents() {
            LogForm.FormClosing += (sender, e) => {
                HideLogForm();
                e.Cancel = true;
            };
            LogForm.FormClosed += (sender, e) => HideLogForm();
            LogForm.Leave += (sender, e) => HideLogForm();
            LogForm.Deactivate += (sender, e) => HideLogForm();            
            LogForm.KeyUp += (s, e) => {
                //tile chooser panel controller
                var tcpc = LogFormPanelController.TileChooserPanelController;
                //selected tile panel controller
                var stpc = tcpc.SelectedTilePanelController;
                int keyScrollDelta = 20;
                if (e.KeyData == Keys.Enter || e.KeyData == Keys.Return) {
                    if (stpc != null) {
                        HideLogForm();
                        MpApplication.Instance.DataModel.ClipboardManager.PasteCopyItem(stpc.CopyItem);
                    }
                }
                else if (e.KeyData == Keys.Tab) {
                    tcpc.SelectNextTile();
                }
                else if (e.KeyData == (Keys.Shift | Keys.Tab)) {
                    tcpc.SelectPreviousTile();
                }
                else if (e.KeyData == Keys.Left) {
                    stpc.TileContentController.ScrollPanelController.HScrollbarPanelController.PerformScroll(-keyScrollDelta);
                }
                else if (e.KeyData == Keys.Right) {
                    stpc.TileContentController.ScrollPanelController.HScrollbarPanelController.PerformScroll(keyScrollDelta);
                }
                else if (e.KeyData == Keys.Up) {
                    stpc.TileContentController.ScrollPanelController.VScrollbarPanelController.PerformScroll(-keyScrollDelta);
                }
                else if (e.KeyData == Keys.Down) {
                    stpc.TileContentController.ScrollPanelController.VScrollbarPanelController.PerformScroll(keyScrollDelta);
                }
                else if (e.KeyData == Keys.Delete || e.KeyData == Keys.Back) {
                    tcpc.DeleteFocusedTile();
                    tcpc.Update();
                } else if(e.KeyData == Keys.Escape) {
                    HideLogForm();
                }
            };
        }
        public void ShowLogForm() {
            if(IsFirstLoad) {
                Update();
                IsFirstLoad = false;
                HideLogForm();
                return;
            }
            if (LogForm.Visible) {
                return;
            }
            LogForm.Show();
            LogForm.Visible = true;
            LogForm.Activate();
            ActivateHotKeys();
            LogFormPanelController.TileChooserPanelController.ActivateHotKeys();
            //LogFormPanelController.TileChooserPanelController.SelectTile(LogFormPanelController.TileChooserPanelController.GetVisibleTilePanelControllerList()[0]);
        }
        public void HideLogForm() {
            if(IsFirstLoad) {
                return;
            }
            LogForm.Hide();
            LogForm.Visible = false;
            DeactivateHotKeys();
            LogFormPanelController.TileChooserPanelController.DeactivateHotKeys();
        }
        public void CloseLogForm() {
            //DeactivateHotKeys();
            //_clickHook.Dispose();
            //_moveHook.Dispose();
            LogForm.Close();
            LogForm = null;
        }
        public override Rectangle GetBounds() {
           return MpSingleton.Instance.ScreenManager.GetScreenWorkingAreaWithMouse();
        }         
    }
}
