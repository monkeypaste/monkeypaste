using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpMessageBoxViewModel : MpViewModelBase<object> {
        #region Propeties

        public string Title { get; set; } = "Title";

        public string IconPath { get; set; }

        public string Caption { get; set; } = "Caption";

        public ObservableCollection<string> ButtonLabels { get; set; } = new ObservableCollection<string>();

        public string Result { get; set; }

        public string Button1 {
            get {
                if(ButtonLabels.Count > 0) {
                    return ButtonLabels[0];
                }
                return nameof(Button1);
            }
        }

        public string Button2 {
            get {
                if (ButtonLabels.Count > 1) {
                    return ButtonLabels[1];
                }
                return nameof(Button2);
            }
        }

        public string Button3 {
            get {
                if (ButtonLabels.Count > 2) {
                    return ButtonLabels[2];
                }
                return nameof(Button3);
            }
        }
        #endregion

        public MpMessageBoxViewModel() : base(null) {
        }
    }
}
