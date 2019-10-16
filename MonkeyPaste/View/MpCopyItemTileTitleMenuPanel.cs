using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpCopyItemTileTitleMenuPanel : MpRoundedPanel {
        private System.Windows.Forms.Label editLabel;

        private void InitializeComponent() {
            this.editLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // editLabel
            // 
            this.editLabel.AutoSize = true;
            this.editLabel.Location = new System.Drawing.Point(0, 0);
            this.editLabel.Name = "editLabel";
            this.editLabel.Size = new System.Drawing.Size(100, 23);
            this.editLabel.TabIndex = 0;
            this.editLabel.Text = "Edit";
            this.editLabel.Click += new System.EventHandler(this.editLabel_Click);
            this.ResumeLayout(false);

        }

        private void editLabel_Click(object sender,EventArgs e) {

        }
    }
}
