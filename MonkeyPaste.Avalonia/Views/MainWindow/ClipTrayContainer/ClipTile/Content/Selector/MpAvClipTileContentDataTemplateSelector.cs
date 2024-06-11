using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using System.Collections.Generic;

namespace MonkeyPaste.Avalonia {
    public class MpAvClipTileContentDataTemplateSelector : IDataTemplate {
        [Content]
        public Dictionary<string, IDataTemplate> AvailableTemplates { get; } = new Dictionary<string, IDataTemplate>();

        Control ITemplate<object, Control>.Build(object param) {
            if (param is not MpAvClipTileViewModel ctvm) {
                return null;
            }
            string key;
            if (MpAvPrefViewModel.Instance.IsRichHtmlContentEnabled) {
#if SUGAR_WV
                key = "CompositeWebViewTemplate";
#else
                key = "ContentWebViewTemplate";
#endif
            } else {
                key = "CompositeWebViewTemplate";
            }
            return AvailableTemplates[key].Build(param);
        }

        public bool Match(object data) {
            return data is MpAvClipTileViewModel;
        }

    }
}
