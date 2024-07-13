using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Rendering;
using Avalonia.VisualTree;
using System;
using System.Linq;
using System.Windows.Input;

namespace iosKeyboardTest
{
    public partial class KeyView : UserControl
    {
        public KeyViewModel BindingContext =>
            DataContext as KeyViewModel;
        public KeyView()
        {
            InitializeComponent();
        }

    }
}