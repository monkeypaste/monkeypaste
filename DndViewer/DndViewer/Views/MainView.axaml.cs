using Avalonia.Controls;
using Avalonia.Input;
using DndViewer.ViewModels;

namespace DndViewer.Views;

public partial class MainView : UserControl {
    public MainView() {
        InitializeComponent();
        lb.AddHandler(DragDrop.DropEvent, Drop);
    }
    void Drop(object sender, DragEventArgs e) {
        var dc = DataContext as MainViewModel;
        dc.SetDataObject(e.Data);
    }
}
