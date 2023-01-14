
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvDataObjectItemViewModel : MpAvTransactionMessageViewModelBase, MpITransactionNodeViewModel {

        public override string LabelText => Format;
        public string Format { get; private set; }
        public object Data { get; private set; }
        public MpAvDataObjectItemViewModel(MpAvTransactionItemViewModelBase parent) : base(parent) { }

        public override async Task InitializeAsync(object jsonOrParsedFragment, MpITransactionNodeViewModel parentAnnotation) {
            IsBusy = true;
            await Task.Delay(1);
            ParentTreeItem = parentAnnotation;

            Format = string.Empty;
            Data = string.Empty;
            if(jsonOrParsedFragment is KeyValuePair<string,object> kvp) {
                Format = kvp.Key;
                Data = kvp.Value;
            } 
            IsBusy = false;
        }

    }
}
