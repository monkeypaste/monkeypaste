using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using System.Collections.Generic;

namespace MonkeyPaste.Avalonia {
    public class MpAvUserTemplateSelector : IDataTemplate {
        [Content]
        public Dictionary<string, IDataTemplate> AvailableTemplates { get; } = new Dictionary<string, IDataTemplate>();

        Control ITemplate<object, Control>.Build(object param) {
            if (param is not MpUserPageType upt) {
                return null;
            }
            string key = $"{upt}Template";
            if (AvailableTemplates[key].Build(param) is not Control c) {
                return null;
            }
            if (upt == MpUserPageType.Register) {
                c.DataContext = MpAvAccountViewModel.Instance.RegistrationViewModel;
            } else if (upt == MpUserPageType.Login) {
                c.DataContext = MpAvAccountViewModel.Instance.LoginViewModel;
            } else {
                c.DataContext = MpAvAccountViewModel.Instance;
            }
            return c;
        }

        public bool Match(object data) {
            return true;
        }

    }
}
