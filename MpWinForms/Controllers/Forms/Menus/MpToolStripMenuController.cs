using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpMenuItemClickedEventArgs : EventArgs {
        public string ItemText { get; set; }
        public MpMenuItemClickedEventArgs(string itemText) { ItemText = itemText; }
    }
    public class MpToolStripMenuController : MpController {       

        public delegate void MenuItemClicked(object sender, MpMenuItemClickedEventArgs e);
        public event MenuItemClicked MenuItemClickedEvent;

        public MpToolStripMenuController(ToolStrip owner,MpController p) : base(p) {
            ToolStripMenuItem settingsSubMenu = new ToolStripMenuItem("&Settings");
            settingsSubMenu.Font = new Font(Properties.Settings.Default.LogFont, Properties.Settings.Default.LogPanelTileFontSize);

            ToolStripMenuItem fileSubMenu = new ToolStripMenuItem("&File");
            fileSubMenu.DropDownItems.Add(ToolStripMenuItemWithHandler("&Clear History", _menuItemEventHandler));
            fileSubMenu.DropDownItems.Add(ToolStripMenuItemWithHandler("&Set Password", _menuItemEventHandler));
            fileSubMenu.DropDownItems.Add(ToolStripMenuItemWithHandler("&Details", _menuItemEventHandler));
            fileSubMenu.DropDownItems.Add(ToolStripMenuItemWithHandler("&Show File Location", _menuItemEventHandler));
            settingsSubMenu.DropDownItems.Add(fileSubMenu);

            ToolStripMenuItem systemSubMenu = new ToolStripMenuItem("&System");
            systemSubMenu.DropDownItems.Add(ToolStripMenuItemWithHandler("&Load on login", _menuItemEventHandler));
            systemSubMenu.DropDownItems.Add(ToolStripMenuItemWithHandler("&Preferences", _menuItemEventHandler));
            settingsSubMenu.DropDownItems.Add(systemSubMenu);

            owner.Items.Add(ToolStripMenuItemWithHandler("&Pause", _menuItemEventHandler));
            owner.Items.Add(settingsSubMenu);
            owner.Items.Add(ToolStripMenuItemWithHandler("&Help/About", _menuItemEventHandler));
            owner.Items.Add(new ToolStripSeparator());
            owner.Items.Add(ToolStripMenuItemWithHandler("&Exit", _menuItemEventHandler));
        }
        private void _menuItemEventHandler(object itemTitle,EventArgs e) {
            MenuItemClickedEvent(this, new MpMenuItemClickedEventArgs((string)itemTitle));
        }

        public override void Update() {
            throw new NotImplementedException();
        }
        private ToolStripMenuItem ToolStripMenuItemWithHandler(string displayText, EventHandler eventHandler) {
            var item = new ToolStripMenuItem(displayText);
            if (eventHandler != null) {
                item.Click += eventHandler;
            }
            return item;
        }
    }
}
