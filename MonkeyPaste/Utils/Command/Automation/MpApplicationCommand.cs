using System.Windows.Input;

namespace MonkeyPaste {
    public class MpApplicationCommand {
        public object Tag { get; set; }
        public ICommand Command { get;  set; }
        public object CommandParameter { get; set; }
        public string NavigateUri { get; set; }
    }
}
