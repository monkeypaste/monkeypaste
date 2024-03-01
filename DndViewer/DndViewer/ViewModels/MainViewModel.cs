using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DndViewer.ViewModels;

public class MainViewModel : ViewModelBase {
    public bool IsTextWrapped { get; set; }
    public ObservableCollection<FormatViewModel> Items { get; set; } = [];
    public FormatViewModel SelectedItem { get; set; }
    public MainViewModel() { }

    public void SetDataObject(IDataObject ido) {
        Items.Clear();
        var formats = ido.GetDataFormats();
        foreach (string format in formats) {
            object data = ido.Get(format);
            Items.Add(new() {
                FormatName = format,
                FormatData = data
            });
        }
    }
    public ICommand ToggleTextWrappingCommand => ReactiveCommand.Create(() => {
        IsTextWrapped = !IsTextWrapped;
        this.RaisePropertyChanged(nameof(IsTextWrapped));
    });

    public ICommand ReadClipboardCommand => ReactiveCommand.Create(async () => {
        if (Application.Current.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime cda ||
            cda.MainWindow is not Window w ||
            w.Clipboard is not { } cb) {
            return;
        }
        DataObject ido = new DataObject();
        var formats = await cb.GetFormatsAsync();
        foreach (var format in formats) {
            var data = await cb.GetDataAsync(format);
            ido.Set(format, data);
        }
        SetDataObject(ido);
    });


    public ICommand ClearClipboardCommand => ReactiveCommand.Create(async () => {
        if (Application.Current.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime cda ||
            cda.MainWindow is not Window w ||
            w.Clipboard is not { } cb) {
            return;
        }
        Items.Clear();
        await cb.ClearAsync();
    });

    public ICommand ExitApplicationCommand => ReactiveCommand.Create(async () => {
        if (Application.Current.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime cda) {
            return;
        }
        cda.Shutdown();
    });
}
public class FormatViewModel : ViewModelBase {
    public string FormatName { get; set; }
    public string FormatType =>
        FormatData == null ? "NULL" : FormatData.GetType().ToString();

    private object _formatData;
    public object FormatData {
        get => _formatData;
        set {
            _formatData = value;
            this.RaisePropertyChanged(nameof(FormatData));
            if (_formatData is IEnumerable<IStorageItem> sil) {
                FormatDataDisplayValue = string.Join(Environment.NewLine, sil.Select(x => x.TryGetLocalPath()));
                return;
            }
            if (_formatData is byte[] bytes) {
                if (FormatName == "PNG") {
                    FormatDataDisplayValue = Convert.ToBase64String(bytes);
                } else {
                    FormatDataDisplayValue = Encoding.Default.GetString(bytes).Replace("\0", string.Empty);
                    if (FormatName == "HTML Format") {
                        string test = Encoding.UTF8.GetString(bytes).Replace("\0", string.Empty);
                        string test2 = Encoding.Unicode.GetString(bytes).Replace("\0", string.Empty);
                        string test3 = Encoding.UTF32.GetString(bytes).Replace("\0", string.Empty);
                    }

                }
                return;
            }
            FormatDataDisplayValue = _formatData == null ? string.Empty : _formatData.ToString();
        }
    }

    public string FormatDataDisplayValue { get; private set; }
    public FormatViewModel() { }
}
