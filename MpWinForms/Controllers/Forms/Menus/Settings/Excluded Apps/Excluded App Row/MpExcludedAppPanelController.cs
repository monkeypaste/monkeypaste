using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {

    public class MpExcludedAppPanelController : MpController,IDisposable {
        public Panel ExcludedAppPanel { get; set; }
        public MpApp ExcludedApp { get; set; }
        
        public MpExcludedAppLabelController ExcludedAppLabelController { get; set; }

        public MpExcludedAppButtonController ExcludedAppButtonController { get; set; }
        
        public bool IsNew { get; set; } = false;

        private Color _ExcludedAppColor;
        
        public MpExcludedAppPanelController(MpController parentController,MpApp excludedApp, Color ExcludedAppColor) : base(parentController) {
            ExcludedApp = excludedApp;
            IsNew = ExcludedApp == null;
            _ExcludedAppColor = ExcludedAppColor;
            Init();            
        }
        private void Init() {
            ExcludedAppPanel = new Panel() {
                AutoSize = false,
                //Radius = 5,
                //BorderThickness = 0,
                BackColor = Color.Chartreuse,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            ExcludedAppPanel.Click += ExcludedAppPanel_Click;
            
            _ExcludedAppColor = ExcludedAppPanel.BackColor;
            
            if(IsNew) {
                ExcludedAppLabelController = new MpExcludedAppLabelController(this, "Add New App", ExcludedAppPanel.BackColor);

            } else {
                ExcludedAppLabelController = new MpExcludedAppLabelController(this, ExcludedApp.GetAppName(), ExcludedAppPanel.BackColor);

            }

            ExcludedAppPanel.Controls.Add(ExcludedAppLabelController.ExcludedAppLabel);
            
            ExcludedAppButtonController = new MpExcludedAppButtonController(this,IsNew);

            ExcludedAppPanel.Controls.Add(ExcludedAppButtonController.ExcludedAppButton);
            ExcludedAppButtonController.ButtonClickedEvent += ExcludedAppButton_ButtonClickedEvent;
            ExcludedAppButtonController.ExcludedAppButton.Click += ExcludedAppPanel_Click;
            ExcludedAppButtonController.ExcludedAppButton.Visible = false;
        }

        private void ExcludedAppPanel_Click(object sender,EventArgs e) {
            if(e.GetType() == typeof(MouseEventArgs)) {
                //for right clicks always show delete context menu 
                if(((MouseEventArgs)e).Button == MouseButtons.Right) {
                    Console.WriteLine("Right mouse clicked on ExcludedApp: " + ExcludedApp.AppPath);
                }
            } 
        }
        

           public override void Update() {
            //tile token chooser panel rect
            Rectangle ttcpr = ((MpExcludedAppListPanelController)Find("MpExcludedAppListPanelController")).ExcludedAppListPanel.Bounds;
            int thisExcludedAppIdx = ((MpExcludedAppListPanelController)Find("MpExcludedAppListPanelController")).ExcludedAppPanelControllerList.IndexOf(this);
            if(thisExcludedAppIdx < 0) {
                return;
            }
            //previous ExcludedApp rect
            Rectangle ptr = thisExcludedAppIdx == 0 ? Rectangle.Empty:((MpExcludedAppListPanelController)Find("MpExcludedAppListController")).ExcludedAppPanelControllerList[thisExcludedAppIdx-1].ExcludedAppPanel.Bounds;

            //token panel height
            float tph = (float)ttcpr.Height*Properties.Settings.Default.TagPanelHeightRatio;
            //token chooser pad
            int tcp = ttcpr.Height - (int)(tph);
            Font f = new Font(Properties.Settings.Default.TagFont,(float)ttcpr.Height-(float)(tcp*1.0f),GraphicsUnit.Pixel);

            //text size
            Size ts = TextRenderer.MeasureText(ExcludedAppLabelController.ExcludedAppLabel.Text,f);

            ExcludedAppPanel.Size = new Size(ts.Width,(int)tph-tcp);
            ExcludedAppPanel.Location = new Point(ptr.Right+tcp,tcp);
            
            ExcludedAppButtonController.Update();
            ExcludedAppLabelController.Update();

            ExcludedAppPanel.Size = new Size(ExcludedAppLabelController.ExcludedAppLabel.Width + (int)tph,ExcludedAppPanel.Height);

            ExcludedAppButtonController.Update(); //LogMenuTileTokenButtonController.LogMenuTileTokenButton.BringToFront();

            ExcludedAppPanel.Invalidate();
        }
        public void CreateExcludedApp() {
            OpenFileDialog pickExcludedAppDialog = new OpenFileDialog()
            {
                Filter = "Exe files (*.exe)|*.exe",
                Title = "Select App to Exclude"
            };
            DialogResult pickExcludedAppResult = pickExcludedAppDialog.ShowDialog();
            if (pickExcludedAppResult == DialogResult.OK) {
                string newExcludedAppFileName = pickExcludedAppDialog.FileName;

                bool isDuplicate = false;
                foreach (MpExcludedAppPanelController ttpc in ((MpExcludedAppListPanelController)Find("MpExcludedAppListController")).ExcludedAppPanelControllerList) {
                    if (ttpc.ExcludedApp.AppPath.ToLower() == newExcludedAppFileName.ToLower()) {
                        isDuplicate = true;
                    }
                }
                if (isDuplicate) {
                    Console.WriteLine("MpExcludedAppPanelController Warning, add invalidation to ui for duplicate/empty ExcludedApp in CreateToken()");
                    return;
                }
                ExcludedApp = new MpApp(newExcludedAppFileName,true);
                ExcludedAppLabelController.ExcludedAppLabel.Text = ExcludedApp.GetAppName();
                ExcludedApp.WriteToDatabase();

                ExcludedAppLabelController.ExcludedAppLabel.Visible = false;
                ExcludedAppLabelController.ExcludedAppLabel.Visible = true;

                ExcludedAppButtonController.ExcludedAppButton.Visible = true;
                ExcludedAppButtonController.ExcludedAppButton.Image = Properties.Resources.close2;
                ExcludedAppButtonController.ExcludedAppButton.DefaultImage = Properties.Resources.close2;
                ExcludedAppButtonController.ExcludedAppButton.OverImage = Properties.Resources.close;
                ExcludedAppButtonController.ExcludedAppButton.DownImage = Properties.Resources.close;
                //((MpExcludedAppListController)Find("MpExcludedAppListController")).AddExcludedAppLabelController.AddExcludedAppLabel.Visible = true;
                //((MpExcludedAppListController)Find("MpExcludedAppListController")).AddExcludedAppLabelController.AddExcludedAppLabel.Text = string.Empty;
                //((MpExcludedAppListController)Find("MpExcludedAppListController")).AddExcludedAppLabelController.AddExcludedAppLabel.Focus();                

                ((MpExcludedAppListPanelController)Find("MpExcludedAppListController")).Update();
            }

            IsNew = false;
            Update();
        }
        public void RemoveExcludedApp() {
            if(IsNew) {
                return;
            }

            ((MpExcludedAppListPanelController)Find("MpExcludedAppListController")).RemoveExcludedApp(this);
        }
        private void ExcludedAppButton_ButtonClickedEvent(object sender,EventArgs e) {
            if(IsNew) {
                CreateExcludedApp();
            }
            else {
                RemoveExcludedApp();
            }
            ((MpExcludedAppListPanelController)Find("MpExcludedAppListController")).Update();
        }
        public void Dispose() {
            ExcludedAppPanel.Visible = false;
            ((MpExcludedAppListPanelController)Find("MpExcludedAppListController")).ExcludedAppPanelControllerList.Remove(this);
            ((MpExcludedAppListPanelController)Find("MpExcludedAppListController")).ExcludedAppListPanel.Controls.Remove(ExcludedAppPanel);
            
            ExcludedAppPanel.Dispose();
        }
    }
}
