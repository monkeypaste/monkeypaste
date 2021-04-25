using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;

namespace MpWpfApp {
    [ContentProperty("Content")] 
    public class MpFragment : FrameworkElement { 
        private static readonly DependencyProperty ContentProperty = 
            DependencyProperty.Register(
                "Content", 
                typeof(FrameworkContentElement), 
                typeof(MpFragment)); 
        
        public FrameworkContentElement Content { 
            get { 
                return (FrameworkContentElement)GetValue(ContentProperty); 
            } 
            set { 
                SetValue(ContentProperty, value); 
            } 
        } 
    }
}
