using System.Collections.Generic;

namespace MonkeyPaste.Avalonia {
    public class MpAvWelcomeOptionGroupViewModel : MpAvViewModelBase<MpAvWelcomeNotificationViewModel> {
        public IList<MpAvWelcomeOptionItemViewModel> Items { get; set; }

        public MpWelcomePageType WelcomePageType { get; private set; }
        public string Title { get; set; }
        public string Caption { get; set; }
        public object SplashIconSourceObj { get; set; }
        public bool WasVisited { get; set; }

        public MpAvWelcomeOptionGroupViewModel() : this(null) { }
        public MpAvWelcomeOptionGroupViewModel(MpAvWelcomeNotificationViewModel parent) : base(parent) { }
        public MpAvWelcomeOptionGroupViewModel(MpAvWelcomeNotificationViewModel parent, MpWelcomePageType pageType) : this(parent) {
            WelcomePageType = pageType;
        }
    }
}
