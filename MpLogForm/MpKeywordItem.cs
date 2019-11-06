using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MpLogForm {
    public partial class MpKeywordItem:MpRoundedPanel {
        public override string Text {
            get {
                return KeywordLabel.Text;
            }
            set {
                KeywordLabel.Text = value;
            }
        }
        public MpKeywordItem() {
            InitializeComponent();
            Radius = 10;
            Thickness = 5;
            BackColor = Color.Green;
            BorderColor = Color.White;
        }
    }
}
