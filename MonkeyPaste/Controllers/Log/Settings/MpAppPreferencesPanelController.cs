using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpAppPreferencesPanelController : MpController{
        public MpLoadOnLoginCheckBoxController LoadOnLoginCheckBoxController { get; set; }

        public MpAppPreferencesPanel AppPreferencesPanel { get; set; }

        public MpAppPreferencesPanelController(MpController p) : base(p) {
            AppPreferencesPanel = new MpAppPreferencesPanel()
            {
                AutoSize = false
            };

            LoadOnLoginCheckBoxController = new MpLoadOnLoginCheckBoxController(this);
            AppPreferencesPanel.Controls.Add(LoadOnLoginCheckBoxController.LoadOnStartUpCheckbox);

            Link(new List<MpIView>() { AppPreferencesPanel });
            Update();
        }

        public override void Update() {
            LoadOnLoginCheckBoxController.Update();
        }
    }
}
