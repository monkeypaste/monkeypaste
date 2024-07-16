using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Rendering;
using Avalonia.VisualTree;

using System;
using System.Linq;

namespace iosKeyboardTest
{

    public partial class KeyboardGridView : UserControl {
        public static Canvas DebugCanvas { get; private set; }

        public KeyboardViewModel BindingContext =>
            DataContext as KeyboardViewModel;

        public KeyboardGridView() 
        {
            InitializeComponent();
            DebugCanvas = this.DebugCanvasOverlay;
        }
    }
}