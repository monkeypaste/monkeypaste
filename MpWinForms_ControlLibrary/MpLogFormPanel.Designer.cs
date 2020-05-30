namespace MonkeyPaste_WindowsControlLibrary {
    partial class MpLogFormPanel {
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MpLogFormPanel));
            this.mpClipCardPanel2 = new MonkeyPaste_WindowsControlLibrary.MpTilePanel();
            this.SuspendLayout();
            // 
            // mpClipCardPanel2
            // 
            this.mpClipCardPanel2.AppIcon = ((System.Drawing.Image)(resources.GetObject("mpClipCardPanel2.AppIcon")));
            this.mpClipCardPanel2.AppIconRect = new System.Drawing.Rectangle(650, 20, 128, 128);
            this.mpClipCardPanel2.CopyItemCreatedDateTime = new System.DateTime(2020, 5, 26, 2, 33, 47, 701);
            this.mpClipCardPanel2.CopyItemData = resources.GetString("mpClipCardPanel2.CopyItemData");
            this.mpClipCardPanel2.CornerRadius = 8;
            this.mpClipCardPanel2.EdgeWidth = 2;
            this.mpClipCardPanel2.FlatBorderColor = System.Drawing.Color.Transparent;
            this.mpClipCardPanel2.GradientColor1 = System.Drawing.Color.Honeydew;
            this.mpClipCardPanel2.GradientColor2 = System.Drawing.Color.SeaShell;
            this.mpClipCardPanel2.GradientMode = MonkeyPaste_WindowsControlLibrary.PanelGradientMode.Vertical;
            this.mpClipCardPanel2.Location = new System.Drawing.Point(434, 279);
            this.mpClipCardPanel2.Name = "mpClipCardPanel2";
            this.mpClipCardPanel2.ShadowColor = System.Drawing.Color.DimGray;
            this.mpClipCardPanel2.ShadowShift = 12;
            this.mpClipCardPanel2.ShadowStyle = MonkeyPaste_WindowsControlLibrary.ShadowMode.ForwardDiagonal;
            this.mpClipCardPanel2.Size = new System.Drawing.Size(812, 636);
            this.mpClipCardPanel2.Style = MonkeyPaste_WindowsControlLibrary.BevelStyle.Flat;
            this.mpClipCardPanel2.TabIndex = 3;
            this.mpClipCardPanel2.Title = "Test Title";
            this.mpClipCardPanel2.TitleColor = System.Drawing.Color.White;
            this.mpClipCardPanel2.TitleFont = new System.Drawing.Font("Calibri", 28.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.mpClipCardPanel2.TitleRect = new System.Drawing.Rectangle(30, 20, 300, 200);
            this.mpClipCardPanel2.TitleShadowColor = System.Drawing.Color.Black;
            this.mpClipCardPanel2.TitleShadowOffset = new System.Drawing.Size(3, 3);
            // 
            // MpLogFormPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.mpClipCardPanel2);
            this.Name = "MpLogFormPanel";
            this.Size = new System.Drawing.Size(1680, 1050);
            this.ResumeLayout(false);

        }

        #endregion

        private MpTilePanel mpClipCardPanel2;
    }
}
