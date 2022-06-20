using MonkeyPaste;

namespace MpWpfApp {
    public class MpContactFieldInfoViewModel : MpViewModelBase, MpITooltipInfoViewModel {
        public object Tooltip => "Contact templates allow you to automate common boilerplate cases that may arise. For example, in an email header writing 'Dear Rebecca,' Rebecca can be defined as a 'First Name' contact template. Then when pasting into your email you simply select the contact and the first name will be added automatically. *If no data for that contact's field is available, the template is left empty :(";
    }
}
