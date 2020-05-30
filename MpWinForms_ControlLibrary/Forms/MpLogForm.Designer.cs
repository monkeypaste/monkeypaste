namespace MonkeyPaste_WindowsControlLibrary {
    partial class MpLogForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MpLogForm));
            this.TilePanel = new MonkeyPaste_WindowsControlLibrary.MpTilePanel();
            this.SuspendLayout();
            // 
            // TilePanel
            // 
            this.TilePanel.AppIcon = ((System.Drawing.Image)(resources.GetObject("TilePanel.AppIcon")));
            this.TilePanel.AppIconRect = new System.Drawing.Rectangle(25, 25, 100, 100);
            this.TilePanel.BackColor = System.Drawing.Color.Transparent;
            this.TilePanel.CopyItemCreatedDateTime = new System.DateTime(2020, 5, 26, 9, 51, 23, 253);
            this.TilePanel.CopyItemData = resources.GetString("TilePanel.CopyItemData");
            this.TilePanel.CornerRadius = 15;
            this.TilePanel.EdgeWidth = 0;
            this.TilePanel.FlatBorderColor = System.Drawing.Color.Transparent;
            this.TilePanel.GradientColor1 = System.Drawing.Color.WhiteSmoke;
            this.TilePanel.GradientColor2 = System.Drawing.Color.Honeydew;
            this.TilePanel.GradientMode = MonkeyPaste_WindowsControlLibrary.PanelGradientMode.Horizontal;
            this.TilePanel.Location = new System.Drawing.Point(495, 237);
            this.TilePanel.Name = "TilePanel";
            this.TilePanel.ShadowColor = System.Drawing.Color.DimGray;
            this.TilePanel.ShadowShift = 5;
            this.TilePanel.ShadowStyle = MonkeyPaste_WindowsControlLibrary.ShadowMode.Dropped;
            this.TilePanel.Size = new System.Drawing.Size(724, 556);
            this.TilePanel.Style = MonkeyPaste_WindowsControlLibrary.BevelStyle.Flat;
            this.TilePanel.TabIndex = 0;
            this.TilePanel.Title = "Test Title";
            this.TilePanel.TitleColor = System.Drawing.Color.White;
            this.TilePanel.TitleFont = new System.Drawing.Font("Berlin Sans FB", 28.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TilePanel.TitleRect = new System.Drawing.Rectangle(135, 30, 200, 55);
            this.TilePanel.TitleShadowColor = System.Drawing.Color.Black;
            this.TilePanel.TitleShadowOffset = new System.Drawing.Size(3, 3);
            this.TilePanel.Load += new System.EventHandler(this.TilePanel_Load);
            // 
            // MpLogForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(192F, 192F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.ClientSize = new System.Drawing.Size(1534, 931);
            this.ControlBox = false;
            this.Controls.Add(this.TilePanel);
            this.Name = "MpLogForm";
            this.ShowInTaskbar = false;
            this.Load += new System.EventHandler(this.MpLogForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private MpTilePanel mpClipCardPanel2;
        public MpTilePanel TilePanel;
    }
}