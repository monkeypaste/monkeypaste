using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpExcludedAppListPanelController : MpController {
        public Panel ExcludedAppListPanel { get; set; }

        public List<MpExcludedAppPanelController> ExcludedAppPanelControllerList = new List<MpExcludedAppPanelController>();


        public MpExcludedAppListPanelController(MpController pc) : base(pc) {
            ExcludedAppListPanel = new Panel()
            {
                AutoSize = false
            };
            bool altRowColor = false;
            List<MpApp> excludedAppList = (List<MpApp>)MpApplication.Instance.DataModel.Db.GetExcludedAppList();
            foreach(MpApp ea in excludedAppList) {
                MpExcludedAppPanelController neac = new MpExcludedAppPanelController(this, ea,altRowColor ? Properties.Settings.Default.ExcludedAppRowColor2:Properties.Settings.Default.ExcludedAppRowColor1);
                ExcludedAppPanelControllerList.Add(neac);
                ExcludedAppListPanel.Controls.Add(neac.ExcludedAppPanel);
                altRowColor = !altRowColor;
            }
            AddEmptyRow();
            //Link(new List<MpIView>() { ExcludedAppListPanel });
        }

        
        public override void Update() {
            //settings form size
            Size sfs = ((MpSettingsFormController)Find("MpSettingsFormController")).SettingsForm.Size;
            //excluded app panel height
            int eaph = ExcludedAppPanelControllerList[0].ExcludedAppPanel.Size.Height;
            //excluded app list panel height
            int ealph = ExcludedAppPanelControllerList.Count * eaph; 

            foreach(MpExcludedAppPanelController eapc in ExcludedAppPanelControllerList) {
                eapc.Update();
            }
        }
        public void RemoveExcludedApp(MpExcludedAppPanelController appControllerToRemove) {
            if(appControllerToRemove == null || !ExcludedAppPanelControllerList.Contains(appControllerToRemove)) {
                Console.WriteLine("MpExcludedAppListPanelController Error removing app from exclsion list because app is null or nnot in list");
                return;
            }

            ExcludedAppListPanel.Controls.Remove(appControllerToRemove.ExcludedAppPanel);
            ExcludedAppPanelControllerList.Remove(appControllerToRemove);

            appControllerToRemove.ExcludedApp.IsAppRejected = false;
            appControllerToRemove.ExcludedApp.WriteToDatabase();
            
            appControllerToRemove.Dispose();

            Update();
        }
            
        public void AddEmptyRow() {
            foreach(MpExcludedAppPanelController eapc in ExcludedAppPanelControllerList) {
                if(eapc.IsNew) {
                    Console.WriteLine("MpExcludedAppListPanelController error, trying to add multiple new excluded row so ignoring new one");
                    return;
                }
            }
            Color newRowColor = ExcludedAppPanelControllerList.Count % 2 == 0 ? Properties.Settings.Default.ExcludedAppRowColor1 : Properties.Settings.Default.ExcludedAppRowColor2;
            MpExcludedAppPanelController neac = new MpExcludedAppPanelController(this, null,newRowColor);
            ExcludedAppPanelControllerList.Add(neac);
            ExcludedAppListPanel.Controls.Add(neac.ExcludedAppPanel);

            Update();
        }
    }
}
