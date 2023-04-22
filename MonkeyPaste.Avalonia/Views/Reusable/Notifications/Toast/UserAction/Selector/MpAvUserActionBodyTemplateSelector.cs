using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;

namespace MonkeyPaste.Avalonia {
    public class MpAvUserActionBodyTemplateSelector : IDataTemplate {
        // This Dictionary should store our shapes. We mark this as [Content], so we can directly add elements to it later.
        [Content]
        public Dictionary<string, IDataTemplate> AvailableTemplates { get; } = new Dictionary<string, IDataTemplate>();


        Control ITemplate<object, Control>.Build(object param) {
            string key = "TextBodyTemplate";
            if (param is MpUserActionNotificationViewModel uanvm &&
                uanvm.Body is MpAvAnalyticItemPresetViewModel aipvm) {
                key = "ParameterCollectionTemplate";
                aipvm.ExecuteItems.ForEach(x => x.PropertyChanged += (s, e) => {
                    if (e.PropertyName == nameof(x.CurrentValue)) {
                        uanvm.CanSubmit = aipvm.Parent.CanExecuteAnalysis(aipvm.Parent.CurrentExecuteArgs);
                    }
                });
                uanvm.CanSubmit = aipvm.Parent.CanExecuteAnalysis(aipvm.Parent.CurrentExecuteArgs);
            }
            return AvailableTemplates[key].Build(param);
        }

        public bool Match(object data) {
            // Check if we can accept the provided data
            return true;
        }
    }
}
