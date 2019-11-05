using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpFileListItemRowPanelController : MpController {
        private int _rowId = -1;

        private Panel _fileListItemRowPanel;
        private PictureBox _iconPictureBox;
        private Label _pathTextBox;

        public MpFileListItemRowPanelController(int rowId, string path,MpController Parent) : base(Parent) {
            _rowId = rowId;

            _fileListItemRowPanel = new FlowLayoutPanel() {
                FlowDirection = FlowDirection.LeftToRight,
                BorderStyle = BorderStyle.FixedSingle,
                Anchor = AnchorStyles.Top                
            };

            _iconPictureBox = new PictureBox() {
                Anchor = AnchorStyles.Left,
                Location = Point.Empty                
            };
            FileAttributes attr = File.GetAttributes(path);
            if((attr & FileAttributes.Directory) == FileAttributes.Directory) {
                _iconPictureBox.Image = IconReader.GetFolderIcon(IconReader.IconSize.Large,IconReader.FolderType.Closed).ToBitmap();
            }
            else {
                _iconPictureBox.Image = IconReader.GetFileIcon(path,IconReader.IconSize.Large,false).ToBitmap();
            }
            _iconPictureBox.SetBounds(0,0,_iconPictureBox.Image.Width,_iconPictureBox.Image.Height);

            _fileListItemRowPanel.Controls.Add(_iconPictureBox);
            float fontSize = Properties.Settings.Default.LogPanelTileFontSize;
            float w = fontSize * Path.GetFileName(path).Length;
            _fileListItemRowPanel.Bounds = new Rectangle(0,0,(int)w,_iconPictureBox.Height);

            _pathTextBox = new Label() {
                Font = new Font(Properties.Settings.Default.LogFont,fontSize),
                //ReadOnly = true,
                Text = Path.GetFileName(path),
                BackColor = (_rowId + 1) % 2 == 0 ? Color.AliceBlue : Color.Bisque,
                Bounds = new Rectangle(_iconPictureBox.Width,0,_fileListItemRowPanel.Width,_iconPictureBox.Height),
                //Dock = DockStyle.Fill,                
                //AutoSize = true
                //BorderStyle = BorderStyle.None,
                //WordWrap = true                
            };
            _fileListItemRowPanel.Controls.Add(_pathTextBox);

            //_fileListItemRowPanel.SetBounds(0,0,_fileListItemRowPanel.Width,_iconPictureBox.Height);
        }
        
        public Panel GetFileListItemRowPanel() {
            return _fileListItemRowPanel;
        }
        public Label GetPathTextBox() {
            return _pathTextBox;
        }
        public PictureBox GetIconPictureBox() {
            return _iconPictureBox;
        }

        public override void Update() {
            throw new NotImplementedException();
        }

        protected override void View_KeyPress(object sender,KeyPressEventArgs e) {
            base.View_KeyPress(sender,e);
        }
    }
}
