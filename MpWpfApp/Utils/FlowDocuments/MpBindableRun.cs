using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace MpWpfApp {
    public class MpBindableRun : Run { 
        public static readonly DependencyProperty BoundTextProperty = 
            DependencyProperty.Register(
                "BoundText", 
                typeof(string), 
                typeof(MpBindableRun), 
                new PropertyMetadata(OnBoundTextChanged)); 
        
        public MpBindableRun() { 
            MpFlowDocumentHelpers.FixupDataContext(this); 
        } 
        
        private static void OnBoundTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) { 
            ((Run)d).Text = (string)e.NewValue; 
        } 
        public String BoundText { 
            get { 
                return (string)GetValue(BoundTextProperty); 
            } 
            set { 
                SetValue(BoundTextProperty, value); 
            } 
        } 
    }
}
