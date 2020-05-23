using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessLibrary;

namespace MonkeyPaste {

    public class MpAppManager  {
        private static readonly Lazy<MpAppManager> lazy = new Lazy<MpAppManager>(() => new MpAppManager());
        public static MpAppManager Instance { get { return lazy.Value; } }

        public MpTaskbarIconController TaskbarController { get; set; } = null;

        // TODO Add NetworkController to gather all db init parameters

        public MpDataModel DataModel { get; set; } = null;
        
        public MpAppManager() {}

        public void InitDb() {
            DataModel = new MpDataModel();
            DataModel.ConnectToDatabase(
                (string)MpRegistryHelper.Instance.GetValue("DBPath"),
                (string)MpRegistryHelper.Instance.GetValue("DBPassword"),
                null,
                null
            );
        }
        public void InitUI() {
            TaskbarController = new MpTaskbarIconController();
        }

        public void Init() {
            // TODO Init NetworkController here for dbpath/dbpass etc.
            InitDb();
            InitUI();
        }
    }
}
