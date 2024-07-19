using ReactiveUI;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace iosKeyboardTest.iOS.KeyboardExt;

public class ViewModelBase_fallback : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null, [CallerFilePath] string path = null, [CallerMemberName] string memName = null, [CallerLineNumber] int line = 0) {
        if (PropertyChanged == null ||
            propertyName == null) {
            return;
        }
        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }
}
