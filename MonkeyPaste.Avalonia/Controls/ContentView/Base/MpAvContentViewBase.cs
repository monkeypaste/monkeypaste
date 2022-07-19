using Avalonia.Controls;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public interface MpAvIContentView {        
        Control ContentControl { get; }
        string ContentData { get; set; }

        void SetContent(string content);
    }
    public abstract class MpAvContentViewBase : MpAvIContentView {
        public abstract Control ContentControl { get; }

        private string _contentData;
        public string ContentData {
            get => _contentData;
            set {
                _contentData = value;
                Dispatcher.UIThread.Post(async () => {
                    while(ContentControl == null) {
                        await Task.Delay(100);
                    }
                    while(!ContentControl.IsInitialized) {
                        await Task.Delay(100);
                    }
                    SetContent(ContentData);
                });
            }
        }

        public abstract void SetContent(string content);

    }
}
