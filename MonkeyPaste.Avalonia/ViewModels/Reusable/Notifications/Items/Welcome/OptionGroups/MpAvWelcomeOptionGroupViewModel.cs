using System.Collections.Generic;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvWelcomeOptionGroupViewModel : MpAvViewModelBase {
        public IList<MpAvWelcomeOptionItemViewModel> Items { get; set; }

        public MpWelcomePageType WelcomePageType { get; private set; }
        public string Title { get; set; }
        public string Caption { get; set; }
        public object SplashIconSourceObj { get; set; }

        public MpAvWelcomeOptionGroupViewModel() : base() { }
        public MpAvWelcomeOptionGroupViewModel(MpWelcomePageType pageType) : this() {
            WelcomePageType = pageType;
        }
    }
}
